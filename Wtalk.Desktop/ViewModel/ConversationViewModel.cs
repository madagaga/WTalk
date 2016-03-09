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
        public event EventHandler AttentionRequired;        

        //user
        public User Contact { get; private set; }
        public string Participants { get; private set; }

        // conversations        
        public WTalk.Model.Conversation Conversation { get; private set; }
        public DateTime LastMessageDate { get { return Conversation.MessagesHistory.Last().MessageDate; } }
        public bool HasUnreadMessage { get { return Conversation.ReadState < DateTime.Now; } }

        public RelayCommand<object> SendMessageCommand { get; private set; }
        public RelayCommand SetFocusCommand { get; private set; }

        public RelayCommand DeleteConversationCommand { get; private set; }
        public RelayCommand SetOTRCommand { get; private set; }


        public ConversationViewModel()
        {
            SendMessageCommand = new RelayCommand<object>((messageContent) => SendMessage(messageContent.ToString()));
            SetFocusCommand = new RelayCommand(() => SetFocus());
            DeleteConversationCommand = new RelayCommand(() => DeleteConversation());
            SetOTRCommand = new RelayCommand(() => SetOTR());
        }

        public ConversationViewModel(Client client):this()
        {
            _client = client;            
        }


        public ConversationViewModel(WTalk.Model.Conversation conversation, Client client):this(client)
        {
            this.Conversation = conversation;
            this.Conversation.NewMessageReceived += Conversation_NewMessageReceived;
            this.Contact = client.GetContactFromCache(conversation.Participants.Where(c => c.Key != client.CurrentUser.Id).FirstOrDefault().Key);
            if (this.Contact != null)
                Participants = this.Contact.DisplayName;
            else
                Participants = "Unknown User";
        }

        private void SetOTR()
        {
            _client.ModifyOTRStatus(Conversation.Id, !Conversation.HistoryEnabled);
        }

        private void DeleteConversation()
        {
            
        }

        void Conversation_NewMessageReceived(object sender, Message e)
        {
            OnPropertyChanged("Messages");
            App.Current.Dispatcher.Invoke(() =>
            {
                
                if (AttentionRequired != null)
                    AttentionRequired(this, null);
            });
        }


        private void SetFocus()
        {
            this.Conversation.UpdateReadState();
            _client.SetFocus(Conversation.Id);            
        }

      
        private void SendMessage(string content)
        {
            _client.SendChatMessage(Conversation.Id, content);
        }

        
    }
}
