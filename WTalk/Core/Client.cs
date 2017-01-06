using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WTalk.Model;
using WTalk.Core.HttpHandler;
using WTalk.Core.Utils;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Core.ProtoJson;
using System.Threading;
using coreJson;
using System.Collections;

namespace WTalk
{
    public class Client
    { 
        Dictionary<string, string> _initParams = new Dictionary<string, string>();        
        Channel _channel;
        
        static CookieContainer _cookieContainer;        
        public static CookieContainer CookieContainer
        {
            get
            {
                if (_cookieContainer == null)
                    _cookieContainer = new CookieContainer();
                return _cookieContainer;
            }            
        }


        public static SynchronizationContext CurrentSynchronizationContext { get; private set; }
        HttpClient _client;
        string _api_key;
        string _email;
        string _header_date;
        string _header_version;
        string _header_id;
        string _client_id;

        bool _wasConnected = false;

        DateTime _last_response_date = DateTime.UtcNow;
        double _timestamp = 0;

        
        public User CurrentUser { get; private set; }
        Dictionary<string, User> _contacts = new Dictionary<string, User>();
        Dictionary<string, WTalk.Model.Conversation> _active_conversations = new Dictionary<string, Model.Conversation>();
        


        #region events
        public event EventHandler<List<WTalk.Model.Conversation>> ConversationHistoryLoaded;
        public event EventHandler<List<User>> ContactListLoaded;
        public event EventHandler ConnectionEstablished;
        public event EventHandler<WTalk.Model.Conversation> NewConversationCreated;
        public event EventHandler<WTalk.Model.Conversation> NewMessageReceived;
        public event EventHandler<User> UserInformationReceived;
        public event EventHandler<User> ContactInformationReceived;

        public event EventHandler<User> UserPresenceChanged;

        #endregion

        

        public Client()
        {
            _initParams.Add("prop", "StartPage");
            _initParams.Add("fc", "https://hangouts.google.com");
            //_initParams.Add("ec", "[\"ci:ec\",true,true,false]");
            _initParams.Add("pvt", "");

            
            _channel = new Channel();

            _channel.OnDataReceived += _channel_OnDataReceived;

            _client = new HttpClient(new SigningMessageHandler());            
            _client.Timeout = new TimeSpan(0, 0, 30);
            
            // initializing shared static             
            CurrentSynchronizationContext = SynchronizationContext.Current;


        }

        public async Task ConnectAsync()
        {   
            await initializeChat().ContinueWith(t=>  {
                _channel.setAppVer(_header_version);                                
                _channel.Listen();
            });
            

        }

