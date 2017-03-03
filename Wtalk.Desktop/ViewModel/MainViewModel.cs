using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WTalk.Desktop.Extension;
using WTalk.Desktop.Mvvm;
using WTalk.Desktop.Model;
using Wtalk.Desktop;

#pragma warning disable CS4014
namespace WTalk.Desktop.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public List<ConversationViewModel> ActiveConversations
        {
            get;
            private set;
        }
    

        ConversationViewModel _selectedConversation;
        public ConversationViewModel SelectedConversation
        {
            get { return _selectedConversation; }
            set
            {
                _selectedConversation = value;                
                OnPropertyChanged(nameof(SelectedConversation));
                OnPropertyChanged(nameof(ActiveConversations));                
            }
        }
        
        public User CurrentUser { get; set; }
        public bool Connected { get; private set; }


        int _currentPresenceIndex = 0;
        DateTime _lastStateUpdate = DateTime.Now;
        public int CurrentPresenceIndex
        {
            get { return _currentPresenceIndex; }
            set {
                switch (value)
                {
                    case 2:
                        _client.SetActiveClientAsync(false);
                        _client.SetPresenceAsync(0);
                        _authenticationManager.Disconnect();
                        System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                        System.Windows.Application.Current.Shutdown();
                        return;
                    case 3:
                        _client.SetPresenceAsync(0);
                        System.Windows.Application.Current.Shutdown();
                        return;
                    default:
                        _currentPresenceIndex = value;
                        _lastStateUpdate = DateTime.Now.AddSeconds(-750);
                        SetPresence();
                        break;
                }
            }
        }

        public bool AuthenticationRequired
        {
            get { return !_authenticationManager.IsAuthenticated; }
        }

        Client _client;
        AuthenticationManager _authenticationManager;

        public RelayCommand AuthenticateCommand { get; private set; }        
        public RelayCommand SetPresenceCommand { get; private set; }        


        private static object _lock = new object();

        public MainViewModel()
        {

            AuthenticateCommand = new RelayCommand(async () =>  await Authenticate());            
            SetPresenceCommand = new RelayCommand(() => SetPresence());

            _authenticationManager = WTalk.AuthenticationManager.Current;
            _authenticationManager.Connect();


            _client = Singleton.DefaultClient;


            _client.OnConnectionEstablished += _client_OnConnectionEstablished;
            _client.OnContactInformationReceived += _client_OnContactInformationReceived;
            _client.OnContactListReceived += _client_OnContactListReceived;
            _client.OnConversationHistoryReceived += _client_OnConversationHistoryReceived;

            _client.OnConversationUpdated += _client_OnConversationUpdated;
            _client.OnNewConversationCreated += _client_OnNewConversationCreated;
            _client.OnNewMessageReceived += _client_OnNewMessageReceived;
            _client.OnPresenceChanged += _client_OnPresenceChanged;

            _client.OnUserInformationReceived += _client_OnUserInformationReceived;
                       
            
            if(_authenticationManager.IsAuthenticated)
                _client.ConnectAsync();

            ActiveConversations = new List<ConversationViewModel>();

            App.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Data.BindingOperations.EnableCollectionSynchronization(ActiveConversations, _lock);
            });
        }

       

        void reorderContacts()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (ActiveConversations.Count > 0)
                {
                    ActiveConversations = ActiveConversations.OrderByDescending(c => c.Contact.Online).ThenByDescending(c => c.LastMessageDate).ToList();
                    OnPropertyChanged(nameof(ActiveConversations));
                }
            });
        }


        #region events 
        void _client_OnConnectionEstablished(object sender, EventArgs e)
        {
            if (CurrentUser != null)
                _client.SetPresenceAsync();
            else
                _client.GetSelfInfoAsync();

            Connected = true;
            OnPropertyChanged(nameof(Connected));

            //if(_conversationCache == null || _conversationCache.Count == 0)
            // _client.SyncRecentConversations();

            //_client.GetSuggestedEntities();
        }

        private void _client_OnContactInformationReceived(object sender, Core.ProtoJson.Schema.Entity e)
        {
            reorderContacts();
        }
        private void _client_OnUserInformationReceived(object sender, Core.ProtoJson.Schema.Entity e)
        {
            CurrentUser = new User(e);
            OnPropertyChanged(nameof(CurrentUser));
        }

        private void _client_OnPresenceChanged(object sender, Core.ProtoJson.Schema.Entity e)
        {
            if (e.id.gaia_id == CurrentUser.Id)
            {
                CurrentUser = new User(e);
                OnPropertyChanged(nameof(CurrentUser));
            }
            else
                reorderContacts();
        }

        private void _client_OnNewConversationCreated(object sender, Core.ProtoJson.Schema.ConversationState e)
        {
            ActiveConversations.Add(new ConversationViewModel(new Conversation(e)));
            reorderContacts();
        }

        private void _client_OnConversationUpdated(object sender, Core.ProtoJson.Schema.ConversationState e)
        {
            OnPropertyChanged(nameof(SelectedConversation));
        }

        private void _client_OnConversationHistoryReceived(object sender, List<Core.ProtoJson.Schema.ConversationState> e)
        {
            // associate contact list and last active conversation
            // only 1 to 1 conversation supported   
            if (ActiveConversations == null)
                ActiveConversations = new List<ConversationViewModel>();

            foreach (Core.ProtoJson.Schema.ConversationState conversation in e)
                ActiveConversations.Add(new ConversationViewModel(new Conversation(conversation)));

            reorderContacts();
        }

        private void _client_OnContactListReceived(object sender, List<Core.ProtoJson.Schema.Entity> e)
        {

        }

        private void _client_OnNewMessageReceived(object sender, Core.ProtoJson.Schema.Event e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (!App.Current.MainWindow.IsActive)
                    App.Current.MainWindow.FlashWindow();
            });

            reorderContacts();

        }

        #endregion
        
        #region relay commands
        async Task Authenticate()
        {

            AuthWindows auth_window = new AuthWindows();
            auth_window.ShowDialog();
                        
            if (_authenticationManager.IsAuthenticated)
            {
                await _client.ConnectAsync();
                OnPropertyChanged(nameof(AuthenticationRequired));
            }
        }

        void SetPresence()
        {               
            if (this.CurrentUser != null && (DateTime.Now - _lastStateUpdate).TotalSeconds > 720)
            {                
                _client.SetPresenceAsync(_currentPresenceIndex == 0 ? 40 : 1);
                _lastStateUpdate = DateTime.Now;
            }
            if(_client.CurrentUser != null)
                _client.QuerySelfPresenceAsync();
            
        }
        #endregion

    }

}
