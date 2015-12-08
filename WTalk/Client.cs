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
using WTalk.HttpHandler;
using WTalk.Utils;

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
        public WTalk.ProtoJson.Entity CurrentUser { get; private set; }

        internal List<string> _contact_ids = new List<string>();
        internal List<string> _active_conversation_ids = new List<string>();
        #region events
        public event EventHandler<List<WTalk.ProtoJson.ConversationState>> ConversationHistoryLoaded;
        public event EventHandler<List<WTalk.ProtoJson.Entity>> ContactListLoaded;
        public event EventHandler ConnectionEstablished;
        public event EventHandler<ProtoJson.ConversationState> NewConversationCreated;
        public event EventHandler<ProtoJson.Event> NewConversationEventReceived;
        public event EventHandler<ProtoJson.Entity> UserInformationReceived;
        public event EventHandler<ProtoJson.Entity> ContactInformationReceived;

        public event EventHandler<WTalk.ProtoJson.PresenceResult> PresenceInformationReceived;
        #endregion


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
                        ProtoJson.GetSuggestedEntitiesResponse cgserp = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.GetSuggestedEntitiesResponse>(jarray[0] as JArray);
                        if (cgserp.response_header.status == ProtoJson.ResponseStatus.RESPONSE_STATUS_OK)
                        {
                            if(ContactListLoaded != null)
                                ContactListLoaded(this, cgserp.contacts_you_hangout_with.contact.Select(c => c.entity).ToList());
                            _contact_ids = cgserp.contacts_you_hangout_with.contact.Select(c => c.entity.id.chat_id).ToList();
                        }
                        break;
                    case "cgsirp":
                        jarray[0][0].Remove();
                        ProtoJson.GetSelfInfoResponse cgsirp = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.GetSelfInfoResponse>(jarray[0] as JArray);
                        if (cgsirp.response_header.status == ProtoJson.ResponseStatus.RESPONSE_STATUS_OK)
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
                        ProtoJson.SyncRecentConversationsResponse csrcrp = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.SyncRecentConversationsResponse>(jarray[0] as JArray);
                        if (csrcrp.response_header.status == ProtoJson.ResponseStatus.RESPONSE_STATUS_OK)
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
                // nothing to do ?? 
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
                    ProtoJson.BatchUpdate batchUpdate = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.BatchUpdate>(cbu as JArray);

                    foreach(WTalk.ProtoJson.StateUpdate state_update in batchUpdate.state_update)
                    {
                        if(state_update.event_notification != null)
                            if (state_update.event_notification.current_event.event_type == ProtoJson.EventType.EVENT_TYPE_REGULAR_CHAT_MESSAGE)
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
                                        ProtoJson.ConversationState s = new ProtoJson.ConversationState()
                                        {
                                            conversation_id = state_update.event_notification.current_event.conversation_id,
                                            conversation = state_update.conversation,
                                            events = new List<ProtoJson.Event>(){ state_update.event_notification.current_event}
                                        };
                                        NewConversationCreated(this, s);
                                    }
                                }
                            }

                        if (state_update.presence_notification != null)
                            foreach (var presence in state_update.presence_notification.presence)
                            PresenceInformationReceived(this, presence);
                    }



                    this._timestamp = long.Parse(wrapper["1"]["4"].ToString());
                }

            }           

           

        }


        #region API

        private ProtoJson.RequestHeader _requestHeader;

        ProtoJson.RequestHeader RequestHeaderBody
        {
            get
            {
                if(_requestHeader == null)
                {
                    _requestHeader = new ProtoJson.RequestHeader();
                    _requestHeader.client_identifier = new ProtoJson.ClientIdentifier() { header_id = _header_id, resource = _client_id};
                    _requestHeader.client_version = new ProtoJson.ClientVersion() { client_id = ProtoJson.ClientId.CLIENT_ID_CHROME, 
                        build_type = ProtoJson.ClientBuildType.BUILD_TYPE_PRODUCTION_APP, major_version = _header_version, version_timestamp = long.Parse(_header_date) };
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
            ProtoJson.QueryPresenceRequest request = new ProtoJson.QueryPresenceRequest()
            {
                request_header = RequestHeaderBody,
                participant_id = this._contact_ids.Select(c => new ProtoJson.ParticipantId() { chat_id = c }).ToList(),
                field_mask = new List<ProtoJson.FieldMask>() { ProtoJson.FieldMask.FIELD_MASK_AVAILABLE, ProtoJson.FieldMask.FIELD_MASK_DEVICE, ProtoJson.FieldMask.FIELD_MASK_REACHABLE }
            };

            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "presence/querypresence", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;

            if (PresenceInformationReceived != null)
            {

                arrayBody = JArray.Parse(result);
                arrayBody.RemoveAt(0);
                ProtoJson.QueryPresenceResponse response = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.QueryPresenceResponse>(arrayBody);

                foreach (var presence in response.presence_result)
                    PresenceInformationReceived(this, presence);
            }
        }

        public void SetActiveClient()
        {

            ProtoJson.SetActiveClientRequest request = new ProtoJson.SetActiveClientRequest()
            {
                request_header = RequestHeaderBody,
                full_jid = string.Format("{0}/{1}", _email, _client_id),
                is_active = true,
                timeout_secs = 120
            };

            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "clients/setactiveclient", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;
        }

        public void SetPresence()
        {
            ProtoJson.SetPresenceRequest request = new ProtoJson.SetPresenceRequest()
            {
                request_header = RequestHeaderBody,
                presence_state_setting = new ProtoJson.PresenceStateSetting() {  type = ProtoJson.ClientPresenceStateType.CLIENT_PRESENCE_STATE_DESKTOP_ACTIVE, timeout_secs=720}
            };

            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);            
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "presence/setpresence", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;
        }

        public void SetFocus(string conversationId)
        {
            ProtoJson.SetFocusRequest request = new ProtoJson.SetFocusRequest()
            {
                request_header = RequestHeaderBody,
                conversation_id = new ProtoJson.ConversationId() { id = conversationId },
                type = ProtoJson.FocusType.FOCUS_TYPE_FOCUSED,
                timeout_secs = 20
            };

            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/setfocus", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;
        }

        public void SetUserTyping(string conversationId)
        {
            ProtoJson.SetTypingRequest request = new ProtoJson.SetTypingRequest()
            {
                request_header = RequestHeaderBody,
                conversation_id = new ProtoJson.ConversationId() {  id = conversationId },
                type = ProtoJson.TypingType.TYPING_TYPE_STARTED
            };

            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/settyping", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;
        }
        
        public void SendChatMessage(string conversationId, string messageText)
        {
            ProtoJson.SendChatMessageRequest request = new ProtoJson.SendChatMessageRequest()
            {
                request_header = RequestHeaderBody,
                annotation = new List<ProtoJson.EventAnnotation>(),
                message_content = new ProtoJson.MessageContent() { attachment = new List<ProtoJson.Attachment>(), segment = new List<ProtoJson.Segment>() { new ProtoJson.Segment() { text=messageText, type = ProtoJson.SegmentType.SEGMENT_TYPE_TEXT, formatting = new ProtoJson.Formatting() { bold = false, italic = false, strikethrough = false, underline = false }, link_data = new ProtoJson.LinkData() } } },
                event_request_header = new ProtoJson.EventRequestHeader { conversation_id = new ProtoJson.ConversationId() { id = conversationId }, client_generated_id = randomId, expected_otr = ProtoJson.OffTheRecordStatus.OFF_THE_RECORD_STATUS_ON_THE_RECORD, delivery_medium = new ProtoJson.DeliveryMedium() { medium_type = ProtoJson.DeliveryMediumType.DELIVERY_MEDIUM_BABEL }, event_type = ProtoJson.EventType.EVENT_TYPE_REGULAR_CHAT_MESSAGE }


            };
            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/sendchatmessage", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;
        }


        public void GetEntityById(params string[] chat_ids)
        {
            ProtoJson.GetEntityByIdRequest request = new ProtoJson.GetEntityByIdRequest()
            {
                request_header = RequestHeaderBody,
                batch_lookup_spec = chat_ids.Select(c => new WTalk.ProtoJson.EntityLookupSpec() { gaia_id = c }).ToList(),
                field_mask = new List<ProtoJson.FieldMask>() { ProtoJson.FieldMask.FIELD_MASK_AVAILABLE, ProtoJson.FieldMask.FIELD_MASK_DEVICE, ProtoJson.FieldMask.FIELD_MASK_REACHABLE }
            };


            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "contacts/getentitybyid", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;

            if (ContactInformationReceived != null)
            {
                arrayBody = JArray.Parse(result);
                arrayBody.RemoveAt(0);
                ProtoJson.GetEntityByIdResponse response = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.GetEntityByIdResponse>(arrayBody);

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
            ProtoJson.GetSelfInfoRequest request = new ProtoJson.GetSelfInfoRequest()
            {
                request_header = RequestHeaderBody
            };
            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "contacts/getselfinfo", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;

            if (UserInformationReceived != null)
            {
                arrayBody = JArray.Parse(result);
                arrayBody.RemoveAt(0);
                ProtoJson.GetSelfInfoResponse response = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.GetSelfInfoResponse>(arrayBody);

                CurrentUser = response.self_entity;
                UserInformationReceived(this,CurrentUser);
            }
        }

        public void SyncRecentConversations()
        {
            ProtoJson.SyncRecentConversationsRequest request = new ProtoJson.SyncRecentConversationsRequest()
            {
                request_header = RequestHeaderBody
            };

            JArray arrayBody = ProtoJson.ProtoJsonSerializer.Serialize(request);
            HttpResponseMessage message = _client.PostProtoJson(_api_key, "conversations/syncrecentconversations", arrayBody);
            string result = message.Content.ReadAsStringAsync().Result;

            if (ConversationHistoryLoaded != null)
            {
                arrayBody = JArray.Parse(result);
                arrayBody.RemoveAt(0);
                ProtoJson.SyncRecentConversationsResponse response = ProtoJson.ProtoJsonSerializer.Deserialize<ProtoJson.SyncRecentConversationsResponse>(arrayBody);

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
