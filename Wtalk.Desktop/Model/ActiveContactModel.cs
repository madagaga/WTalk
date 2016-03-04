using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Model;

namespace Wtalk.Desktop.Model
{
    public class ActiveContactModel
    {
        public User Contact { get; set; }
        public Conversation Conversation { get; set; }
        public string LastMessage { get { return Conversation.LastMessage; } }
        public bool Online { get { return Contact.Online; } }
        public DateTime LastMessageDate { get { return Conversation.MessagesHistory.Last().MessageDate; } }
    }
}