        async Task initializeChat()
        {

            // We first need to fetch the 'pvt' token, which is required for the
            // initialization request (otherwise it will return 400).
            HttpResponseMessage message = await _client.Execute(HangoutUri.PVT_TOKEN_URL, _initParams);
            string data = await message.Content.ReadAsStringAsync();
            List<object> array = JSON.ParseAsList(data);

            _initParams["pvt"] = array[1].ToString();
            _email = array[3].ToString();

           // Now make the actual initialization request:
           message = await _client.Execute(HangoutUri.CHAT_INIT_URL, _initParams);
            data = await message.Content.ReadAsStringAsync();
            message.Dispose();


            // Parse the response by using a regex to find all the JS objects, and
            // parsing them. Not everything will be parsable, but we don't care if
            // an object we don't need can't be parsed.
            
            IEnumerable<DynamicJson> dataDictionary = Parser.ParseInitParams(data); 
            
            //// a this time we can not fix key as index can change.
            string key = null;
            foreach (DynamicJson jarray in dataDictionary)
            {
                if (jarray.Count > 0 && jarray[0].Count > 0 && !string.IsNullOrEmpty(jarray[0][0].ToString()))
                    key = jarray[0][0].ToString();
                else
                    continue;

                switch (key)
                {
                    case "cin:cac":
                        _api_key = jarray[0][2].ToString();
                        break;
                    case "cic:vd":
                        _email = jarray[0][2].ToString(); // cic:vd
                        break;
                    case "cin:acc":
                        if (jarray[0].Count > 6)
                        {
                            _header_date = jarray[0][4].ToString(); //cin:acc
                            _header_version = jarray[0][6].ToString();
                        }
                        break;
                    case "cin:bcsc":
                        if (jarray[0].Count > 7)
                            _header_id = jarray[0][7].ToString(); // cin:bcsc
                        break;
                        #region loaded on connection
                        //case "cgserp":
                        //    jarray[0].RemoveAt(0);
                        //    GetSuggestedEntitiesResponse cgserp = ProtoJsonSerializer.Deserialize<GetSuggestedEntitiesResponse>(jarray[0]);
                        //    if (cgserp.response_header.status == ResponseStatus.RESPONSE_STATUS_OK)
                        //    {
                        //        _contacts = cgserp.contacts_you_hangout_with.contact.ToDictionary(c => c.entity.id.gaia_id, c => new User(c.entity));
                        //    }
                        //    break;
                        //case "cgsirp":
                        //    jarray[0].RemoveAt(0);
                        //    GetSelfInfoResponse cgsirp = ProtoJsonSerializer.Deserialize<GetSelfInfoResponse>(jarray[0]);
                        //    if (cgsirp.response_header.status == ResponseStatus.RESPONSE_STATUS_OK)
                        //    {
                        //        if (!string.IsNullOrEmpty(_email))
                        //            cgsirp.self_entity.properties.canonical_email = _email;
                        //        CurrentUser = new User(cgsirp.self_entity);                                
                        //    }
                        //    break;
                        //case "csrcrp":
                        //    jarray[0].RemoveAt(0);
                        //    SyncRecentConversationsResponse csrcrp = ProtoJsonSerializer.Deserialize<SyncRecentConversationsResponse>(jarray[0]);
                        //    if (csrcrp.response_header.status == ResponseStatus.RESPONSE_STATUS_OK)
                        //    {
                        //        _active_conversations = csrcrp.conversation_state.ToDictionary(c => c.conversation_id.id, c => new WTalk.Model.Conversation(c));
                        //    }
                        //    break;
                        #endregion

                }
            }

            data = null;
            dataDictionary = null;
        }


