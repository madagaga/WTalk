using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTalk.Model
{
    public class Participant
    {
        private ProtoJson.ConversationParticipantData _conversationParticipantData;

        public Participant(ProtoJson.ConversationParticipantData conversationParticipantData)
        {
            // TODO: Complete member initialization
            _conversationParticipantData = conversationParticipantData;
        }


        public string DisplayName { get { return _conversationParticipantData.fallback_name; } }
        public string Id { get { return _conversationParticipantData.id.chat_id; } }
    }
}
