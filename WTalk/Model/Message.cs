using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wtalk.MvvM;

namespace WTalk.Model
{
    public class Message : ObservableObject
    {
        private WTalk.Core.ProtoJson.Schema.ChatMessage _chatMessage;
        private Participant _participant;
        

        public Message()
        {
        }

        public Message(Participant participant, WTalk.Core.ProtoJson.Schema.ChatMessage chatMessage)
        {
            // TODO: Complete member initialization
            this._participant = participant;
            this._chatMessage = chatMessage;
            AppendContent(chatMessage);
        }

        public string Id { get; internal set; }
        public string Sender { get { return _participant.DisplayName; } }
        public string SenderId { get { return _participant.Id; } }
        //public Enums.MessageType Type { get; internal set; }
        public string Content { get; internal set; }
        public DateTime MessageDate { get; internal set; }

        public void AppendContent(WTalk.Core.ProtoJson.Schema.ChatMessage chatMessage)
        {
            StringBuilder builder = new StringBuilder(Content);
            foreach( var c in chatMessage.message_content.segment)
                builder.AppendLine(c.text);

            Content = builder.ToString();
            OnPropertyChanged("Content");

        }
    }
}