        async void _channel_OnDataReceived(object sender, DynamicJson rawdata)
        {               

            //Parse channel array and call the appropriate events.
            if (rawdata[0].ToString() == "noop")
            {
              // set active client if more than 120 sec of inactivity
                if ((DateTime.UtcNow - _last_response_date).TotalSeconds > 120 && _client_id != null)
                {                    
                    SetActiveClientAsync();
                    _last_response_date = DateTime.UtcNow;
                }
            }
            else if (rawdata[0].ToString() == "resync") // internal handling
            {
                SyncAllNewEventsAsync(long.Parse(rawdata[1].ToString()));
            }
            else if (rawdata[0]["p"] != null)
            {
                DynamicJson wrapper = new DynamicJson(rawdata[0]["p"].ToString());
                if (wrapper["4"] != null && wrapper["4"]["2"] != null)
                {
                    if (_channel.Connected && !_wasConnected)
                    {
                        _client_id = wrapper["4"]["2"].ToString();
                        _requestHeader = null;
                        _channel.SendAck(0);

                        _wasConnected = _channel.Connected;
                        
                        // load self info 
                        await GetSelfInfoAsync();

                        // load conversations
                        await SyncRecentConversationsAsync();


                        // check if contacts are loaded
                        if (_contacts == null || _contacts.Count == 0)
                        {
                            string[] participants_id = _active_conversations.Values.SelectMany(c => c._conversation.current_participant.Where(p => p.gaia_id != CurrentUser.Id).Select(p => p.gaia_id)).Distinct().ToArray();
                            await GetEntityByIdAsync(participants_id);                            
                        }

                        if (ConnectionEstablished != null)
                            ConnectionEstablished(this, null);
                    }
                }

                if (wrapper["2"] != null && wrapper["2"]["2"] != null)
                {
                    DynamicJson cbu = new DynamicJson(wrapper["2"]["2"].ToString());
                    cbu.RemoveAt(0);
                    BatchUpdate batchUpdate = ProtoJsonSerializer.Deserialize<BatchUpdate>(cbu as DynamicJson);

                    foreach(StateUpdate state_update in batchUpdate.state_update)
                    {

                        if (state_update.event_notification != null )
                        {
                            switch (state_update.event_notification.current_event.event_type)
                            {
                                case EventType.EVENT_TYPE_REGULAR_CHAT_MESSAGE:
                                case EventType.EVENT_TYPE_UNKNOWN:
                                    if (state_update.event_notification.current_event.conversation_id == null)
                                        break;

                                    if (_active_conversations.ContainsKey(state_update.event_notification.current_event.conversation_id.id))
                                    {
                                        _active_conversations[state_update.event_notification.current_event.conversation_id.id].AddNewMessage(state_update.event_notification.current_event);
                                        if (NewMessageReceived != null)
                                            NewMessageReceived(this, _active_conversations[state_update.event_notification.current_event.conversation_id.id]);
                                    }
                                    else
                                    {
                                        ConversationState s = new ConversationState()
                                        {
                                            conversation_id = state_update.event_notification.current_event.conversation_id,
                                            conversation = state_update.conversation,
                                            events = new List<Event>() { state_update.event_notification.current_event }
                                        };

                                        _active_conversations.Add(s.conversation_id.id, new Model.Conversation(s));
                                        if (NewConversationCreated != null)
                                            NewConversationCreated(this, _active_conversations.Last().Value);
                                    }
                                    break;
                                case EventType.EVENT_TYPE_OTR_MODIFICATION:
                                    _active_conversations[state_update.event_notification.current_event.conversation_id.id]._conversation.otr_status = state_update.event_notification.current_event.otr_status;
                                    break;
                                case EventType.EVENT_TYPE_CONVERSATION_RENAME:
                                    _active_conversations[state_update.event_notification.current_event.conversation_id.id]._conversation.name = state_update.event_notification.current_event.conversation_rename.new_name;
                                    break;
                            }
                        }

                        if (state_update.presence_notification != null)
                            foreach (var presence in state_update.presence_notification.presence)
                                if (_contacts.ContainsKey(presence.user_id.gaia_id))
                                    setPresence(_contacts[presence.user_id.gaia_id], presence.presence);
                                
                        
                        if(state_update.self_presence_notification != null)
                            CurrentUser.SetPresence(new Presence()
                                {
                                    available = state_update.self_presence_notification.client_presence_state.state == ClientPresenceStateType.CLIENT_PRESENCE_STATE_DESKTOP_ACTIVE
                                }
                            );

                        if(state_update.watermark_notification != null )
                        {
                            if (state_update.watermark_notification.sender_id.gaia_id == CurrentUser.Id)
                                _active_conversations[state_update.watermark_notification.conversation_id.id].SelfReadState = state_update.watermark_notification.latest_read_timestamp.FromUnixTime();
                            else
                                _active_conversations[state_update.watermark_notification.conversation_id.id].ReadState = state_update.watermark_notification.latest_read_timestamp.FromUnixTime();
                        }
                    }

                    _timestamp = long.Parse(wrapper["1"]["4"].ToString());
                }
            }    
        }

       

        #region internal helpers
        private void setPresence(User user, Presence presence)
        {
            user.SetPresence(presence);
            if (UserPresenceChanged != null)
                UserPresenceChanged(this, user);
        }
        #endregion

        #region LocalCache

        public async Task<User> GetContactFromCache(string id)
        {
            if (!_contacts.ContainsKey(id))
                await GetEntityByIdAsync(id);
            if (_contacts.ContainsKey(id))
                return _contacts[id];
            else
                return null;
        }

        #endregion

        #region API


        #region request header 
        private RequestHeader _requestHeader;

