using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Model;
using WTalk.Core.ProtoJson.Schema;

namespace Wtalk.Desktop.ViewModel
{
    internal class FakeConversationViewModel
    {
        public Participant Participant { get; set; }
        WTalk.Model.Conversation _conversationCache;
        public ObservableCollection<Message> Messages { get ; private set;}

        public FakeConversationViewModel()
        {
            Participant = new Participant(new ConversationParticipantData()
                {
                    fallback_name = "Contact",
                    id = new ParticipantId() {  chat_id = "3"}
                });

            Participant p = new Participant(new ConversationParticipantData()
            {
                fallback_name = "Contact 1",
                id = new ParticipantId() {  chat_id = "2"}
            });

            Messages = new ObservableCollection<Message>();
            Messages.Add(
                new Message(Participant, new Event()
                {
                    chat_message = new ChatMessage()
                    {
                        message_content = new MessageContent()
                        {
                            segment = new List<Segment>() { new Segment() { text = "segment 1" } }
                        }
                    },
                    timestamp = 1456479966813000
                }));

            Messages.Add(
                new Message(p, new Event()
                {
                    chat_message = new ChatMessage()
                    {
                        message_content = new MessageContent()
                        {
                            segment = new List<Segment>() { new Segment() { text = "segment 2" } }
                        }
                    },
                    timestamp = 1456479966813000
                }));
            Messages.Add(
               new Message(p, new Event()
                {
                    chat_message = new ChatMessage()
                    {
                        message_content = new MessageContent()
                        {
                            segment = new List<Segment>() { new Segment() { text = "segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3 segment 3" } }
                        }
                    },
                    timestamp = 1456479966813000
                }));
                 
        }
    }
}
