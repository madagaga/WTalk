using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Mvvm;
using WTalk.Core.Utils;

namespace WTalk.Model
{
    public class Message : ObservableObject
    {   
        public Message()
        {
        }

        internal Message(WTalk.Core.ProtoJson.Schema.Event chatEvent)
        {
            Id = chatEvent.event_id;
            SenderId = chatEvent.sender_id.gaia_id;
            selfUserId = chatEvent.self_event_state.user_id.gaia_id;
            AppendContent(chatEvent);
            MessageDate = DateTime.Now.FromMillisecondsSince1970(chatEvent.timestamp / 1000);            
        }

        string selfUserId;
        public string Id { get; internal set; }
        public string SenderId { get; internal set; }        
        public bool IncomingMessage { get { return SenderId != selfUserId; } }
        public string SenderPhotoUrl { get { return FileCache.Current.Get(SenderId); } }

        //public Enums.MessageType Type { get; internal set; }
        public string Content { get { return string.Join<string>("\r\n", _content); } }
        public DateTime MessageDate { get; internal set; }
        internal string LastSegment { get { return _content.LastOrDefault(); } }

        
        List<string> _content = new List<string>();

        internal void AppendContent(WTalk.Core.ProtoJson.Schema.Event chatEvent)
        {
            _content.AddRange(chatEvent.chat_message.message_content.segment.Select(c => c.text));
            MessageDate = DateTime.Now.FromMillisecondsSince1970(chatEvent.timestamp / 1000); 
            OnPropertyChanged("Content");

        }
    }
}
