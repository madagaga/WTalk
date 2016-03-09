using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Mvvm;
using WTalk;
using WTalk.Model;
using System.Collections.ObjectModel;
using Wtalk.Desktop.WindowManager;

namespace Wtalk.Desktop.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public Dictionary<string, ConversationViewModel> ActiveContacts { get; private set; }
        
        public User CurrentUser { get; set; }

        int _currentPresenceIndex = 0;
        DateTime _lastStateUpdate = DateTime.Now;
        public int CurrentPresenceIndex
        {
            get { return _currentPresenceIndex; }
            set {
                if (value == 2)
                {
                    _authenticationManager.Disconnect();
                    System.Diagnostics.Process.Start(System.Windows.Application.ResourceAssembly.Location);
                    System.Windows.Application.Current.Shutdown();
                    return;
                }
                if(value == 3)
                {
                    System.Windows.Application.Current.Shutdown();
                    return;
                }
                _currentPresenceIndex = value;
                _lastStateUpdate = DateTime.Now.AddSeconds(-750);
                SetPresence();
            }
        }

        public bool AuthenticationRequired
        {
            get { return !_authenticationManager.IsAuthenticated; }
        }
        Client _client;
        AuthenticationManager _authenticationManager;

        Dictionary<string, ConversationWindowManager> _conversationCache;

        public RelayCommand<string> OpenConversationCommand { get; private set; }
        public RelayCommand<string> LoadConversationCommand { get; private set; }

        public RelayCommand<string> SetAuthenticationCodeCommand { get; private set; }
        public RelayCommand GetCodeCommand { get; private set; }
        public RelayCommand SetPresenceCommand { get; private set; }

        public MainViewModel()
        {
            OpenConversationCommand = new RelayCommand<string>((p) => OpenConversation(p, true));
            LoadConversationCommand = new RelayCommand<string>((p) => LoadConversation(p, true));
            SetAuthenticationCodeCommand = new RelayCommand<string>((p) => SetAuthenticationCode(p));
            GetCodeCommand = new RelayCommand(() => GetCode());
            SetPresenceCommand = new RelayCommand(() => SetPresence());


            _authenticationManager = new AuthenticationManager();
            _authenticationManager.Connect();

            _client = new Client();

            _client.ContactListLoaded += _client_ContactListLoaded;
            _client.ConversationHistoryLoaded += _client_ConversationHistoryLoaded;
            _client.NewConversationCreated += _client_NewConversationCreated;
            _client.UserInformationReceived += _client_UserInformationLoaded;
            _client.ContactInformationReceived += _client_ContactInformationReceived;
            _client.ConnectionEstablished += _client_OnConnectionEstablished;
            _client.UserInformationReceived += _client_UserInformationReceived;
            
            if(_authenticationManager.IsAuthenticated)
                _client.Connect();

            
        }

        void _client_UserInformationReceived(object sender, User e)
        {
            OnPropertyChanged("ActiveContacts");  
        }

        void _client_NewConversationCreated(object sender, Conversation e)
        {            
            _conversationCache.Add(e.Id,new ConversationWindowManager(new ConversationViewModel(e, _client)));            
            LoadConversation(e.Id, false);
            
        }

        void _client_ContactInformationReceived(object sender, User e)
        {
            OnPropertyChanged("ActiveContacts");           
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
                ActiveContacts = new Dictionary<string, ConversationViewModel>();            
            foreach(Conversation conversation in e)
                ActiveContacts.Add(conversation.Id, new ConversationViewModel(conversation,_client));
            _conversationCache = ActiveContacts.ToDictionary(c => c.Key, c => new ConversationWindowManager(c.Value));
            OnPropertyChanged("ActiveContacts"); 
        }

        void _client_ContactListLoaded(object sender, List<User> e)
        {
            
        }


        void _client_OnConnectionEstablished(object sender, EventArgs e)
        {  
            if (CurrentUser != null)
                _client.SetPresence();
            else
                _client.GetSelfInfo();

            if(_conversationCache == null || _conversationCache.Count == 0)
                _client.SyncRecentConversations();
        }

        void OpenConversation(string conversationId, bool bringToFront)
        {
            if (_conversationCache.ContainsKey(conversationId))
                _conversationCache[conversationId].Show(bringToFront);             

        }

        void LoadConversation(string userId, bool bringToFront)
        {
            //User participant = selectedUser as User;

            //if (_conversationCache.ContainsKey(conversationId))
            //    _conversationCache[conversationId].Show(bringToFront);

        }

        void GetCode()
        {
            string url = _authenticationManager.GetCodeUrl();
            System.Diagnostics.Process p = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                WorkingDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows),
                FileName = url,
                UseShellExecute = true
            });
        }

        void SetAuthenticationCode(string code)
        {
            _authenticationManager.AuthenticateWithCode(code);
            if (_authenticationManager.IsAuthenticated)
                _client.Connect();
            OnPropertyChanged("AuthenticationRequired");
        }

        void SetPresence()
        {            
            if (this.CurrentUser != null && (DateTime.Now - _lastStateUpdate).TotalSeconds > 720)
            {                
                _client.SetPresence(_currentPresenceIndex == 0 ? 40 : 1);
                _lastStateUpdate = DateTime.Now;
            }

            //_client.QuerySelfPresence();
            
        }

    }

}
