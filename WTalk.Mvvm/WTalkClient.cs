using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WTalk.Abstraction.Model;

namespace WTalk.Abstraction
{
    public class WTalkClient
    {
        Client _client ;

        public static SynchronizationContext CurrentSynchronizationContext { get; private set; }

        #region events (routing)

        // connection
        public event EventHandler OnConnectionEstablished;
        public event EventHandler OnConnectionLost;

        // init 
        public event EventHandler<List<Conversation>> OnConversationHistoryReceived;
        public event EventHandler<List<User>> OnContactListReceived;

        // conversation
        public event EventHandler<Conversation> OnConversationUpdated;
        public event EventHandler<Conversation> OnNewConversationCreated;
        public event EventHandler<Message> OnNewMessageReceived;

        // user
        public event EventHandler<User> OnUserInformationReceived;
        public event EventHandler<User> OnContactInformationReceived;

        public event EventHandler<User> OnPresenceChanged;
        #endregion

        #region singleton
        static WTalkClient _instance;
        public static WTalkClient Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new WTalkClient();
                return _instance;
            }
        }
        #endregion

        WTalkClient() {

            CurrentSynchronizationContext = SynchronizationContext.Current;

            _client = new Client();
            _client.OnConnectionEstablished += _client_OnConnectionEstablished;
        }

        private void _client_OnConnectionEstablished(object sender, EventArgs e)
        {
            if (OnConnectionEstablished != null)
                OnConnectionEstablished(this, new EventArgs());
        }





#pragma warning disable CS4014
        public void Connect()
        {
            _client.ConnectAsync();
        }


    }
}
