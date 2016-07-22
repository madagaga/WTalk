using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Mvvm;
using WTalk;
using WTalk.Model;

namespace Wtalk.Desktop.ViewModel
{
    public class ConversationViewModel : ObservableObject
    {           
        Client _client;                

        //user
        public User Contact { get; private set; }
        string _name;

        public string ConversationName
        {
            get { return _name; }
            set { _name = value; }
        }

        double _scrollPosition = double.MaxValue;

        public double ScrollPosition
        {
            get { return _scrollPosition; }
            set { 
                
                _scrollPosition = value;
                if (value == 0)
                    _client.GetConversationAsync(Conversation.Id);
            }
        }
        
        public string MessageContent { get; set; }

        // conversations        
        public WTalk.Model.Conversation Conversation { get; private set; }
        public DateTime LastMessageDate { get { return Conversation.MessagesHistory.Last(c=>c.IncomingMessage).MessageDate; } }

        public bool HasUnreadMessages { get { return Conversation.SelfReadState < LastMessageDate; } }

        public RelayCommand SendMessageCommand { get; private set; }
        public RelayCommand SetFocusCommand { get; private set; }

        public RelayCommand DeleteConversationCommand { get; private set; }
        public RelayCommand SetOTRCommand { get; private set; }

        public RelayCommand SetUserTypingCommand { get; private set; }
        
        public ConversationViewModel()
        {            
            SendMessageCommand = new RelayCommand(() => SendMessage());
            SetFocusCommand = new RelayCommand(() => SetFocus());
            DeleteConversationCommand = new RelayCommand(() => DeleteConversation());
            SetOTRCommand = new RelayCommand(() => SetOTR());
            SetUserTypingCommand = new RelayCommand(() => SetUserTyping());
        }

        public ConversationViewModel(Client client):this()
        {
            _client = client;            
        }


        public ConversationViewModel(WTalk.Model.Conversation conversation, Client client):this(client)
        {
            this.Conversation = conversation;
            this.Conversation.NewMessageReceived += Conversation_NewMessageReceived;
            this.Contact = client.GetContactFromCache(conversation.Participants.Where(c => c.Key != client.CurrentUser.Id).FirstOrDefault().Key).Result;
            if (this.Contact != null)
                _name = this.Contact.DisplayName;
            else                
                _name = "Unknown User";
        }

        private void SetOTR()
        {
            _client.ModifyOTRStatusAsync(Conversation.Id, !Conversation.HistoryEnabled);
        }

        private void DeleteConversation()
        {
            
        }

        void Conversation_NewMessageReceived(object sender, Message e)
        {
            OnPropertyChanged("HasUnreadMessages");
        }
        
        private async Task SetFocus()
        {
            
            if(this.HasUnreadMessages)
                _client.UpdateWaterMarkAsync(Conversation.Id, DateTime.UtcNow);

            await _client.SetFocusAsync(Conversation.Id);
            OnPropertyChanged("HasUnreadMessages");
        }

      
        private async void SendMessage()
        {            
            await _client.SendChatMessageAsync(Conversation.Id, MessageContent);
            MessageContent = null;
            OnPropertyChanged(MessageContent);
        }


        
        private void SetUserTyping()
        {
            _client.SetUserTypingAsync(Conversation.Id);
        }
               

        
    }
}
