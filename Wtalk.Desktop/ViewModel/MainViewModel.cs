using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Mvvm;
using WTalk;
using WTalk.Model;
using System.Collections.ObjectModel;
using Wtalk.Desktop.Extension;

namespace Wtalk.Desktop.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public List<ConversationViewModel> ActiveContacts
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
                OnPropertyChanged("SelectedConversation");
                OnPropertyChanged("ActiveContacts");                
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


            _client = new Client();
            _client.ConversationHistoryLoaded += _client_ConversationHistoryLoaded;
            _client.NewConversationCreated += _client_NewConversationCreated;
            _client.UserInformationReceived += _client_UserInformationLoaded;            
            _client.ConnectionEstablished += _client_OnConnectionEstablished;
            _client.NewMessageReceived += _client_NewMessageReceived;
            _client.UserInformationReceived += _client_UserInformationReceived;
            _client.UserPresenceChanged += _client_UserPresenceChanged;

            _client.ContactInformationReceived += _client_ContactInformationReceived;
            
            if(_authenticationManager.IsAuthenticated)
                _client.ConnectAsync();
            ActiveContacts = new List<ConversationViewModel>();
            App.Current.Dispatcher.Invoke(() =>
            {
                System.Windows.Data.BindingOperations.EnableCollectionSynchronization(ActiveContacts, _lock);
            });
        }

        
        void reorderContacts()
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                if (ActiveContacts.Count > 0)
                {
                    ActiveContacts = ActiveContacts.OrderByDescending(c => c.Contact.Online).ThenByDescending(c => c.LastMessageDate).ToList();
                    OnPropertyChanged("ActiveContacts");
                }
            });
        }

        void _client_UserPresenceChanged(object sender, User e)
        {
            reorderContacts();
        }

        void _client_NewMessageReceived(object sender, Conversation e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
            if (!App.Current.MainWindow.IsActive)
                    App.Current.MainWindow.FlashWindow();
            });

            reorderContacts();
            
        }

        void _client_UserInformationReceived(object sender, User e)
        {
            OnPropertyChanged("CurrentUser");
            
        }

        void _client_NewConversationCreated(object sender, Conversation e)
        {
            ActiveContacts.Add(new ConversationViewModel(e, _client));
            reorderContacts();
            
        }

        void _client_ContactInformationReceived(object sender, User e)
        {
            reorderContacts();
        }

       
             
        void _client_UserInformationLoaded(object sender, User e)
        {
            CurrentUser = _client.CurrentUser;
            OnPropertyChanged("CurrentUser");
        }

        void _client_ConversationHistoryLoaded(object sender, List<Conversation> e)
        {
            // associate contact list and last active conversation
            // only 1 to 1 conversation supported   
            if (ActiveContacts == null)
                ActiveContacts = new List<ConversationViewModel>();

            foreach (Conversation conversation in e)
                ActiveContacts.Add(new ConversationViewModel(conversation, _client));

            reorderContacts();
            
        }


        void _client_OnConnectionEstablished(object sender, EventArgs e)
        {  
            if (CurrentUser != null)
                _client.SetPresenceAsync();
            else
                _client.GetSelfInfoAsync();

            Connected = true;
            OnPropertyChanged("Connected");

            //if(_conversationCache == null || _conversationCache.Count == 0)
               // _client.SyncRecentConversations();

            //_client.GetSuggestedEntities();
        }

        async Task Authenticate()
        {

            AuthWindows auth_window = new AuthWindows();
            auth_window.ShowDialog();

            //_authenticationManager.AuthenticateWithCode(code);
            if (_authenticationManager.IsAuthenticated)
            {
                _client.ConnectAsync();
                OnPropertyChanged("AuthenticationRequired");
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

    }

}