        RequestHeader RequestHeaderBody
        {
            get
            {
                if(_requestHeader == null)
                {
                    _requestHeader = new RequestHeader();
                    _requestHeader.client_identifier = new ClientIdentifier() { header_id = _header_id, resource = _client_id};
                    _requestHeader.client_version = new ClientVersion() { client_id = ClientId.CLIENT_ID_UNKNOWN, 
                    build_type = ClientBuildType.BUILD_TYPE_PRODUCTION_APP, major_version = _header_version , version_timestamp = long.Parse(_header_date) };
                    _requestHeader.language_code = "en";

                }
                return _requestHeader;
            }
        }
        static Random rnd = new Random();
        string random_id()
        {
            return rnd.Next(6544455, int.MaxValue).ToString();
        }

        #endregion

        #region presence
        public async Task QueryPresencesAsync()
        {
            QueryPresenceRequest request = new QueryPresenceRequest()
            {
                request_header = RequestHeaderBody,
                participant_id = _contacts.Keys.Select(c => new ParticipantId() { chat_id = c, gaia_id = c }).ToList(),
                field_mask = Enum.GetValues(typeof(FieldMask)).Cast<FieldMask>().ToList()
            };

            using (HttpResponseMessage message = await _client.PostProtoJson("presence/querypresence", _api_key, request))
            {
                QueryPresenceResponse response = await message.Content.ReadAsProtoJson<QueryPresenceResponse>();
                foreach (var presence in response.presence_result)
                    if (_contacts.ContainsKey(presence.user_id.gaia_id))
                        setPresence(_contacts[presence.user_id.gaia_id], presence.presence);
            }

        }

        public async Task QuerySelfPresenceAsync()
        {
            QueryPresenceRequest request = new QueryPresenceRequest()
            {
                request_header = RequestHeaderBody,
                participant_id = new List<ParticipantId>() { new ParticipantId() {chat_id= CurrentUser.Id,  gaia_id = CurrentUser.Id } },
                field_mask = Enum.GetValues(typeof(FieldMask)).Cast<FieldMask>().ToList()
            };

            using (HttpResponseMessage message = await _client.PostProtoJson("presence/querypresence", _api_key, request))
            {
                QueryPresenceResponse response = await message.Content.ReadAsProtoJson<QueryPresenceResponse>();

                foreach (var presence in response.presence_result)
                    setPresence(CurrentUser, presence.presence);
            }
        }
        

        public async Task SetPresenceAsync(int state = 40)
        {
            SetPresenceRequest request = new SetPresenceRequest()
            {
                request_header = RequestHeaderBody,
                presence_state_setting = new PresenceStateSetting() { type = (ClientPresenceStateType)state, timeout_secs = 720 }
            };


            HttpResponseMessage message = await _client.PostProtoJson("presence/setpresence", _api_key, request);
            message.Dispose();
        }

        public async Task SetActiveClientAsync(bool active = true)
        {

            SetActiveClientRequest request = new SetActiveClientRequest()
            {
                request_header = RequestHeaderBody,
                full_jid = string.Format("{0}/{1}", _email, _client_id),
                is_active = active,
                timeout_secs = 120,
                unknown = true
            };

            HttpResponseMessage message = await _client.PostProtoJson("clients/setactiveclient", _api_key, request);
            message.Dispose();
        }
        #endregion

        #region conversation

        public async Task SetFocusAsync(string conversationId)
        {
            if (conversationId.Length == 0)
                return;

            SetFocusRequest request = new SetFocusRequest()
            {
                request_header = RequestHeaderBody,
                conversation_id = new ConversationId() { id = conversationId },
                type = FocusType.FOCUS_TYPE_FOCUSED,
                timeout_secs = 20
            };

            HttpResponseMessage message = await _client.PostProtoJson("conversations/setfocus", _api_key, request);
            message.Dispose();
        }



