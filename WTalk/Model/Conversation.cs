using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
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

        public string Id { get { return _conversation.conversation_id.id; } }
        public Dictionary<string,Participant> Participants { get; internal set; }
        public string ParticipantsName { get; private set; }
        public string LastMessage { get; private set; }
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
            _conversation = conversationState.conversation;
            if (_conversation.read_state.Count > 0)
                ReadState = DateTime.Now.FromMillisecondsSince1970(_conversation.read_state.Last().latest_read_timestamp / 1000);

            Participants = _conversation.participant_data.ToDictionary(c=>c.id.chat_id, c => new Participant(c));
            ParticipantsName = string.Join<string>(", ", _conversation.participant_data.Select(c => c.fallback_name));
            MessagesHistory = new ObservableCollection<Message>();
            
            foreach(var cse in conversationState.events.Where(c=>c.chat_message != null))
            {
                if (_lastMessage != null && _lastMessage.SenderId == cse.sender_id.chat_id)
                    _lastMessage.AppendContent(cse.chat_message);
                else 
                {
                    _lastMessage = new Message(cse);
                    MessagesHistory.Add(_lastMessage);
                }
                LastMessage = cse.chat_message.message_content.segment.Last().text;
            }           
        }

        internal void NewEventReceived(Client client, Event e)
        {


            if (_lastMessage.SenderId == e.sender_id.chat_id)
                _lastMessage.AppendContent(e.chat_message);
            else
            {
                _lastMessage = new Message(e);
                MessagesHistory.Add(_lastMessage);
            }

            if (NewMessageReceived != null)
                NewMessageReceived(this, _lastMessage);

            LastMessage = e.chat_message.message_content.segment.Last().text;            
        }
        
    }
}
