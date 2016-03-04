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
        WTalk.Model.Conversation _innerConversation;  
        public event EventHandler AttentionRequired;
        public string Participants { get { return _innerConversation.ParticipantsName; } }
        public bool HistoryEnabled { get { return _innerConversation.HistoryEnabled; } }
        public ObservableCollection<Message> Messages { get { return _innerConversation.MessagesHistory; } }

        Client _client;        

        public RelayCommand<object> SendMessageCommand { get; private set; }
        public RelayCommand SetFocusCommand { get; private set; }

        public RelayCommand DeleteConversationCommand { get; private set; }
        public RelayCommand SetOTRCommand { get; private set; }

        public ConversationViewModel(WTalk.Model.Conversation conversation, Client client)
        {            
            this._innerConversation = conversation;
            this._innerConversation.NewMessageReceived += _innerConversation_NewMessageReceived;
            this._client = client;
            
            SendMessageCommand = new RelayCommand<object>((p) => SendMessage(p.ToString()));
            SetFocusCommand = new RelayCommand(() => SetFocus());
            DeleteConversationCommand = new RelayCommand(() => DeleteConversation());
            SetOTRCommand = new RelayCommand(() => SetOTR());
        }

        private void SetOTR()
        {
            _client.ModifyOTRStatus(_innerConversation.Id, !_innerConversation.HistoryEnabled);
        }

        private void DeleteConversation()
        {
            
        }

        void _innerConversation_NewMessageReceived(object sender, Message e)
        {            
            App.Current.Dispatcher.Invoke(() =>
            {
                if (AttentionRequired != null)
                    AttentionRequired(this, null);
            });
        }


        private void SetFocus()
        {
            this._innerConversation.UpdateReadState();
            _client.SetFocus(_innerConversation.Id);            
        }

      
        private void SendMessage(string p)
        {
            _client.SendChatMessage(_innerConversation.Id, p);
        }

        
    }
}