        public async Task SetUserTypingAsync(string conversationId)
        {
            SetTypingRequest request = new SetTypingRequest()
            {
                request_header = RequestHeaderBody,
                conversation_id = new ConversationId() {  id = conversationId },
                type = TypingType.TYPING_TYPE_STARTED 
            };

            HttpResponseMessage message = await _client.PostProtoJson("conversations/settyping", _api_key, request);
            
        }

        public async Task SendChatMessageAsync(string conversationId, string messageText)
        {

            OffTheRecordStatus expected_otr = _active_conversations[conversationId]._conversation.otr_status;

            SendChatMessageRequest request = new SendChatMessageRequest()
            {
                request_header = RequestHeaderBody,
                annotation = new List<EventAnnotation>(),
                message_content = new MessageContent() { attachment = new List<Attachment>(), segment = new List<Segment>() { new Segment() { text=messageText, type = SegmentType.SEGMENT_TYPE_TEXT, formatting = new Formatting() { bold = false, italic = false, strikethrough = false, underline = false }} } },
                event_request_header = new EventRequestHeader { conversation_id = new ConversationId() { id = conversationId }, client_generated_id = random_id(), expected_otr = expected_otr, delivery_medium = new DeliveryMedium() { medium_type = DeliveryMediumType.DELIVERY_MEDIUM_BABEL }, event_type = EventType.EVENT_TYPE_REGULAR_CHAT_MESSAGE }
            };

            HttpResponseMessage message = await _client.PostProtoJson("conversations/sendchatmessage", _api_key, request);
            message.Dispose();
        }

        public async Task SyncRecentConversationsAsync()
        {
            SyncRecentConversationsRequest request = new SyncRecentConversationsRequest()
            {
                request_header = RequestHeaderBody                
            };


            using (HttpResponseMessage message = await _client.PostProtoJson("conversations/syncrecentconversations", _api_key, request))
            {

                if (ConversationHistoryLoaded != null)
                {
                    SyncRecentConversationsResponse response = await message.Content.ReadAsProtoJson<SyncRecentConversationsResponse>();
                    _active_conversations = response.conversation_state.ToDictionary(c => c.conversation_id.id, c => new WTalk.Model.Conversation(c));
                    ConversationHistoryLoaded(this, _active_conversations.Values.ToList());
                }
            }
        }

        public async Task ModifyOTRStatusAsync(string conversationId, bool enable)
        {
            ModifyOTRStatusRequest request = new ModifyOTRStatusRequest()
            {
                request_header = RequestHeaderBody,
                otr_status = enable ? OffTheRecordStatus.OFF_THE_RECORD_STATUS_ON_THE_RECORD : OffTheRecordStatus.OFF_THE_RECORD_STATUS_OFF_THE_RECORD,
                event_request_header = new EventRequestHeader()
                {
                    conversation_id = new ConversationId() { id = conversationId },
                    event_type = EventType.EVENT_TYPE_OTR_MODIFICATION,
                    client_generated_id = random_id(),
                    expected_otr = enable ? OffTheRecordStatus.OFF_THE_RECORD_STATUS_ON_THE_RECORD : OffTheRecordStatus.OFF_THE_RECORD_STATUS_OFF_THE_RECORD
                }
            };

            HttpResponseMessage message = await _client.PostProtoJson("conversations/modifyotrstatus", _api_key, request);
            message.Dispose();
        }

        public async Task UpdateWaterMarkAsync(string conversationId, DateTime last_read_state)
        {            
            UpdateWatermarkRequest request = new UpdateWatermarkRequest()
            {
                request_header = RequestHeaderBody,
                conversation_id = new ConversationId() {  id = conversationId },
                last_read_timestamp = last_read_state.ToUnixTime()
            };

            HttpResponseMessage message = await _client.PostProtoJson("conversations/updatewatermark", _api_key, request);
            message.Dispose();

        }

