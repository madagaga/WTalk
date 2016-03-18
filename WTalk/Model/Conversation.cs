using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using WTalk.Core.ProtoJson;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Core.Utils;
using WTalk.Mvvm;

namespace WTalk.Model
{
    public class Conversation : ObservableObject
    {
        internal WTalk.Core.ProtoJson.Schema.Conversation _conversation;
        Message _lastMessage;
        SynchronizationContext _synchronizationContext;

        public string Id { get { return _conversation.conversation_id.id; } }
        public string Name { get { return _conversation.name; } }
        public Dictionary<string,Participant> Participants { get; internal set; }
        public string LastMessage { get { return _lastMessage.LastSegment; } }
        //public Enums.ConversationType Type { get; internal set; }
        public ObservableCollection<Message> MessagesHistory { get; internal set; }
        public bool HistoryEnabled { get { return _conversation.otr_status == OffTheRecordStatus.OFF_THE_RECORD_STATUS_ON_THE_RECORD; } }

        public DateTime ReadState { get; internal set; }        
        public void UpdateReadState()
        {
            this.ReadState = DateTime.UtcNow;
        }

        public event EventHandler<Message> NewMessageReceived;

        internal Conversation(ConversationState conversationState)
        {
            _synchronizationContext = SynchronizationContext.Current;

            _conversation = conversationState.conversation;
            if (_conversation.read_state.Count > 0)
                ReadState = DateTime.Now.FromMillisecondsSince1970(_conversation.read_state.Last().latest_read_timestamp / 1000);

            Participants = _conversation.participant_data.ToDictionary(c=>c.id.chat_id, c => new Participant(c));
            MessagesHistory = new ObservableCollection<Message>();
            
            foreach(var cse in conversationState.events.Where(c=>c.chat_message != null))
            {
                if (_lastMessage != null && _lastMessage.SenderId == cse.sender_id.gaia_id)
                    _lastMessage.AppendContent(cse.chat_message);
                else 
                {
                    _lastMessage = new Message(cse);
                    MessagesHistory.Add(_lastMessage);
                }
                
            }           
        }

        internal void NewEventReceived(Client client, Event e)
        {


            if (_lastMessage.SenderId == e.sender_id.chat_id)
                _lastMessage.AppendContent(e.chat_message);
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
        
    }
}
