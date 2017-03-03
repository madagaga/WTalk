using System;
using System.Collections.Generic;
using System.Linq;
using WTalk.Core.Utils;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Desktop.Mvvm;

namespace WTalk.Desktop.Model
{
    public class Message : ObservableObject
    {
        IEnumerable<Event> _events;
        Event _last;
        public Message()
        {
        }

        internal Message(IEnumerable<WTalk.Core.ProtoJson.Schema.Event> chatEvents)
        {
            _events = chatEvents;
            _last = chatEvents.Last();

            OnPropertyChanged(nameof(Content));
        }

        public bool IncomingMessage { get { return _last.sender_id.gaia_id != _last.self_event_state.user_id.gaia_id; } }
        public string SenderPhotoUrl { get { return FileCache.Current.Get(_last.sender_id.gaia_id); } }

        //public Enums.MessageType Type { get; internal set; }
        public string Content { get { return string.Join<string>("\r\n", _events.SelectMany(c => c.chat_message.message_content.segment.Select(s => s.text))); } }
        public DateTime MessageDate { get { return _last.timestamp.FromUnixTime(); } }
        internal string LastSegment { get { return _last.chat_message.message_content.segment.Last().text; } }

    }
}
