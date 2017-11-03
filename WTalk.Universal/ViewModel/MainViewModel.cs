using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Universal.Model;
using WTalk.Universal.Observable;

namespace WTalk.Universal.ViewModel
{    
    public class MainViewModel : ObservableObject, INotifyPropertyChanged
    {

        Client _client;
       
        #region authentication
        AuthenticationManager _authenticationManager;
        
        public bool Connected { get; private set; }

        public bool IsBusy { get; private set; } = true;

        #endregion


        public MainViewModel()
        {
            _authenticationManager = WTalk.AuthenticationManager.Current;
            

            _client = WTalk.Client.Current;

            if (!_authenticationManager.IsAuthenticated)
                _authenticationManager.Connect();
            
            _client.ConnectAsync().ConfigureAwait(false);

            _client.OnConnectionEstablished += _client_OnConnectionEstablished;
            _client.OnConversationHistoryReceived += _client_OnConversationHistoryReceived;
            _client.OnUserInformationReceived += _client_OnUserInformationReceived;
            _client.OnContactListReceived += _client_OnContactListReceived;

            SendMessageCommand = new RelayCommand(() => SendMessage());
            SetFocusCommand = new RelayCommand(() => SetFocus());
            SetTypingCommand = new RelayCommand<Windows.UI.Xaml.Input.KeyRoutedEventArgs>((e) => SetTyping(e));

        }

        private void _client_OnContactListReceived(object sender, List<Core.ProtoJson.Schema.Entity> e)
        {
            Contacts = e.Select(c => new User(c)).OrderBy(c => c.DisplayName).ThenByDescending(c=>c.Online).ToList();
            OnPropertyChanged(nameof(Contacts));
        }

        private void _client_OnUserInformationReceived(object sender, Core.ProtoJson.Schema.Entity e)
        {
           CurrentUser = new User(e);           
           OnPropertyChanged(nameof(CurrentUser));
          
        }

        private void _client_OnConversationHistoryReceived(object sender, List<Core.ProtoJson.Schema.ConversationState> e)
        {
            ActiveConversations = e.Select(c => new Conversation(c)).OrderByDescending(c=>c.LastMessage.MessageDate).ToList();
            OnPropertyChanged(nameof(ActiveConversations));

            IsBusy = false;
            OnPropertyChanged(nameof(IsBusy));

        }

        private void _client_OnConnectionEstablished(object sender, EventArgs e)
        {
            if (CurrentUser != null)
                _client.SetPresenceAsync().ConfigureAwait(false);
            else
                _client.GetSelfInfoAsync().ConfigureAwait(false);

            Connected = true;
            
            OnPropertyChanged(nameof(Connected));           
        }


        #region Active conversations
        public List<Conversation> ActiveConversations { get; set; }

        Conversation _selectedConversation;

        public int ScrollPosition { get; set; }        
        #endregion

        #region Contacts
        public List<User> Contacts { get; set; }
        public User SelectedContact { get; set; }
        #endregion

        #region current user
        public User CurrentUser { get; private set; }
        public Conversation SelectedConversation { get => _selectedConversation;
            set
            {
                _selectedConversation = value;
                OnPropertyChanged(nameof(SelectedConversation));
            }
            
        }


        #endregion

        #region COmmands 
        public RelayCommand SendMessageCommand { get; private set; }
        private async void SendMessage()
        {
            if (!string.IsNullOrEmpty(_selectedConversation.MessageContent))
            {
                await _client.SendChatMessageAsync(SelectedConversation.Id, _selectedConversation.MessageContent);
                _selectedConversation.MessageContent = null;
                _selectedConversation.OnPropertyChanged(nameof(_selectedConversation.MessageContent));
            }
        }

        public RelayCommand SetFocusCommand { get; private set; } 
        public async void SetFocus()
        {
            //if (SelectedConversation.HasUnreadMessages)
                _client.UpdateWaterMarkAsync(SelectedConversation.Id, DateTime.UtcNow).ConfigureAwait(false);

            await _client.SetFocusAsync(SelectedConversation.Id);
            
        }

        public RelayCommand<Windows.UI.Xaml.Input.KeyRoutedEventArgs> SetTypingCommand { get; private set; }

        public void SetTyping(Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
                SendMessage();
            
            _client.SetUserTypingAsync(SelectedConversation.Id);
        }
        #endregion
    }
}
