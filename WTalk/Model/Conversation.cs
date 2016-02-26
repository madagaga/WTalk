using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using WTalk.Core.ProtoJson;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Core.Utils;

namespace WTalk.Model
{
    public class Conversation
    {
        WTalk.Core.ProtoJson.Schema.Conversation _conversation;
        public DateTime ReadState { get; internal set; }
        public bool HasUnreadMessages
        {
            get
            {
                return MessagesHistory.Count(c => c.MessageDate > ReadState) != 0;
            }
        }

        public Conversation(ConversationState conversationState)
        {
            _conversation = conversationState.conversation;
            if (_conversation.read_state.Count > 0)
                ReadState = DateTime.Now.FromMillisecondsSince1970(_conversation.read_state.Last().latest_read_timestamp / 1000);


            Participants = _conversation.participant_data.ToDictionary(c=>c.id.chat_id, c => new Participant(c));
            MessagesHistory = new List<Message>();
            Message message = null;
            foreach(var cse in conversationState.events.Where(c=>c.chat_message != null))
            {                
                if (message != null && message.SenderId == cse.sender_id.chat_id)
                    message.AppendContent(cse.chat_message);
                else 
                {
                    message = new Message(Participants[cse.sender_id.chat_id], cse);
                    MessagesHistory.Add(message);
                }
            }

            
                        
        }

        public string Id { get { return _conversation.conversation_id.id; } }
        public Dictionary<string,Participant> Participants { get; internal set; }
        //public Enums.ConversationType Type { get; internal set; }
        public List<Message> MessagesHistory { get; internal set; }
                
    }
}