        public async Task GetConversationAsync(string conversationId)
        {
            var conv = _active_conversations[conversationId]._conversation;
            var evt = _active_conversations[conversationId].messagesIds.Values.Min();
            GetConversationRequest request = new GetConversationRequest()
            {
                request_header = RequestHeaderBody,
                include_event = true,
                max_events_per_conversation = 50,
                conversation_spec = new ConversationSpec() { conversation_id = conv.conversation_id },
                event_continuation_token = new EventContinuationToken() { event_id = null, storage_continuation_token=null, event_timestamp = evt }

            };

            using (HttpResponseMessage message = await _client.PostProtoJson("conversations/getconversation", _api_key, request))
            {
                GetConversationResponse response = await message.Content.ReadAsProtoJson<GetConversationResponse>();
                _active_conversations[response.conversation_state.conversation_id.id].AddOldMessages(response.conversation_state.events);
            }
            
        }

        public async Task SyncAllNewEventsAsync(long last_sync_timestamp)
        {
            SyncAllNewEventsRequest request = new SyncAllNewEventsRequest()
            {
                request_header = RequestHeaderBody,
                last_sync_timestamp = last_sync_timestamp * 1000,
                max_response_size_bytes = 1048576

            };

            using(HttpResponseMessage message = await _client.PostProtoJson("conversations/syncallnewevents", _api_key, request))
            {
                SyncAllNewEventsResponse response = await message.Content.ReadAsProtoJson<SyncAllNewEventsResponse>();
                foreach(var conversation in response.conversation_state)
                    _active_conversations[conversation.conversation_id.id].AddNewMessage(conversation.events);
                
                
            }            
        }


        #endregion

        #region contacts

        public async Task GetEntityByIdAsync(params string[] ids)
        {
            if (ids.Length == 0)
                return;

            GetEntityByIdRequest request = new GetEntityByIdRequest()
            {
                request_header = RequestHeaderBody,
                batch_lookup_spec = ids.Select(c => new EntityLookupSpec() { gaia_id = c }).ToList()
                //field_mask = new List<FieldMask>() { FieldMask.FIELD_MASK_AVAILABLE, FieldMask.FIELD_MASK_DEVICE, FieldMask.FIELD_MASK_REACHABLE }
            };

            using (HttpResponseMessage message = await _client.PostProtoJson("contacts/getentitybyid", _api_key, request))
            {


                GetEntityByIdResponse response = await message.Content.ReadAsProtoJson<GetEntityByIdResponse>();
                if (_contacts.Count == 0)
                {
                    _contacts = response.entity.Where(c => c.id != null).ToDictionary(c => c.id.gaia_id, c => new User(c));
                    if (ContactListLoaded != null)
                        ContactListLoaded(this, _contacts.Values.ToList());
                }
                else
                {

                    foreach (var contact in response.entity.Where(c => c.id != null))
                    {
                        if (_contacts.ContainsKey(contact.id.gaia_id))
                            _contacts[contact.id.gaia_id] = new User(contact);
                        else
                            _contacts.Add(contact.id.gaia_id, new User(contact));

                        if (ContactInformationReceived != null)
                            ContactInformationReceived(this, _contacts[contact.id.gaia_id]);
                    }
                }
                QueryPresencesAsync();
            }

        }

        public async Task GetSuggestedEntitiesAsync()
        {
            GetSuggestedEntitiesRequest request = new GetSuggestedEntitiesRequest()
            {
                request_header = RequestHeaderBody,

            };

            HttpResponseMessage message = await _client.PostProtoJson("contacts/getsuggestedentities", _api_key, request);
            message.Dispose();
        }

        #endregion

        public async Task GetSelfInfoAsync()
        {
            GetSelfInfoRequest request = new GetSelfInfoRequest()
            {
                request_header = RequestHeaderBody,
                
            };

            using (HttpResponseMessage message = await _client.PostProtoJson("contacts/getselfinfo", _api_key, request))
            {

                if (UserInformationReceived != null)
                {
                    GetSelfInfoResponse response = await message.Content.ReadAsProtoJson<GetSelfInfoResponse>();

                    CurrentUser = new User(response.self_entity);
                    UserInformationReceived(this, CurrentUser);
                }
            }
        }

        #endregion
    }
}
