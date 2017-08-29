using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Core.ProtoJson.Schema;
using WTalk.Core.Utils;
using WTalk.Universal.Observable;

namespace WTalk.Universal.Model
{
    public class Message : ObservableObject
    {
        Event _event;

        public Message()
        {
        }

        internal Message(WTalk.Core.ProtoJson.Schema.Event chatEvent)
        {
            _event = chatEvent;
           

            OnPropertyChanged(nameof(Content));
        }

        public bool IncomingMessage { get { return _event.sender_id.gaia_id != _event.self_event_state.user_id.gaia_id; } }
        public string SenderPhotoUrl { get { return FileCache.Current.Get(_event.sender_id.gaia_id); } }

        //public Enums.MessageType Type { get; internal set; }
        public string Content { get { return string.Join<string>("\r\n", _event.chat_message.message_content.segment.Select(s => s.text)); } }
        public DateTime MessageDate { get { return _event.timestamp.FromUnixTime(); } }
        public string LastSegment { get { return _event.chat_message.message_content.segment.Last().text; } }

    }
}
