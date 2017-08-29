using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using Wtalk.Desktop;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Core.Utils;
using WTalk.Desktop.Mvvm;

namespace WTalk.Desktop.Model
{
    public class Conversation : ObservableObject
    {
        internal WTalk.Core.ProtoJson.Schema.ConversationState _conversationState;



        Message _lastMessage { get { return MessagesHistory.Last(); } }
        SynchronizationContext _synchronizationContext;

        public string Id { get { return _conversationState.conversation_id.id; } }
        public string Name { get { return _conversationState.conversation.name; } }
        public Dictionary<string,Participant> Participants { get; internal set; }
        public string LastMessage { get { return string.Format("{0}{1}", !_lastMessage.IncomingMessage ? "You : " : "", _lastMessage.LastSegment); } }


        List<Message> _messagesHistory = new List<Message>();
        public List<Message> MessagesHistory
        {
            get
            {
                if(_messagesHistory.Count < 1)
                {
                    try
                    {
                        string _lastSender = "";
                        List<Event> groupedEvents = null;
                        _conversationState.events.Where(c => c.chat_message != null).ToList().ForEach((evt) =>
                         {
                             if (_lastSender == evt.sender_id.gaia_id)
                                 groupedEvents.Add(evt);
                             else
                             {
                                 if (groupedEvents != null)
                                     _messagesHistory.Add(new Message(groupedEvents));

                                 groupedEvents = new List<Event>();
                                 groupedEvents.Add(evt);
                                 _lastSender = evt.sender_id.gaia_id;
                             }
                         });
                        if (groupedEvents != null)
                            _messagesHistory.Add(new Message(groupedEvents));
                    }
                    catch(Exception e)
                    {
                       
                    }
                }
                return _messagesHistory;                
            }
        }
        public bool HistoryEnabled { get { return _conversationState.conversation.otr_status == OffTheRecordStatus.OFF_THE_RECORD_STATUS_ON_THE_RECORD; } }

        public DateTime ReadState
        {
            get
            {
                if (_conversationState.conversation.read_state.Count > 0)
                    return _conversationState.conversation.read_state.Last(c => c.latest_read_timestamp > 0).latest_read_timestamp.FromUnixTime();
                else
                    return DateTime.Now;
            }
        }
        public DateTime SelfReadState
        {
            get
            {
                if (_conversationState.conversation.self_conversation_state.self_read_state != null)
                    return _conversationState.conversation.self_conversation_state.self_read_state.latest_read_timestamp.FromUnixTime();
                else
                    return DateTime.Now;
            }
        }

        internal Dictionary<string, long> messagesIds = new Dictionary<string, long>();


        public event EventHandler<Message> NewMessageReceived;

        internal Conversation(ConversationState conversationState)
        {
            _conversationState = conversationState;
                      
            Participants = _conversationState.conversation.participant_data.ToDictionary(c=>c.id.chat_id, c => new Participant(c));
            Singleton.DefaultClient.OnConversationUpdated += DefaultClient_OnConversationUpdated;
            Singleton.DefaultClient.OnNewMessageReceived += DefaultClient_OnNewMessageReceived;
                  
        }

        private void DefaultClient_OnNewMessageReceived(object sender, Event e)
        {
            if (e.conversation_id.id == _conversationState.conversation_id.id)
            {
                _messagesHistory.Clear();
                OnPropertyChanged(nameof(MessagesHistory));
                OnPropertyChanged(nameof(LastMessage));
                NewMessageReceived(this, null);
            }
        }

        private void DefaultClient_OnConversationUpdated(object sender, ConversationState e)
        {
            if (e.conversation_id.id == _conversationState.conversation_id.id)
            {
                _messagesHistory.Clear();
                OnPropertyChanged(nameof(MessagesHistory));
                OnPropertyChanged(nameof(LastMessage));
            }

        }
        
    }
}
