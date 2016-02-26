using Newtonsoft.Json.Linq;
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

namespace WTalk
{
    public class Client
    { 
        Dictionary<string, string> _initParams = new Dictionary<string, string>();        
        Channel _channel;
        public static CookieContainer CookieContainer = new CookieContainer();
        HttpClient _client;

        string _api_key;
        string _email;
        string _header_date;
        string _header_version;
        string _header_id;
        string _client_id;

        bool _wasConnected = false;

        double _timestamp = 0;
        public Entity CurrentUser { get; private set; }

        internal List<string> _contact_ids = new List<string>();
        internal List<string> _active_conversation_ids = new List<string>();
        
        #region events
        public event EventHandler<List<ConversationState>> ConversationHistoryLoaded;
        public event EventHandler<List<Entity>> ContactListLoaded;
        public event EventHandler ConnectionEstablished;
        public event EventHandler<ConversationState> NewConversationCreated;
        public event EventHandler<Event> NewConversationEventReceived;
        public event EventHandler<Entity> UserInformationReceived;
        public event EventHandler<Entity> ContactInformationReceived;

        public event EventHandler<PresenceResult> PresenceInformationReceived;
        #endregion

        private DateTime _last_response_date = DateTime.UtcNow;

        public Client()
        {

            //_initParams.Add("prop", "StartPage");
            _initParams.Add("client", "sm");
            _initParams.Add("stime", DateTime.Now.TimeIntervalSince1970().TotalSeconds.ToString("0"));
            _initParams.Add("nav", "true");
            _initParams.Add("prop", "ChromeApp");
            _initParams.Add("fid", "gtn-roster-iframe-id");
            _initParams.Add("ec", "[\"ci:ec\",true,true,false]");
            _initParams.Add("pvt", "");

            
            _channel = new Channel();

            _channel.OnDataReceived += _channel_OnDataReceived;

            _client = new HttpClient(new SigningMessageHandler());            
            _client.Timeout = new TimeSpan(0, 0, 30);
            
        }
        
        public void Connect()
        {
            initializeChat();

            _channel.setAppVer(_header_version);

                Task.Run(() =>
                {
                    _channel.Listen();
                });
            
            
        }

        void initializeChat()
        {
            
            // We first need to fetch the 'pvt' token, which is required for the
            // initialization request (otherwise it will return 400).
            HttpResponseMessage message = _client.Execute(HangoutUri.PVT_TOKEN_URL, _initParams);
            string data = message.Content.ReadAsStringAsync().Result;
            Newtonsoft.Json.Linq.JArray array = Parser.ParseData(data);
            
            _initParams["pvt"] = array[1].ToString();

            // Now make the actual initialization request:
            message = _client.Execute(HangoutUri.CHAT_INIT_URL, _initParams);
            data = message.Content.ReadAsStringAsync().Result;

            // Parse the response by using a regex to find all the JS objects, and
            // parsing them. Not everything will be parsable, but we don't care if
            // an object we don't need can't be parsed.

            Dictionary<string, Newtonsoft.Json.Linq.JArray> dataDictionary = Parser.ParseInitParams(data);

            //this._api_key = dataDictionary["ds:7"][0][2].ToString(); // cin:cac
            //this._email = dataDictionary["ds:31"][0][2].ToString(); // cic:vd
            //this._header_date = dataDictionary["ds:2"][0][4].ToString(); //cin:acc
            //this._header_version = dataDictionary["ds:2"][0][6].ToString();
            //this._header_id = dataDictionary["ds:4"][0][7].ToString(); // cin:bcsc



            // a this time we can not fix key as index can change.
            string key = null;
            foreach(JArray jarray in dataDictionary.Values)
            {
                try
                {
                    key = jarray[0][0].ToString();
                }
                catch { key = null; }

                switch (key)
                {
                    case "cin:cac":
                        this._api_key = jarray[0][2].ToString();
                        break;
                    case "cic:vd":
                        this._email = jarray[0][2].ToString(); // cic:vd
                        break;
                    case "cin:acc":
                        if (jarray[0].Count() > 6)
                        {
                            this._header_date = jarray[0][4].ToString(); //cin:acc
                            this._header_version = jarray[0][6].ToString();
                        }
                        break;
                    case "cin:bcsc":
                        this._header_id = jarray[0][7].ToString(); // cin:bcsc
                        break;
                    case "cgserp":
                        jarray[0][0].Remove();
                        GetSuggestedEntitiesResponse cgserp = ProtoJsonSerializer.Deserialize<GetSuggestedEntitiesResponse>(jarray[0] as JArray);
                        if (cgserp.response_header.status == ResponseStatus.RESPONSE_STATUS_OK)
                        {
                            if(ContactListLoaded != null)
                                ContactListLoaded(this, cgserp.contacts_you_hangout_with.contact.Select(c => c.entity).ToList());
                            _contact_ids = cgserp.contacts_you_hangout_with.contact.Select(c => c.entity.id.chat_id).ToList();
                        }
                        break;
                    case "cgsirp":
                        jarray[0][0].Remove();
                        GetSelfInfoResponse cgsirp = ProtoJsonSerializer.Deserialize<GetSelfInfoResponse>(jarray[0] as JArray);
                        if (cgsirp.response_header.status == ResponseStatus.RESPONSE_STATUS_OK)
                        {
                            this.CurrentUser = cgsirp.self_entity;
                            if (!string.IsNullOrEmpty(_email))
                                this.CurrentUser.properties.canonical_email = _email;
                            if (UserInformationReceived != null)
                                UserInformationReceived(this, this.CurrentUser);
                        }
                        break;
                    case "csrcrp":
                        jarray[0][0].Remove();
                        SyncRecentConversationsResponse csrcrp = ProtoJsonSerializer.Deserialize<SyncRecentConversationsResponse>(jarray[0] as JArray);
                        if (csrcrp.response_header.status == ResponseStatus.RESPONSE_STATUS_OK)
                        {
                            if (ConversationHistoryLoaded != null)
                                ConversationHistoryLoaded(this, csrcrp.conversation_state);

                            _active_conversation_ids = csrcrp.conversation_state.Select(c => c.conversation_id.id).ToList() ;
                        }
                        break;

                }
            }


            //this._timestamp = double.Parse(dataDictionary["ds:21"][0][1][4].ToString());
            
        }


