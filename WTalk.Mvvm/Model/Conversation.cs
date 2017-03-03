using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Core.Utils;

namespace WTalk.Abstraction.Model
{
    public class Conversation : ObservableObject
    {
        internal WTalk.Core.ProtoJson.Schema.Conversation _conversation;
        Message _lastMessage;
        SynchronizationContext _synchronizationContext;

        public string Id { get { return _conversation.conversation_id.id; } }
        public string Name { get { return _conversation.name; } }
        public Dictionary<string,Participant> Participants { get; internal set; }
        public string LastMessage { get { return string.Format("{0}{1}", !_lastMessage.IncomingMessage ? "You : " : "", _lastMessage.LastSegment); } }
        //public Enums.ConversationType Type { get; internal set; }
        public ObservableCollection<Message> MessagesHistory { get; internal set; }
        public bool HistoryEnabled { get { return _conversation.otr_status == OffTheRecordStatus.OFF_THE_RECORD_STATUS_ON_THE_RECORD; } }

        public DateTime ReadState { get; internal set; }
        public DateTime SelfReadState { get; internal set; }
        internal Dictionary<string, long> messagesIds = new Dictionary<string, long>();


        public event EventHandler<Message> NewMessageReceived;

        internal Conversation(ConversationState conversationState)
        {            

            _conversation = conversationState.conversation;
            if (_conversation.read_state.Count > 0)
                ReadState = _conversation.read_state.Last(c=>c.latest_read_timestamp > 0).latest_read_timestamp.FromUnixTime();
            if (_conversation.self_conversation_state.self_read_state != null)
                SelfReadState = _conversation.self_conversation_state.self_read_state.latest_read_timestamp.FromUnixTime();
            Participants = _conversation.participant_data.ToDictionary(c=>c.id.chat_id, c => new Participant(c));
            MessagesHistory = new ObservableCollection<Message>();
            
            foreach(var cse in conversationState.events.Where(c=>c.chat_message != null))
            {
                messagesIds.Add(cse.event_id, cse.timestamp);
                if (_lastMessage != null && _lastMessage.SenderId == cse.sender_id.gaia_id)
                    _lastMessage.AddContent(cse);
                else 
                {
                    _lastMessage = new Message(cse);
                    MessagesHistory.Add(_lastMessage);
                }
                
            }           
        }

        internal void AddNewMessage(List<Event> events)
        {
            IEnumerable<Event> orderedEvents = events.OrderBy(c => c.timestamp);
            foreach (Event e in orderedEvents)
                AddNewMessage(e);
        }

        internal void AddNewMessage(Event e)
        {
            if (messagesIds.ContainsKey(e.event_id))
                return;

            messagesIds.Add(e.event_id, e.timestamp);
            if (_lastMessage.SenderId == e.sender_id.chat_id)
                _lastMessage.AddContent(e);
            else
            {
                _lastMessage = new Message(e);
                _synchronizationContext.Post((obj) =>
                {
                    MessagesHistory.Add(obj as Message);
                }, _lastMessage);

            }

            OnPropertyChanged("MessagesHistory");
            OnPropertyChanged("LastMessage");

            if (NewMessageReceived != null)
                NewMessageReceived(this, _lastMessage);
        }

        internal void AddOldMessages(List<Event> events)
        {
            _synchronizationContext.Post((obj) =>
            {   
                IEnumerable<Event> orderedEvents = ((List<Event>)obj).OrderByDescending(c => c.timestamp);
                Message current = MessagesHistory.First();
                // hangout return all messages                 
                foreach (Event e in orderedEvents)
                {
                    if (messagesIds.ContainsKey(e.event_id))
                        continue;

                    messagesIds.Add(e.event_id, e.timestamp);
                    if (current.SenderId == e.sender_id.chat_id)
                        current.AddContent(e, true);
                    else
                    {
                        current = new Message(e);
                        MessagesHistory.Insert(0, current);
                    }
                }
                
            }, events);
        }
        
    }
}
