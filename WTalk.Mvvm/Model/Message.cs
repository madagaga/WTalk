using System;
using System.Collections.Generic;
using System.Linq;
using WTalk.Core.Utils;
using WTalk.Core.ProtoJson.Schema;

namespace WTalk.Abstraction.Model
{
    public class Message : ObservableObject
    {
        internal Event _event;
        public Message()
        {
        }

        internal Message(WTalk.Core.ProtoJson.Schema.Event chatEvent)
        {
            _event = chatEvent;
            AddContent(chatEvent);
            MessageDate = chatEvent.timestamp.FromUnixTime();            
        }

        public string Id { get { return _event.event_id; } }
        public string SenderId { get { return _event.sender_id.gaia_id; } }
        public bool IncomingMessage { get { return _event.sender_id.gaia_id != _event.self_event_state.user_id.gaia_id; } }
        public string SenderPhotoUrl { get { return FileCache.Current.Get(SenderId); } }

        //public Enums.MessageType Type { get; internal set; }
        public string Content { get { return string.Join<string>("\r\n", _content); } }
        public DateTime MessageDate { get; internal set; }
        internal string LastSegment { get { return _content.LastOrDefault(); } }

        
        List<string> _content = new List<string>();

        internal void AddContent(WTalk.Core.ProtoJson.Schema.Event chatEvent, bool prepend = false )
        {
            if(prepend)
                _content.InsertRange(0, chatEvent.chat_message.message_content.segment.Select(c => c.text));
            else
                _content.AddRange(chatEvent.chat_message.message_content.segment.Select(c => c.text));
            MessageDate = chatEvent.timestamp.FromUnixTime(); 
            OnPropertyChanged("Content");

        }
    }
}
