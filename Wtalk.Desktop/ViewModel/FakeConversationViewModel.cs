using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Model;

namespace Wtalk.Desktop.ViewModel
{
    internal class FakeConversationViewModel
    {
        public Participant Participant { get; set; }
        Conversation _conversationCache;
        public ObservableCollection<Message> Messages { get ; private set;}

        public FakeConversationViewModel()
        {
            Participant = new Participant(new WTalk.ProtoJson.ConversationParticipantData()
                {
                    fallback_name = "Contact",
                    id = new WTalk.ProtoJson.ParticipantId() {  chat_id = "3"}
                });

            Participant p = new Participant(new WTalk.ProtoJson.ConversationParticipantData()
            {
                fallback_name = "Contact 1",
                id = new WTalk.ProtoJson.ParticipantId() {  chat_id = "2"}
            });

            Messages = new ObservableCollection<Message>();
            Messages.Add(
                new Message(Participant, new WTalk.ProtoJson.ChatMessage()
                {
                    message_content = new WTalk.ProtoJson.MessageContent()
                    {
                        segment = new List<WTalk.ProtoJson.Segment>() { new WTalk.ProtoJson.Segment() { text = "segment 1" } }
                    }
                }));

            Messages.Add(
                new Message(p, new WTalk.ProtoJson.ChatMessage()
                {
                    message_content = new WTalk.ProtoJson.MessageContent()
                    {
                        segment = new List<WTalk.ProtoJson.Segment>() { new WTalk.ProtoJson.Segment() { text = "segment 2" } }
                    }
                }));
            Messages.Add(
               new Message(p, new WTalk.ProtoJson.ChatMessage()
               {
                   message_content = new WTalk.ProtoJson.MessageContent()
                   {
                       segment = new List<WTalk.ProtoJson.Segment>() { new WTalk.ProtoJson.Segment() { text = "segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3" } }
                   }
               }));
                 
        }
    }
}
