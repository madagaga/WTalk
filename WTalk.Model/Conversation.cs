using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace WTalk.Model
{
    public class Conversation
    {
        ProtoJson.Conversation _conversation;

        public Conversation(ProtoJson.ConversationState conversationState)
        {
            _conversation = conversationState.conversation;            
            Participants = _conversation.participant_data.ToDictionary(c=>c.id.chat_id, c => new Participant(c));
            MessagesHistory = new List<Message>();
            Message message = null;
            conversationState.events.ForEach(cse =>
            {                
                if (message != null && message.SenderId == cse.sender_id.chat_id)
                    message.AppendContent(cse.chat_message);
                else
                {
                    message = new Message(Participants[cse.sender_id.chat_id], cse.chat_message);
                    MessagesHistory.Add(message);
                }
            });

            
                        
        }

        public string Id { get { return _conversation.conversation_id.id; } }
        public Dictionary<string,Participant> Participants { get; internal set; }
        //public Enums.ConversationType Type { get; internal set; }
        public List<Message> MessagesHistory { get; internal set; }
                
    }
}
