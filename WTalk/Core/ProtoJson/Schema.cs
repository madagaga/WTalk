using System.Collections.Generic;

namespace WTalk.Core.ProtoJson.Schema
{
    
    [ProtoContract]
    internal class ModifyOTRStatusRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public object unknown1 { get; set; }

        [ProtoMember(Position = 3)]
        public OffTheRecordStatus otr_status { get; set; }

        [ProtoMember(Position = 4)]
        public object unknown2 { get; set; }

        [ProtoMember(Position = 5)]
        public EventRequestHeader event_request_header { get; set; }
    }
}
