using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Universal.Model
{
    public class Conversation : Observable.ObservableObject
    {
        WTalk.Core.ProtoJson.Schema.ConversationState _conversationState;

        public string Id { get { return _conversationState.conversation_id.id; } }
        public string Name { get; private set; }
        public List<User> Participants { get; set; }
        public IEnumerable<Message> Messages { get; set; }
        public Message LastMessage { get { return Messages?.Last(); } }

        public User MainParticipant { get; set; }

        internal Conversation() { }

        public Conversation(WTalk.Core.ProtoJson.Schema.ConversationState state)
        {
            _conversationState = state;
            Participants = state.conversation.participant_data.Select(c => new User(Client.Current.GetContactFromCache(c.id.gaia_id).Result)).ToList();
            MainParticipant = Participants.Skip(1).FirstOrDefault();

            Messages = state.events.Where(c => c.chat_message != null).Select(c => new Message(c));
            Name = string.IsNullOrEmpty(_conversationState.conversation.name) ? string.Join(", ",Participants.Where(c => c.Id != Client.Current.CurrentUser.id.gaia_id).Select(c=>c.FirstName)) : _conversationState.conversation.name;

            Client.Current.OnNewMessageReceived += Current_OnNewMessageReceived;
        }

        private void Current_OnNewMessageReceived(object sender, Core.ProtoJson.Schema.Event e)
        {
            if (e.conversation_id.id == _conversationState.conversation_id.id)
            {
                Messages = _conversationState.events.Where(c => c.chat_message != null).Select(c => new Message(c));
                OnPropertyChanged(nameof(Messages));
            }
        }
    }
}
