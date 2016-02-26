using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wtalk.MvvM;
using WTalk;
using WTalk.Model;

using System.Collections.ObjectModel;
using Wtalk.Desktop.WindowManager;
using WTalk.Core.ProtoJson.Schema;

namespace Wtalk.Desktop.ViewModel
{
    public class MainViewModel : ObservableObject
    {
        public ObservableCollection<User> Contacts { get; private set; }
        Dictionary<string, User> _contactDictionary;
        public User CurrentUser { get; set; }

        public bool AuthenticationRequiered
        {
            get { return !_authenticationManager.IsAuthenticated; }
        }
        Client _client;
        AuthenticationManager _authenticationManager;

        Dictionary<string, ConversationWindowManager> _conversationCache;

        public RelayCommand OpenConversationCommand { get; private set; }
        public RelayCommand SetAuthenticationCodeCommand { get; private set; }
        public RelayCommand GetCodeCommand { get; private set; }
        public RelayCommand SetPresenceCommand { get; private set; }

        public MainViewModel()
        {
            OpenConversationCommand = new RelayCommand((p) => LoadConversation(p, true));
            SetAuthenticationCodeCommand = new RelayCommand((p) => SetAuthenticationCode(p.ToString()));
            GetCodeCommand = new RelayCommand((p)=> GetCode());
            SetPresenceCommand = new RelayCommand((p) => SetPresence());


            _authenticationManager = new AuthenticationManager();
            _authenticationManager.Connect();

            _client = new Client();

            _client.ContactListLoaded += _client_ContactListLoaded;
            _client.ConversationHistoryLoaded += _client_ConversationHistoryLoaded;
            _client.NewConversationCreated += _client_NewConversationCreated;
            _client.UserInformationReceived += _client_UserInformationLoaded;
            _client.ContactInformationReceived += _client_ContactInformationReceived;
            _client.ConnectionEstablished += _client_OnConnectionEstablished;
            _client.PresenceInformationReceived += _client_PresenceInformationReceived;         
            
            if(_authenticationManager.IsAuthenticated)
                _client.Connect();

            
        }

        void _client_NewConversationCreated(object sender, ConversationState e)
        {
            string participantId = e.conversation.current_participant.FirstOrDefault(c => c.chat_id != CurrentUser.Id).chat_id;
            _conversationCache.Add(participantId,new ConversationWindowManager(new ConversationViewModel(new WTalk.Model.Conversation(e), _client)));
            if(!_contactDictionary.ContainsKey(participantId))
            {
                _client.GetEntityById(participantId);
                LoadConversation(participantId, false);
            }
        }

        void _client_ContactInformationReceived(object sender, Entity e)
        {
            if (!_contactDictionary.ContainsKey(e.id.chat_id))
            {
                _contactDictionary.Add(e.id.chat_id, new User(e));
                App.Current.Dispatcher.Invoke(() =>
                {
                    Contacts.Add(_contactDictionary[e.id.chat_id]);
                    OnPropertyChanged("Contacts");
                });
            }
        }

        void _client_PresenceInformationReceived(object sender, PresenceResult e)
        {
            if (_contactDictionary != null && _contactDictionary.ContainsKey(e.user_id.chat_id))
            {
                _contactDictionary[e.user_id.chat_id].SetPresence(e.presence);
                Contacts = new ObservableCollection<User>(Contacts.OrderBy(c => !c.Online).ThenBy(c => c.DisplayName));
                OnPropertyChanged("Contacts");
            }
            else if (CurrentUser.Id == e.user_id.chat_id)
                CurrentUser.SetPresence(e.presence);
        }

        void _client_UserInformationLoaded(object sender, Entity e)
        {
            CurrentUser = new User(_client.CurrentUser);
            OnPropertyChanged("CurrentUser");
        }

        void _client_ConversationHistoryLoaded(object sender, List<ConversationState> e)
        {
            // associate contact list and last active conversation
            // only 1 to 1 conversation supported
            _conversationCache = new Dictionary<string, ConversationWindowManager>();
            List<WTalk.Model.Conversation> filteredConversations = e.Where(c => c.conversation.type == ConversationType.CONVERSATION_TYPE_ONE_TO_ONE)
                .Select(c => new WTalk.Model.Conversation(c)).ToList();

            string participantId = null;
            filteredConversations.ForEach((convCache) =>
            {
                participantId = convCache.Participants.Keys.FirstOrDefault(c => c != CurrentUser.Id);
                if (!string.IsNullOrEmpty(participantId))
                    _conversationCache.Add(participantId, new ConversationWindowManager(new ConversationViewModel(convCache, _client)));
            });
        }

        void _client_ContactListLoaded(object sender, List<Entity> e)
        {
            _contactDictionary = e.ToDictionary(c => c.id.chat_id, c => new User(c));
            Contacts = new ObservableCollection<User>(_contactDictionary.Values);            
        }


        void _client_OnConnectionEstablished(object sender, EventArgs e)
        {
            _client.SetActiveClient();
           
            if (CurrentUser != null)
                _client.SetPresence();
            else
                _client.GetSelfInfo();

            if(_conversationCache == null || _conversationCache.Count == 0)
                _client.SyncRecentConversations();

            if (Contacts == null || Contacts.Count == 0)
            {
                _contactDictionary = new Dictionary<string, User>();
                Contacts = new ObservableCollection<User>();
                _client.GetEntityById(_conversationCache.Keys.Distinct().ToArray());
            }
            else
                _client.QueryPresences();
        }

        void LoadConversation(object selectedUser, bool bringToFront)
        {
            User participant = selectedUser as User;

            if (_conversationCache.ContainsKey(participant.Id))
                _conversationCache[participant.Id].Show(bringToFront);             

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
            OnPropertyChanged("AuthenticationRequiered");
        }

        void SetPresence()
        {
            if (this.CurrentUser != null && !this.CurrentUser.Online)
            {
                _client.SetPresence();
                _client.SetActiveClient();
            }

            _client.QuerySelfPresence();
            
        }

    }

}