        void _channel_OnDataReceived(object sender, JArray rawdata)
        {               

            //Parse channel array and call the appropriate events.
            if (rawdata[0].ToString() == "noop")
            {
              // set active client if more than 120 sec of inactivity
                if ((DateTime.UtcNow - _last_response_date).TotalSeconds > 120)
                {
                    SetActiveClient();
                    _last_response_date = DateTime.UtcNow;
                }
            }
            else if (rawdata[0]["p"] != null)
            {
                JObject wrapper = JObject.Parse(rawdata[0]["p"].ToString());
                if (wrapper["3"] != null && wrapper["3"]["2"] != null)
                {
                    _client_id = wrapper["3"]["2"].ToString();
                    _requestHeader = null;

                    _channel.SendAck(0);

                    if (_channel.Connected && !_wasConnected)
                    {
                        _wasConnected = _channel.Connected;
                        if (ConnectionEstablished != null)
                            ConnectionEstablished(this, null);
                    }
                }

                if (wrapper["2"] != null && wrapper["2"]["2"] != null)
                {
                    JArray cbu = JArray.Parse(wrapper["2"]["2"].ToString());
                    cbu.RemoveAt(0);
                    BatchUpdate batchUpdate = ProtoJsonSerializer.Deserialize<BatchUpdate>(cbu as JArray);

                    foreach(StateUpdate state_update in batchUpdate.state_update)
                    {
                        
                        if(state_update.event_notification != null)
                            if (state_update.event_notification.current_event.event_type == EventType.EVENT_TYPE_REGULAR_CHAT_MESSAGE)
                            {
                                if (_active_conversation_ids.Contains(state_update.event_notification.current_event.conversation_id.id))
                                {
                                     if (NewConversationEventReceived != null)
                                        NewConversationEventReceived(this, state_update.event_notification.current_event);
                                }                                    
                                else
                                {
                                    if(NewConversationCreated != null)
                                    {
                                        ConversationState s = new ConversationState()
                                        {
                                            conversation_id = state_update.event_notification.current_event.conversation_id,
                                            conversation = state_update.conversation,
                                            events = new List<Event>(){ state_update.event_notification.current_event}
                                        };
                                        NewConversationCreated(this, s);
                                    }
                                }
                            }

                        if (state_update.presence_notification != null)
                            foreach (var presence in state_update.presence_notification.presence)
                                PresenceInformationReceived(this, presence);
                        
                        if(state_update.self_presence_notification != null)
                            PresenceInformationReceived(this, new PresenceResult()
                            {
                                user_id = CurrentUser.id,
                                presence = new Presence()
                                {
                                    available = state_update.self_presence_notification.client_presence_state.state == ClientPresenceStateType.CLIENT_PRESENCE_STATE_DESKTOP_ACTIVE
                                }
                            });

                    }



                    this._timestamp = long.Parse(wrapper["1"]["4"].ToString());
                }

            }           

           

        }


        #region API

        private RequestHeader _requestHeader;

        RequestHeader RequestHeaderBody
        {
            get
            {
                if(_requestHeader == null)
                {
                    _requestHeader = new RequestHeader();
                    _requestHeader.client_identifier = new ClientIdentifier() { header_id = _header_id, resource = _client_id};
                    _requestHeader.client_version = new ClientVersion() { client_id = ClientId.CLIENT_ID_WEB_GMAIL, 
                        build_type = ClientBuildType.BUILD_TYPE_PRODUCTION_APP, major_version = _header_version, version_timestamp = long.Parse(_header_date) };
                    _requestHeader.language_code = "en";

                }
                return _requestHeader;
            }
        } 
       
        long randomId
        {
            get
            { return long.Parse((DateTime.Now.TimeIntervalSince1970().Seconds * Math.Pow(2,32)).ToString("0")); }
        }

