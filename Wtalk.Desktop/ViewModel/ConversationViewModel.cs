using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wtalk.MvvM;
using WTalk;
using WTalk.Model;

namespace Wtalk.Desktop.ViewModel
{
    public class ConversationViewModel : ObservableObject
    {
        public Participant Participant { get; set; }
        Conversation _conversationCache;
        Message _lastMessage;
        public ObservableCollection<Message> Messages { get; private set; }

        public event EventHandler AttentionRequired; 

        Client _client;

        public RelayCommand SendMessageCommand { get; private set; }
        public RelayCommand SetFocusCommand { get; private set; }

        public ConversationViewModel(Conversation conversationCache, Client client)
        {
            this.Participant = conversationCache.Participants.Values.FirstOrDefault(c=>c.Id != client.CurrentUser.id.chat_id);
            this.Messages = new ObservableCollection<Message>(conversationCache.MessagesHistory);
            _lastMessage = this.Messages.LastOrDefault();
            this._conversationCache = conversationCache;            
            this._client = client;
            this._client.NewConversationEventReceived += _client_NewConversationEventReceived;


            SendMessageCommand = new RelayCommand((p) => SendMessage(p.ToString()));
            SetFocusCommand = new RelayCommand((p) => SetFocus(_conversationCache.Id));            
        }

        void _client_NewConversationEventReceived(object sender, WTalk.ProtoJson.Event e)
        {
            if(e.conversation_id.id == this._conversationCache.Id)
            {
                if (_lastMessage.SenderId == e.sender_id.chat_id)
                    _lastMessage.AppendContent(e.chat_message);
                else
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _lastMessage = new Message(_conversationCache.Participants[e.sender_id.chat_id], e.chat_message);
                        Messages.Add(_lastMessage);
                        if (AttentionRequired != null)
                            AttentionRequired(this, null);
                    });
                }
            }
        }

        private void SetFocus(string conversationId)
        {
            _client.SetFocus(conversationId);
        }

      
        private void SendMessage(string p)
        {
            _client.SendChatMessage(_conversationCache.Id, p);
        }

        
    }
}