        public void QueryPresences()
        {
            QueryPresenceRequest request = new QueryPresenceRequest()
            {
                request_header = RequestHeaderBody,
                participant_id = this._contact_ids.Select(c => new ParticipantId() { chat_id = c }).ToList(),
                field_mask = new List<FieldMask>() { FieldMask.FIELD_MASK_AVAILABLE, FieldMask.FIELD_MASK_DEVICE, FieldMask.FIELD_MASK_REACHABLE }
            };

            HttpResponseMessage message = _client.PostProtoJson(_api_key, "presence/querypresence", request);
            
            if (PresenceInformationReceived != null)
            {
                QueryPresenceResponse response = message.Content.ReadAsProtoJson<QueryPresenceResponse>();

                foreach (var presence in response.presence_result)
                    PresenceInformationReceived(this, presence);
            }
        }

        public void SetActiveClient()
        {

            SetActiveClientRequest request = new SetActiveClientRequest()
            {
                request_header = RequestHeaderBody,
                full_jid = string.Format("{0}/{1}", _email, _client_id),
                is_active = true,
                timeout_secs = 120
            };
                        
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "clients/setactiveclient", request);            
        }

        public void SetPresence()
        {
            SetPresenceRequest request = new SetPresenceRequest()
            {
                request_header = RequestHeaderBody,
                presence_state_setting = new PresenceStateSetting() { type = ClientPresenceStateType.CLIENT_PRESENCE_STATE_DESKTOP_ACTIVE, timeout_secs = 720 },
                desktop_off_setting = new DesktopOffSetting() {  desktop_off = false }
            };

            
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "presence/setpresence", request);
            
            //GetSelfInfo();
        }

        public void SetFocus(string conversationId)
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
                        
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/setfocus", request);            
        }

        public void SetUserTyping(string conversationId)
        {
            SetTypingRequest request = new SetTypingRequest()
            {
                request_header = RequestHeaderBody,
                conversation_id = new ConversationId() {  id = conversationId },
                type = TypingType.TYPING_TYPE_STARTED
            };

            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/settyping", request);
            
        }
        
        public void SendChatMessage(string conversationId, string messageText)
        {
            SendChatMessageRequest request = new SendChatMessageRequest()
            {
                request_header = RequestHeaderBody,
                annotation = new List<EventAnnotation>(),
                message_content = new MessageContent() { attachment = new List<Attachment>(), segment = new List<Segment>() { new Segment() { text=messageText, type = SegmentType.SEGMENT_TYPE_TEXT, formatting = new Formatting() { bold = false, italic = false, strikethrough = false, underline = false }, link_data = new LinkData() } } },
                event_request_header = new EventRequestHeader { conversation_id = new ConversationId() { id = conversationId }, client_generated_id = randomId, expected_otr = OffTheRecordStatus.OFF_THE_RECORD_STATUS_ON_THE_RECORD, delivery_medium = new DeliveryMedium() { medium_type = DeliveryMediumType.DELIVERY_MEDIUM_BABEL }, event_type = EventType.EVENT_TYPE_REGULAR_CHAT_MESSAGE }


            };
            
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/sendchatmessage", request);
            
        }


        public void GetEntityById(params string[] chat_ids)
        {
            GetEntityByIdRequest request = new GetEntityByIdRequest()
            {
                request_header = RequestHeaderBody,
                batch_lookup_spec = chat_ids.Select(c => new EntityLookupSpec() { gaia_id = c }).ToList(),
                field_mask = new List<FieldMask>() { FieldMask.FIELD_MASK_AVAILABLE, FieldMask.FIELD_MASK_DEVICE, FieldMask.FIELD_MASK_REACHABLE }
            };

            HttpResponseMessage message = _client.PostProtoJson(_api_key, "contacts/getentitybyid", request);
            
            if (ContactInformationReceived != null)
            {
                GetEntityByIdResponse response = message.Content.ReadAsProtoJson<GetEntityByIdResponse>();

                foreach (var contact in response.entity.Where(c=>c.id != null))
                {   
                    _contact_ids.Add(contact.id.chat_id);
                    ContactInformationReceived(this, contact);
                }

                QueryPresences();
            }
        }

        public void GetSelfInfo()
        {
            GetSelfInfoRequest request = new GetSelfInfoRequest()
            {
                request_header = RequestHeaderBody
            };
            
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "contacts/getselfinfo", request);
            

            if (UserInformationReceived != null)
            {
                GetSelfInfoResponse response = message.Content.ReadAsProtoJson<GetSelfInfoResponse>();

                CurrentUser = response.self_entity;
                UserInformationReceived(this,CurrentUser);
            }
        }

        public void SyncRecentConversations()
        {
            SyncRecentConversationsRequest request = new SyncRecentConversationsRequest()
            {
                request_header = RequestHeaderBody
            };

            
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/syncrecentconversations", request);
            
            if (ConversationHistoryLoaded != null)
            {   
                SyncRecentConversationsResponse response = message.Content.ReadAsProtoJson<SyncRecentConversationsResponse>();

                ConversationHistoryLoaded(this, response.conversation_state);
                _active_conversation_ids = response.conversation_state.Select(c => c.conversation_id.id).ToList();
            }
        }

        public void SyncAllNewEvents()
        {

        }



        #endregion
    }
}
