using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTalk.Core.ProtoJson;

namespace WTalk.Core.ProtoJson.Schema
{
    /*

hangouts.proto - Unofficial, reverse engineered Protocol Buffers for Google's
Hangouts chat protocol.

Originally based on Darryl Pogue's reversed engineered proto file from an early
version of Hangouts for Android: https://gist.github.com/dpogue/5692114

Field and message name should generally match those used by Google, with the
major exception that "Client" prefixes have been removed.

(repeated)\s(\w*)\s(\w*)\s\=\s(\d*)\; =>  [ProtoMember\(\4\)]\r\n public List<\2> \3 {get;set;}\r\n
(optional)\s(\w*)\s(\w*)\s\=\s(\d*)\; =>  [ProtoMember\(\4\)]\r\n public \2 \3 {get;set;}\r\n
 */




    // Describes which Hangouts client is active.
    internal enum ActiveClientState : int
    {
        // No client is active.
        ACTIVE_CLIENT_STATE_NO_ACTIVE = 0,
        // This is the active client.
        ACTIVE_CLIENT_STATE_IS_ACTIVE = 1,
        // Other client is active.
        ACTIVE_CLIENT_STATE_OTHER_ACTIVE = 2
    }

    // The state of do-not-disturb mode. Not to be confused with DndSetting, which
    // is used to change the state of do-not-disturb mode.
    [ProtoContract]
    internal class DoNotDisturbSetting
    {
        // Whether do-not-disturb mode is enabled.
        [ProtoMember(Position = 1)]
        public bool do_not_disturb { get; set; }

        // Timestamp when do-not-disturb mode expires.
        [ProtoMember(Position = 2)]
        public long expiration_timestamp { get; set; }

        // Timestamp when this setting was applied. Not present when this message
        // comes from a notification.
        [ProtoMember(Position = 3)]
        public long version { get; set; }

    }

    [ProtoContract]
    internal class NotificationSettings
    {
        [ProtoMember(Position = 1)]
        public DoNotDisturbSetting dnd_settings { get; set; }

    }

    internal enum FocusType : int
    {
        FOCUS_TYPE_UNKNOWN = 0,
        FOCUS_TYPE_FOCUSED = 1,
        FOCUS_TYPE_UNFOCUSED = 2
    }

    internal enum FocusDevice : int
    {
        FOCUS_DEVICE_UNSPECIFIED = 0,
        FOCUS_DEVICE_DESKTOP = 20,
        FOCUS_DEVICE_MOBILE = 300,
    }

    // Identifies a conversation.
    [ProtoContract]
    internal class ConversationId
    {
        // Unique identifier for a conversation.
        [ProtoMember(Position = 1)]
        public string id { get; set; }

    }

    // Identifies a user.
    [ProtoContract]
    internal class ParticipantId
    {
        // Unique identifier for a user's Google account.
        [ProtoMember(Position = 1, Optional = true)]
        public string gaia_id { get; set; }

        // Seems to always be the same as gaia_id.
        [ProtoMember(Position = 2)]
        public string chat_id { get; set; }

    }

    // Indicates whether Hangouts is active (running in the foreground) on
    // different types of devices.
    [ProtoContract]
    internal class DeviceStatus
    {
        // True if a mobile phone is active.
        [ProtoMember(Position = 1)]
        public bool mobile { get; set; }

        // True if a desktop or laptop is active.
        [ProtoMember(Position = 2)]
        public bool desktop { get; set; }

        // True if a tablet is active.
        [ProtoMember(Position = 3)]
        public bool tablet { get; set; }

    }

    [ProtoContract]
    internal class Presence
    {
        [ProtoMember(Position = 1)]
        public bool reachable { get; set; }

        [ProtoMember(Position = 2)]
        public bool available { get; set; }

        [ProtoMember(Position = 6)]
        public DeviceStatus device_status { get; set; }

        [ProtoMember(Position = 9)]
        public MoodMessage mood_message { get; set; }

        [ProtoMember(Position = 10)]
        public LastSeen last_seen { get; set; }

    }

    [ProtoContract]
    internal class PresenceResult
    {
        [ProtoMember(Position = 1)]
        public ParticipantId user_id { get; set; }

        [ProtoMember(Position = 2)]
        public Presence presence { get; set; }

    }

    internal enum TypingType : int
    {
        TYPING_TYPE_UNKNOWN = 0,
        // Started typing.
        TYPING_TYPE_STARTED = 1,
        // Stopped typing with inputted text.
        TYPING_TYPE_PAUSED = 2,
        // Stopped typing with no inputted text.
        TYPING_TYPE_STOPPED = 3
    }

    [ProtoContract]
    internal class ClientIdentifier
    {
        // (client_id in hangups).
        [ProtoMember(Position = 1)]
        public string resource { get; set; }

        // unknown (header_id in hangups).
        [ProtoMember(Position = 2)]
        public string header_id { get; set; }

    }

    internal enum ClientPresenceStateType : int
    {
        CLIENT_PRESENCE_STATE_UNKNOWN = 0,
        CLIENT_PRESENCE_STATE_NONE = 1,
        CLIENT_PRESENCE_STATE_DESKTOP_IDLE = 30,
        CLIENT_PRESENCE_STATE_DESKTOP_ACTIVE = 40

        // TODO
    }

    [ProtoContract]
    internal class ClientPresenceState
    {
        [ProtoMember(Position = 1)]
        public ClientIdentifier identifier { get; set; }

        [ProtoMember(Position = 2)]
        public ClientPresenceStateType state { get; set; }

    }

    internal enum NotificationLevel : int
    {
        NOTIFICATION_LEVEL_UNKNOWN = 0,
        // Notifications are disabled.
        NOTIFICATION_LEVEL_QUIET = 10,
        // Notifications are enabled
        NOTIFICATION_LEVEL_RING = 30
    }

    [ProtoContract]
    internal class UserEventState
    {
        [ProtoMember(Position = 1)]
        public ParticipantId user_id { get; set; }

        [ProtoMember(Position = 2)]
        public string client_generated_id { get; set; }

        [ProtoMember(Position = 3)]
        public NotificationLevel notification_level { get; set; }

    }

    internal enum SegmentType : int
    {
        // Segment is text.
        SEGMENT_TYPE_TEXT = 0,
        // Segment is a line break.
        SEGMENT_TYPE_LINE_BREAK = 1,
        // Segment is hyperlinked text.
        SEGMENT_TYPE_LINK = 2
    }

    [ProtoContract]
    internal class Formatting
    {
        [ProtoMember(Position = 1)]
        public bool bold { get; set; }

        [ProtoMember(Position = 2)]
        public bool italic { get; set; }

        [ProtoMember(Position = 3)]
        public bool strikethrough { get; set; }

        [ProtoMember(Position = 4)]
        public bool underline { get; set; }

    }

    [ProtoContract]
    internal class LinkData
    {
        [ProtoMember(Position = 1)]
        public string link_target { get; set; }

    }

    // A segment of a message. Message are broken into segments that may be of
    // different types and have different formatting.
    [ProtoContract]
    internal class Segment
    {
        // Note: This field is required because Hangouts for Chrome misbehaves if it
        // isn't serialized.
        [ProtoMember(Position = 1)]
        public SegmentType type { get; set; }
        // The segment text. For line breaks, may either be empty or contain new line
        // character.
        [ProtoMember(Position = 2)]
        public string text { get; set; }

        // Formatting for this segment.
        [ProtoMember(Position = 3)]
        public Formatting formatting { get; set; }

        // Link data for this segment, if it is a link.
        [ProtoMember(Position = 4, Optional=true)]
        public LinkData link_data { get; set; }

    }

    // A type of embedded item.
    internal enum ItemType : int
    {
        ITEM_TYPE_THING = 0,
        // Google Plus photo.
        ITEM_TYPE_PLUS_PHOTO = 249,
        ITEM_TYPE_PLACE = 335,
        // Google Map place.
        ITEM_TYPE_PLACE_V2 = 340
    }

    // Google Plus photo that can be embedded in a chat message.
    [ProtoContract]
    internal class PlusPhoto
    {

        // Metadata for displaying an image thumbnail.
        [ProtoContract]
        internal class Thumbnail
        {

            // URL to navigate to when thumbnail is selected (a Google Plus album
            // page).
            [ProtoMember(Position = 1)]
            public string url { get; set; }

            // URL of thumbnail image.
            [ProtoMember(Position = 4)]
            public string image_url { get; set; }

            // Image width in pixels.
            [ProtoMember(Position = 10)]
            public long width_px { get; set; }

            // Image height in pixels.
            [ProtoMember(Position = 11)]
            public long height_px { get; set; }

        }

        // Media type.
        internal enum MediaType
        {
            MEDIA_TYPE_UNKNOWN = 0,
            MEDIA_TYPE_PHOTO = 1
        }

        // Thumbnail.
        [ProtoMember(Position = 1)]
        public Thumbnail thumbnail { get; set; }

        // Owner obfuscated ID.
        [ProtoMember(Position = 2)]
        public string owner_obfuscated_id { get; set; }

        // Album ID.
        [ProtoMember(Position = 3)]
        public string album_id { get; set; }

        // Photo ID.
        [ProtoMember(Position = 4)]
        public string photo_id { get; set; }

        // URL of full-sized image.
        [ProtoMember(Position = 6)]
        public string url { get; set; }

        // URL of image thumbnail.
        [ProtoMember(Position = 10)]
        public string original_content_url { get; set; }

        // The media type.
        [ProtoMember(Position = 13)]
        public MediaType media_type { get; set; }

        // List of stream ID parameters.
        [ProtoMember(Position = 14)]
        public List<string> stream_id { get; set; }


    }

    // Place that can be embedded in a chat message via Google Maps.
    [ProtoContract]
    internal class Place
    {

        // Representative image of a place.
        [ProtoContract]
        internal class RepresentativeImage
        {
            // URL of image.
            [ProtoMember(Position = 2)]
            public string url { get; set; }

        }

        // Google Maps URL pointing to the map coordinates.
        [ProtoMember(Position = 1)]
        public string url { get; set; }

        // Name of map location.
        [ProtoMember(Position = 3)]
        public string name { get; set; }

        // Representative image of the place (map with pin).
        [ProtoMember(Position = 185)]
        public RepresentativeImage representative_image { get; set; }

    }

    // An item of some type embedded in a chat message.
    [ProtoContract]
    internal class EmbedItem
    {
        // List of embedded item types in this message.
        [ProtoMember(Position = 1)]
        public List<ItemType> type { get; set; }

        // For photos this is not given, for maps, it's the URL of the map.
        [ProtoMember(Position = 2)]
        public string id { get; set; }

        // Embedded Google Plus photo.
        [ProtoMember(Position = 27639957)]
        public PlusPhoto plus_photo { get; set; }

        // Embedded Google Map of a place.
        [ProtoMember(Position = 35825640)]
        public Place place { get; set; }

    }

    // An attachment for a chat message.
    [ProtoContract]
    internal class Attachment
    {
        [ProtoMember(Position = 1)]
        public EmbedItem embed_item { get; set; }

    }

    // Chat message content.
    [ProtoContract]
    internal class MessageContent
    {
        [ProtoMember(Position = 1)]
        public List<Segment> segment { get; set; }

        [ProtoMember(Position = 2)]
        public List<Attachment> attachment { get; set; }

    }

    // Annotation that can be applied to a chat message event. The only known use
    // for this is "\me" actions supported by the Chrome client (type 4).
    [ProtoContract]
    internal class EventAnnotation
    {
        // Annotation type.
        [ProtoMember(Position = 1)]
        public int type { get; set; }

        // Optional annotation string value.
        [ProtoMember(Position = 2)]
        public string value { get; set; }

    }

    // A chat message in a conversation.
    [ProtoContract]
    internal class ChatMessage
    {
        // Optional annotation to attach to message.
        [ProtoMember(Position = 2)]
        public List<EventAnnotation> annotation { get; set; }

        // The message's content.
        [ProtoMember(Position = 3)]
        public MessageContent message_content { get; set; }


        // TODO:
        // always 0? = 1;
    }

    internal enum MembershipChangeType : int
    {
        MEMBERSHIP_CHANGE_TYPE_JOIN = 1,
        MEMBERSHIP_CHANGE_TYPE_LEAVE = 2
    }

    [ProtoContract]
    internal class MembershipChange
    {
        [ProtoMember(Position = 1)]
        public MembershipChangeType type { get; set; }

        [ProtoMember(Position = 3)]
        public List<ParticipantId> participant_ids { get; set; }


        // TODO:
        // unknown [] = 2;
        // leave_reason (4, 2) = 4;
    }

    [ProtoContract]
    internal class ConversationRename
    {
        [ProtoMember(Position = 1)]
        public string new_name { get; set; }

        [ProtoMember(Position = 2)]
        public string old_name { get; set; }

    }

    internal enum HangoutEventType : int
    {
        HANGOUT_EVENT_TYPE_UNKNOWN = 0,
        HANGOUT_EVENT_TYPE_START = 1,
        HANGOUT_EVENT_TYPE_END = 2,
        HANGOUT_EVENT_TYPE_JOIN = 3,
        HANGOUT_EVENT_TYPE_LEAVE = 4,
        HANGOUT_EVENT_TYPE_COMING_SOON = 5,
        HANGOUT_EVENT_TYPE_ONGOING = 6
    }

    [ProtoContract]
    internal class HangoutEvent
    {
        [ProtoMember(Position = 1)]
        public HangoutEventType event_type { get; set; }

        [ProtoMember(Position = 2)]
        public List<ParticipantId> participant_id { get; set; }


        // TODO:
        // unknown 7 = 1;
        // unknown 3 = 25;
    }

    [ProtoContract]
    internal class OTRModification
    {
        [ProtoMember(Position = 1)]
        public OffTheRecordStatus old_otr_status { get; set; }

        [ProtoMember(Position = 2)]
        public OffTheRecordStatus new_otr_status { get; set; }

        [ProtoMember(Position = 3)]
        public OffTheRecordToggle old_otr_toggle { get; set; }

        [ProtoMember(Position = 4)]
        public OffTheRecordToggle new_otr_toggle { get; set; }

    }

    // Whether the OTR toggle is available to the user.
    internal enum OffTheRecordToggle : int
    {
        OFF_THE_RECORD_TOGGLE_UNKNOWN = 0,
        OFF_THE_RECORD_TOGGLE_ENABLED = 1,
        OFF_THE_RECORD_TOGGLE_DISABLED = 2,
    }

    internal enum OffTheRecordStatus : int
    {
        OFF_THE_RECORD_STATUS_UNKNOWN = 0,
        // Conversation is off-the-record (history disabled).
        OFF_THE_RECORD_STATUS_OFF_THE_RECORD = 1,
        // Conversation is on-the-record (history enabled).
        OFF_THE_RECORD_STATUS_ON_THE_RECORD = 2
    }

    internal enum SourceType : int
    {
        SOURCE_TYPE_UNKNOWN = 0,
    }

    internal enum EventType : int
    {
        EVENT_TYPE_UNKNOWN = 0,
        EVENT_TYPE_REGULAR_CHAT_MESSAGE = 1,
        EVENT_TYPE_SMS = 2,
        EVENT_TYPE_VOICEMAIL = 3,
        EVENT_TYPE_ADD_USER = 4,
        EVENT_TYPE_REMOVE_USER = 5,
        EVENT_TYPE_CONVERSATION_RENAME = 6,
        EVENT_TYPE_HANGOUT = 7,
        EVENT_TYPE_PHONE_CALL = 8,
        EVENT_TYPE_OTR_MODIFICATION = 9,
        EVENT_TYPE_PLAN_MUTATION = 10,
        EVENT_TYPE_MMS = 11,
        EVENT_TYPE_DEPRECATED_12 = 12,
    }

    [ProtoContract]
    internal class HashModifier
    {
        [ProtoMember(Position = 1)]
        public string update_id { get; set; }

        [ProtoMember(Position = 2)]
        public long hash_diff { get; set; }

        [ProtoMember(Position = 4)]
        public long version { get; set; }

    }

    // Event that becomes part of a conversation's history.
    [ProtoContract]
    internal class Event
    {
        // ID of the conversation this event belongs to.
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        // ID of the user that sent this event.
        [ProtoMember(Position = 2)]
        public ParticipantId sender_id { get; set; }

        // Timestamp when the event occurred.
        [ProtoMember(Position = 3)]
        public long timestamp { get; set; }

        [ProtoMember(Position = 4)]
        public UserEventState self_event_state { get; set; }

        [ProtoMember(Position = 6)]
        public SourceType source_type { get; set; }

        [ProtoMember(Position = 7)]
        public ChatMessage chat_message { get; set; }

        [ProtoMember(Position = 9)]
        public MembershipChange membership_change { get; set; }

        [ProtoMember(Position = 10)]
        public ConversationRename conversation_rename { get; set; }

        [ProtoMember(Position = 11)]
        public HangoutEvent hangout_event { get; set; }

        // Unique ID for the event.
        [ProtoMember(Position = 12)]
        public string event_id { get; set; }

        [ProtoMember(Position = 13)]
        public long expiration_timestamp { get; set; }

        [ProtoMember(Position = 14)]
        public OTRModification otr_modification { get; set; }

        [ProtoMember(Position = 15)]
        public bool advances_sort_timestamp { get; set; }

        [ProtoMember(Position = 16)]
        public OffTheRecordStatus otr_status { get; set; }

        [ProtoMember(Position = 17)]
        public bool persisted { get; set; }

        [ProtoMember(Position = 20)]
        public DeliveryMedium medium_type { get; set; }

        // The event's type.
        [ProtoMember(Position = 23)]
        public EventType event_type { get; set; }

        // Event version timestamp.
        [ProtoMember(Position = 24)]
        public long event_version { get; set; }

        [ProtoMember(Position = 26)]
        public HashModifier hash_modifier { get; set; }

    }

    internal enum ConversationType : int
    {
        CONVERSATION_TYPE_UNKNOWN = 0,
        // Conversation is one-to-one (only 2 participants).
        CONVERSATION_TYPE_ONE_TO_ONE = 1,
        // Conversation is group (any number of participants).
        CONVERSATION_TYPE_GROUP = 2
    }

    [ProtoContract]
    internal class UserReadState
    {
        [ProtoMember(Position = 1)]
        public ParticipantId participant_id { get; set; }

        [ProtoMember(Position = 2)]
        public long latest_read_timestamp { get; set; }


        // TODO: is latest_read_timestamp always 0?
    }

    internal enum ConversationStatus : int
    {
        CONVERSATION_STATUS_UNKNOWN = 0,
        // User is invited to conversation.
        CONVERSATION_STATUS_INVITED = 1,
        // User is participating in conversation.
        CONVERSATION_STATUS_ACTIVE = 2,
        // User has left conversation.
        CONVERSATION_STATUS_LEFT = 3
    }

    internal enum ConversationView : int
    {
        CONVERSATION_VIEW_UNKNOWN = 0,
        // Conversation is in inbox.
        CONVERSATION_VIEW_INBOX = 1,
        // Conversation has been archived.
        CONVERSATION_VIEW_ARCHIVED = 2
    }

    internal enum DeliveryMediumType : int
    {
        DELIVERY_MEDIUM_UNKNOWN = 0,
        DELIVERY_MEDIUM_BABEL = 1,
        DELIVERY_MEDIUM_GOOGLE_VOICE = 2,
        DELIVERY_MEDIUM_LOCAL_SMS = 3
    }

    [ProtoContract]
    internal class DeliveryMedium
    {
        [ProtoMember(Position = 1)]
        public DeliveryMediumType medium_type { get; set; }

        // Phone number to use for sending Google Voice messages.
        [ProtoMember(Position = 2, Optional = true)]
        public Phone phone { get; set; }

    }

    [ProtoContract]
    internal class DeliveryMediumOption
    {
        [ProtoMember(Position = 1)]
        public DeliveryMedium delivery_medium { get; set; }

        [ProtoMember(Position = 2)]
        public bool current_default { get; set; }

    }

    [ProtoContract]
    internal class UserConversationState
    {
        [ProtoMember(Position = 2)]
        public string client_generated_id { get; set; }

        [ProtoMember(Position = 7)]
        public UserReadState self_read_state { get; set; }

        [ProtoMember(Position = 8)]
        public ConversationStatus status { get; set; }

        [ProtoMember(Position = 9)]
        public NotificationLevel notification_level { get; set; }

        [ProtoMember(Position = 10)]
        public List<ConversationView> view { get; set; }

        [ProtoMember(Position = 11)]
        public ParticipantId inviter_id { get; set; }

        [ProtoMember(Position = 12)]
        public long invite_timestamp { get; set; }

        [ProtoMember(Position = 13)]
        public long sort_timestamp { get; set; }

        [ProtoMember(Position = 14)]
        public long active_timestamp { get; set; }

        [ProtoMember(Position = 17)]
        public List<DeliveryMediumOption> delivery_medium_option { get; set; }

    }

    internal enum ParticipantType : int
    {
        PARTICIPANT_TYPE_UNKNOWN = 0,
        PARTICIPANT_TYPE_GAIA = 2
    }

    internal enum InvitationStatus : int
    {
        INVITATION_STATUS_UNKNOWN = 0,
        INVITATION_STATUS_PENDING = 1,
        INVITATION_STATUS_ACCEPTED = 2
    }

    [ProtoContract]
    internal class ConversationParticipantData
    {
        [ProtoMember(Position = 1)]
        public ParticipantId id { get; set; }

        [ProtoMember(Position = 2)]
        public string fallback_name { get; set; }

        [ProtoMember(Position = 3)]
        public InvitationStatus invitation_status { get; set; }

        [ProtoMember(Position = 5)]
        public ParticipantType participant_type { get; set; }

        [ProtoMember(Position = 6)]
        public InvitationStatus new_invitation_status { get; set; }

    }

    internal enum ForceHistory : int
    {
        FORCE_HISTORY_UNKNOWN = 0,
        FORCE_HISTORY_NO = 1
    }

    internal enum NetworkType : int
    {
        NETWORK_TYPE_UNKNOWN = 0,
        NETWORK_TYPE_BABEL = 1
    }

    // A conversation between two or more users.
    [ProtoContract]
    internal class Conversation
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationType type { get; set; }

        [ProtoMember(Position = 3)]
        public string name { get; set; }

        [ProtoMember(Position = 4)]
        public UserConversationState self_conversation_state { get; set; }

        [ProtoMember(Position = 8)]
        public List<UserReadState> read_state { get; set; }

        // True if the conversation has an active Hangout.
        [ProtoMember(Position = 9)]
        public bool has_active_hangout { get; set; }

        // The conversation's "off the record" status.
        [ProtoMember(Position = 10)]
        public OffTheRecordStatus otr_status { get; set; }

        // Whether the OTR toggle is available to the user for this conversation.
        [ProtoMember(Position = 11)]
        public OffTheRecordToggle otr_toggle { get; set; }

        [ProtoMember(Position = 12)]
        public bool conversation_history_supported { get; set; }

        [ProtoMember(Position = 13)]
        public List<ParticipantId> current_participant { get; set; }

        [ProtoMember(Position = 14)]
        public List<ConversationParticipantData> participant_data { get; set; }

        [ProtoMember(Position = 18)]
        public List<NetworkType> network_type { get; set; }

        [ProtoMember(Position = 19)]
        public ForceHistory force_history_state { get; set; }

    }

    [ProtoContract]
    internal class EasterEgg
    {
        [ProtoMember(Position = 1)]
        public string message { get; set; }

    }

    internal enum BlockState : int
    {
        BLOCK_STATE_UNKNOWN = 0,
        BLOCK_STATE_BLOCK = 1,
        BLOCK_STATE_UNBLOCK = 2
    }

    [ProtoContract]
    internal class BlockStateChange
    {
        [ProtoMember(Position = 1)]
        public ParticipantId participant_id { get; set; }

        [ProtoMember(Position = 2)]
        public BlockState new_block_state { get; set; }

    }

    internal enum ReplyToInviteType : int
    {
        REPLY_TO_INVITE_TYPE_UNKNOWN = 0,
        REPLY_TO_INVITE_TYPE_ACCEPT = 1,
        REPLY_TO_INVITE_TYPE_DECLINE = 2
    }

    [ProtoContract]
    internal class Photo
    {
        [ProtoMember(Position = 1)]
        public string photo_id { get; set; }

        [ProtoMember(Position = 2)]
        public bool delete_albumless_source_photo { get; set; }


        // TODO: test delete_albumless_source_photo
    }

    [ProtoContract]
    internal class ExistingMedia
    {
        [ProtoMember(Position = 1)]
        public Photo photo { get; set; }

    }

    [ProtoContract]
    internal class EventRequestHeader
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public string client_generated_id { get; set; }

        [ProtoMember(Position = 3)]
        public OffTheRecordStatus expected_otr { get; set; }

        [ProtoMember(Position = 4)]
        public DeliveryMedium delivery_medium { get; set; }

        [ProtoMember(Position = 5)]
        public EventType event_type { get; set; }

    }

    // Identifies the client.
    internal enum ClientId : int
    {
        CLIENT_ID_UNKNOWN = 0,
        // Hangouts app for Android.
        CLIENT_ID_ANDROID = 1,
        // Hangouts app for iOS.
        CLIENT_ID_IOS = 2,
        // Hangouts Chrome extension.
        CLIENT_ID_CHROME = 3,
        // Hangouts web interface in Google Plus.
        CLIENT_ID_WEB_GPLUS = 5,
        // Hangouts web interface in Gmail.
        CLIENT_ID_WEB_GMAIL = 6,
        // Hangouts Chrome app ("ultraviolet").
        CLIENT_ID_ULTRAVIOLET = 13
    }

    // Build type of the client.
    internal enum ClientBuildType : int
    {
        BUILD_TYPE_UNKNOWN = 0,
        // Web app.
        BUILD_TYPE_PRODUCTION_WEB = 1,
        // Native app.
        BUILD_TYPE_PRODUCTION_APP = 3
    }

    // The client and device version.
    [ProtoContract]
    internal class ClientVersion
    {
        // Identifies the client.
        [ProtoMember(Position = 1)]
        public ClientId client_id { get; set; }

        // The client build type.
        [ProtoMember(Position = 2)]
        public ClientBuildType build_type { get; set; }

        // Client version.
        [ProtoMember(Position = 3, Optional = true)]
        public string major_version { get; set; }

        // Client version timestamp.
        [ProtoMember(Position = 4)]
        public long version_timestamp { get; set; }

        // OS version string (for native apps).
        [ProtoMember(Position = 5, Optional = true)]
        public string device_os_version { get; set; }

        // Device hardware name (for native apps).
        [ProtoMember(Position = 6, Optional = true)]
        public string device_hardware { get; set; }

    }

    // Header for requests from the client to the server.
    [ProtoContract]
    internal class RequestHeader
    {
        [ProtoMember(Position = 1)]
        public ClientVersion client_version { get; set; }

        [ProtoMember(Position = 2)]
        public ClientIdentifier client_identifier { get; set; }

        [ProtoMember(Position = 3)]
        public object unknow { get; set; }

        [ProtoMember(Position = 4)]
        public string language_code { get; set; }


        // TODO: incomplete
    }

    // Status of the response from the server to the client.
    internal enum ResponseStatus : int
    {
        RESPONSE_STATUS_UNKNOWN = 0,
        RESPONSE_STATUS_OK = 1,
        RESPONSE_STATUS_UNEXPECTED_ERROR = 3,
        RESPONSE_STATUS_INVALID_REQUEST = 4,
    }

    // Header for responses from the server to the client.
    [ProtoContract]
    internal class ResponseHeader
    {
        [ProtoMember(Position = 1)]
        public ResponseStatus status { get; set; }

        [ProtoMember(Position = 2)]
        public string error_description { get; set; }

        [ProtoMember(Position = 3)]
        public string debug_url { get; set; }

        [ProtoMember(Position = 4)]
        public string request_trace_id { get; set; }

        [ProtoMember(Position = 5)]
        public long current_server_time { get; set; }

    }

    // A user that can participate in conversations.
    [ProtoContract]
    internal class Entity
    {
        // The user's ID.
        [ProtoMember(Position = 9)]
        public ParticipantId id { get; set; }

        // Optional user presence status.
        [ProtoMember(Position = 8)]
        public Presence presence { get; set; }

        // Optional user properties.
        [ProtoMember(Position = 10)]
        public EntityProperties properties { get; set; }

        [ProtoMember(Position = 13)]
        public ParticipantType entity_type { get; set; }

        internal enum PastHangoutState
        {
            PAST_HANGOUT_STATE_UNKNOWN = 0,
            PAST_HANGOUT_STATE_HAD_PAST_HANGOUT = 1,
            PAST_HANGOUT_STATE_NO_PAST_HANGOUT = 2,
        }

        [ProtoMember(Position = 16)]
        public PastHangoutState had_past_hangout_state { get; set; }

        // TODO:
        // unknown 1 = 15;
        // unknown 2 = 17;
    }

    [ProtoContract]
    internal class EntityProperties
    {
        [ProtoMember(Position = 1)]
        public ProfileType type { get; set; }

        [ProtoMember(Position = 2)]
        public string display_name { get; set; }

        [ProtoMember(Position = 3)]
        public string first_name { get; set; }

        [ProtoMember(Position = 4)]
        public string photo_url { get; set; }

        [ProtoMember(Position = 5)]
        public List<string> email { get; set; }

        [ProtoMember(Position = 6)]
        public List<string> phone { get; set; }

        [ProtoMember(Position = 10)]
        public bool in_users_domain { get; set; }

        [ProtoMember(Position = 11)]
        public Gender gender { get; set; }

        [ProtoMember(Position = 12)]
        public PhotoUrlStatus photo_url_status { get; set; }

        [ProtoMember(Position = 15)]
        public string canonical_email { get; set; }

    }

    // Status of EntityProperties.photo_url.
    internal enum PhotoUrlStatus : int
    {
        PHOTO_URL_STATUS_UNKNOWN = 0,
        // URL is a placeholder.
        PHOTO_URL_STATUS_PLACEHOLDER = 1,
        // URL is a photo set by the user.
        PHOTO_URL_STATUS_USER_PHOTO = 2,
    }

    internal enum Gender : int
    {
        GENDER_UNKNOWN = 0,
        GENDER_MALE = 1,
        GENDER_FEMALE = 2,
    }

    internal enum ProfileType : int
    {
        PROFILE_TYPE_NONE = 0,
        PROFILE_TYPE_ES_USER = 1,
    }

    // State of a conversation and recent events.
    [ProtoContract]
    internal class ConversationState
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public Conversation conversation { get; set; }

        [ProtoMember(Position = 3)]
        public List<Event> events { get; set; }

        [ProtoMember(Position = 5)]
        public EventContinuationToken event_continuation_token { get; set; }
    }

    // Token that allows retrieving more events from a position in a conversation.
    // Specifying event_timestamp is sufficient.
    [ProtoContract]
    internal class EventContinuationToken
    {
        [ProtoMember(Position = 1)]
        public string event_id { get; set; }

        [ProtoMember(Position = 2)]
        public string storage_continuation_token { get; set; }

        [ProtoMember(Position = 3)]
        public long event_timestamp { get; set; }

    }

    [ProtoContract]
    internal class EntityLookupSpec
    {
        [ProtoMember(Position = 1)]
        public string gaia_id { get; set; }


        // TODO
    }

    // A type of binary configuration option.
    internal enum ConfigurationBitType : int
    {

        // TODO
        // RICH_PRESENCE_ACTIVITY_PROMO_SHOWN
        // RICH_PRESENCE_DEVICE_PROMO_SHOWN
        // RICH_PRESENCE_LAST_SEEN_DESKTOP_PROMO_SHOWN
        // RICH_PRESENCE_LAST_SEEN_MOBILE_PROMO_SHOWN
        // RICH_PRESENCE_IN_CALL_STATE_PROMO_SHOWN
        // RICH_PRESENCE_MOOD_PROMO_SHOWN
        // GV_SMS_INTEGRATION_PROMO_SHOWN
        // RICH_PRESENCE_LAST_SEEN_DESKTOP_PROMPT_SHOWN
        // BUSINESS_FEATURES_ENABLED
        // BUSINESS_FEATURES_PROMO_DISMISSED
        // CONVERSATION_INVITE_SETTINGS_SET_TO_CUSTOM
        // REPORT_ABUSE_NOTICE_ACKNOWLEDGED
        // PHONE_VERIFICATION_MOBILE_PROMPT_SHOWN
        // HANGOUT_P2P_NOTICE_ACKNOWLEDGED
        // HANGOUT_P2P_ENABLED
        // INVITE_NOTIFICATIONS_ENABLED
        // DESKTOP_AUTO_EMOJI_CONVERSION_ENABLED
        // ALLOWED_FOR_DOMAIN
        // GMAIL_CHAT_ARCHIVE_ENABLED
        // QUASAR_MARKETING_PROMO_DISMISSED
        // GPLUS_SIGNUP_PROMO_DISMISSED
        // GPLUS_UPGRADE_ALLOWED_FOR_DOMAIN
        // CHAT_WITH_CIRCLES_ACCEPTED
        // CHAT_WITH_CIRCLES_PROMO_DISMISSED
        // PHOTO_SERVICE_REGISTERED
        // GV_SMS_INTEGRATION_ENABLED
        // CAN_OPT_INTO_GV_SMS_INTEGRATION
        // BUSINESS_FEATURES_ELIGIBLE
        // CAN_USE_GV_CALLER_ID_FEATURE

        CONFIGURATION_BIT_TYPE_UNKNOWN = 0,
        CONFIGURATION_BIT_TYPE_UNKNOWN_1 = 1,
        CONFIGURATION_BIT_TYPE_UNKNOWN_2 = 2,
        CONFIGURATION_BIT_TYPE_UNKNOWN_3 = 3,
        CONFIGURATION_BIT_TYPE_UNKNOWN_4 = 4,
        CONFIGURATION_BIT_TYPE_UNKNOWN_5 = 5,
        CONFIGURATION_BIT_TYPE_UNKNOWN_6 = 6,
        CONFIGURATION_BIT_TYPE_UNKNOWN_7 = 7,
        CONFIGURATION_BIT_TYPE_UNKNOWN_8 = 8,
        CONFIGURATION_BIT_TYPE_UNKNOWN_9 = 9,
        CONFIGURATION_BIT_TYPE_UNKNOWN_10 = 10,
        CONFIGURATION_BIT_TYPE_UNKNOWN_11 = 11,
        CONFIGURATION_BIT_TYPE_UNKNOWN_12 = 12,
        CONFIGURATION_BIT_TYPE_UNKNOWN_13 = 13,
        CONFIGURATION_BIT_TYPE_UNKNOWN_14 = 14,
        CONFIGURATION_BIT_TYPE_UNKNOWN_15 = 15,
        CONFIGURATION_BIT_TYPE_UNKNOWN_16 = 16,
        CONFIGURATION_BIT_TYPE_UNKNOWN_17 = 17,
        CONFIGURATION_BIT_TYPE_UNKNOWN_18 = 18,
        CONFIGURATION_BIT_TYPE_UNKNOWN_19 = 19,
        CONFIGURATION_BIT_TYPE_UNKNOWN_20 = 20,
        CONFIGURATION_BIT_TYPE_UNKNOWN_21 = 21,
        CONFIGURATION_BIT_TYPE_UNKNOWN_22 = 22,
        CONFIGURATION_BIT_TYPE_UNKNOWN_23 = 23,
        CONFIGURATION_BIT_TYPE_UNKNOWN_24 = 24,
        CONFIGURATION_BIT_TYPE_UNKNOWN_25 = 25,
        CONFIGURATION_BIT_TYPE_UNKNOWN_26 = 26,
        CONFIGURATION_BIT_TYPE_UNKNOWN_27 = 27,
        CONFIGURATION_BIT_TYPE_UNKNOWN_28 = 28,
        CONFIGURATION_BIT_TYPE_UNKNOWN_29 = 29,
        CONFIGURATION_BIT_TYPE_UNKNOWN_30 = 30,
        CONFIGURATION_BIT_TYPE_UNKNOWN_31 = 31,
        CONFIGURATION_BIT_TYPE_UNKNOWN_32 = 32,
        CONFIGURATION_BIT_TYPE_UNKNOWN_33 = 33,
        CONFIGURATION_BIT_TYPE_UNKNOWN_34 = 34,
        CONFIGURATION_BIT_TYPE_UNKNOWN_35 = 35
    }

    [ProtoContract]
    internal class ConfigurationBit
    {
        [ProtoMember(Position = 1)]
        public ConfigurationBitType configuration_bit_type { get; set; }

        [ProtoMember(Position = 2)]
        public bool value { get; set; }

    }

    internal enum RichPresenceType : int
    {
        RICH_PRESENCE_TYPE_UNKNOWN = 0,
        RICH_PRESENCE_TYPE_IN_CALL_STATE = 1,

        // TODO
        // RICH_PRESENCE_TYPE_GLOBALLY_ENABLED
        // RICH_PRESENCE_TYPE_ACTIVITY
        // RICH_PRESENCE_TYPE_MOOD

        RICH_PRESENCE_TYPE_UNKNOWN_3 = 3,
        RICH_PRESENCE_TYPE_UNKNOWN_4 = 4,
        RICH_PRESENCE_TYPE_UNKNOWN_5 = 5,

        RICH_PRESENCE_TYPE_DEVICE = 2,
        RICH_PRESENCE_TYPE_LAST_SEEN = 6
    }

    [ProtoContract]
    internal class RichPresenceState
    {
        [ProtoMember(Position = 3)]
        public List<RichPresenceEnabledState> get_rich_presence_enabled_state { get; set; }

    }

    [ProtoContract]
    internal class RichPresenceEnabledState
    {
        [ProtoMember(Position = 1)]
        public RichPresenceType type { get; set; }

        [ProtoMember(Position = 2)]
        public bool enabled { get; set; }

    }

    internal enum FieldMask : int
    {
        FIELD_MASK_REACHABLE = 1,
        FIELD_MASK_AVAILABLE = 2,
        FIELD_MASK_MOOD = 3,
        FIELD_MASK_IN_CALL = 6,
        FIELD_MASK_DEVICE = 7,
        FIELD_MASK_LAST_SEEN = 10
    }

    [ProtoContract]
    internal class DesktopOffSetting
    {
        // State of "desktop off" setting.
        [ProtoMember(Position = 1)]
        public bool desktop_off { get; set; }

    }

    [ProtoContract]
    internal class DesktopOffState
    {
        // Whether Hangouts desktop is signed off or on.
        [ProtoMember(Position = 1)]
        public bool desktop_off { get; set; }

        [ProtoMember(Position = 2)]
        public long version { get; set; }

    }

    // Enable or disable do-not-disturb mode. Not to be confused with
    // DoNotDisturbSetting, which is used to indicate the state of do-not-disturb
    // mode.
    [ProtoContract]
    internal class DndSetting
    {
        // Whether to enable or disable do-not-disturb mode.
        [ProtoMember(Position = 1)]
        public bool do_not_disturb { get; set; }

        // Do not disturb expiration in seconds.
        [ProtoMember(Position = 2)]
        public long timeout_secs { get; set; }

    }

    [ProtoContract]
    internal class PresenceStateSetting
    {
        [ProtoMember(Position = 1)]
        public long timeout_secs { get; set; }

        [ProtoMember(Position = 2)]
        public ClientPresenceStateType type { get; set; }

    }

    [ProtoContract]
    internal class MoodMessage
    {
        [ProtoMember(Position = 1)]
        public MoodContent mood_content { get; set; }

    }

    [ProtoContract]
    internal class MoodContent
    {
        [ProtoMember(Position = 1)]
        public List<Segment> segment { get; set; }

    }

    // The user's mood message.
    [ProtoContract]
    internal class MoodSetting
    {
        [ProtoMember(Position = 1)]
        public MoodMessage mood_message { get; set; }

    }

    [ProtoContract]
    internal class LastSeen
    {
        [ProtoMember(Position = 1)]
        public long last_seen_timestamp_usec { get; set; }
    }

    [ProtoContract]
    internal class MoodState
    {
        [ProtoMember(Position = 4)]
        public MoodSetting mood_setting { get; set; }

    }

    internal enum DeleteType : int
    {
        DELETE_TYPE_UNKNOWN = 0,
        DELETE_TYPE_UPPER_BOUND = 1
    }

    [ProtoContract]
    internal class DeleteAction
    {
        [ProtoMember(Position = 1)]
        public long delete_action_timestamp { get; set; }

        [ProtoMember(Position = 2)]
        public long delete_upper_bound_timestamp { get; set; }

        [ProtoMember(Position = 3)]
        public DeleteType delete_type { get; set; }

    }

    [ProtoContract]
    internal class InviteeID
    {
        [ProtoMember(Position = 1)]
        public string gaia_id { get; set; }

        [ProtoMember(Position = 4)]
        public string fallback_name { get; set; }

    }

    internal enum SyncFilter : int
    {
        SYNC_FILTER_UNKNOWN = 0,
        SYNC_FILTER_INBOX = 1,
        SYNC_FILTER_ARCHIVED = 2

        // TODO
    }

    // Describes a user's country.
    [ProtoContract]
    internal class Country
    {
        // Abbreviated region code (eg. "CA").
        [ProtoMember(Position = 1)]
        public string region_code { get; set; }

        // Country's calling code (eg. "1").
        [ProtoMember(Position = 2)]
        public long country_code { get; set; }

    }

    internal enum SoundState : int
    {
        SOUND_STATE_UNKNOWN = 0,
        SOUND_STATE_ON = 1,
        SOUND_STATE_OFF = 2
    }

    // Sound settings in the desktop Hangouts client.
    [ProtoContract]
    internal class DesktopSoundSetting
    {
        // Whether to play sound for incoming messages.
        [ProtoMember(Position = 1)]
        public SoundState desktop_sound_state { get; set; }

        // Whether to ring for incoming calls.
        [ProtoMember(Position = 2)]
        public SoundState desktop_ring_sound_state { get; set; }

    }

    internal enum CallerIdSettingsMask : int
    {
        CALLER_ID_SETTINGS_MASK_UNKNOWN = 0,
        CALLER_ID_SETTINGS_MASK_PROVIDED = 1
    }

    [ProtoContract]
    internal class PhoneData
    {
        [ProtoMember(Position = 1)]
        public List<Phone> phone { get; set; }

        [ProtoMember(Position = 3)]
        public CallerIdSettingsMask caller_id_settings_mask { get; set; }

    }

    internal enum PhoneVerificationStatus : int
    {
        PHONE_VERIFICATION_STATUS_UNKNOWN = 0,
        PHONE_VERIFICATION_STATUS_VERIFIED = 1
    }

    internal enum PhoneDiscoverabilityStatus : int
    {
        PHONE_DISCOVERABILITY_STATUS_UNKNOWN = 0,
        PHONE_DISCOVERABILITY_STATUS_OPTED_IN_BUT_NOT_DISCOVERABLE = 2
    }

    [ProtoContract]
    internal class Phone
    {
        [ProtoMember(Position = 1)]
        public PhoneNumber phone_number { get; set; }

        [ProtoMember(Position = 2)]
        public bool google_voice { get; set; }

        [ProtoMember(Position = 3)]
        public PhoneVerificationStatus verification_status { get; set; }

        [ProtoMember(Position = 4)]
        public bool discoverable { get; set; }

        [ProtoMember(Position = 5)]
        public PhoneDiscoverabilityStatus discoverability_status { get; set; }

        [ProtoMember(Position = 6)]
        public bool primary { get; set; }

    }

    internal enum PhoneValidationResult : int
    {
        PHONE_VALIDATION_RESULT_IS_POSSIBLE = 0
    }

    [ProtoContract]
    internal class I18nData
    {
        [ProtoMember(Position = 1)]
        public string national_number { get; set; }

        [ProtoMember(Position = 2)]
        public string international_number { get; set; }

        [ProtoMember(Position = 3)]
        public long country_code { get; set; }

        [ProtoMember(Position = 4)]
        public string region_code { get; set; }

        [ProtoMember(Position = 5)]
        public bool is_valid { get; set; }

        [ProtoMember(Position = 6)]
        public PhoneValidationResult validation_result { get; set; }

    }

    [ProtoContract]
    internal class PhoneNumber
    {
        // Phone number as string (eg. "+15551234567").
        [ProtoMember(Position = 1)]
        public string e164 { get; set; }

        [ProtoMember(Position = 2)]
        public I18nData i18n_data { get; set; }

    }

    [ProtoContract]
    internal class SuggestedContactGroupHash
    {
        // Number of results to return from this group.
        [ProtoMember(Position = 1)]
        public long max_results { get; set; }

        // A 4-byte hash.
        [ProtoMember(Position = 2)]
        public string hash { get; set; }

    }

    [ProtoContract]
    internal class SuggestedContact
    {
        [ProtoMember(Position = 1)]
        public Entity entity { get; set; }

        [ProtoMember(Position = 2)]
        public InvitationStatus invitation_status { get; set; }

    }

    [ProtoContract]
    internal class SuggestedContactGroup
    {
        [ProtoMember(Position = 1)]
        public bool hash_matched { get; set; }

        // A 4-byte hash.
        [ProtoMember(Position = 2)]
        public string hash { get; set; }

        [ProtoMember(Position = 3)]
        public List<SuggestedContact> contact { get; set; }

    }

    // ----------------------------------------------------------------------------
    // State Update and Notifications
    // ----------------------------------------------------------------------------

    // Pushed from the server to the client to notify it of state changes. Includes
    // exactly one type of notification, and optionally updates the attributes of a
    // conversation.
    [ProtoContract]
    internal class StateUpdate
    {

        [ProtoMember(Position = 1)]
        public StateUpdateHeader state_update_header { get; set; }


        // If set, includes conversation attributes that have been updated by the
        // notification.
        [ProtoMember(Position = 13)]
        public Conversation conversation { get; set; }


        [ProtoMember(Position = 3)]
        public EventNotification event_notification { get; set; }
        [ProtoMember(Position = 4)]
        public SetFocusNotification focus_notification { get; set; }
        [ProtoMember(Position = 5)]
        public SetTypingNotification typing_notification { get; set; }
        [ProtoMember(Position = 6)]
        public SetConversationNotificationLevelNotification notification_level_notification { get; set; }
        [ProtoMember(Position = 7)]
        public ReplyToInviteNotification reply_to_invite_notification { get; set; }
        [ProtoMember(Position = 8)]
        public WatermarkNotification watermark_notification { get; set; }

        //UnimplementedMessage unknown_1 = 9;
        //UnimplementedMessage settings_notification = 10;

        // TODO: rename to ViewModificationNotification?

        [ProtoMember(Position = 11)]
        public ConversationViewModification view_modification { get; set; }
        [ProtoMember(Position = 12)]
        public EasterEggNotification easter_egg_notification { get; set; }
        [ProtoMember(Position = 14)]
        public SelfPresenceNotification self_presence_notification { get; set; }
        [ProtoMember(Position = 15)]
        public DeleteActionNotification delete_notification { get; set; }
        [ProtoMember(Position = 16)]
        public PresenceNotification presence_notification { get; set; }
        [ProtoMember(Position = 17)]
        public BlockNotification block_notification { get; set; }

        //UnimplementedMessage invitation_watermark_notification = 18;

        [ProtoMember(Position = 19)]
        public SetNotificationSettingNotification notification_setting_notification { get; set; }
        [ProtoMember(Position = 20)]
        public RichPresenceEnabledStateNotification rich_presence_enabled_state_notification { get; set; }
    }

    // Header for StateUpdate messages.
    [ProtoContract]
    internal class StateUpdateHeader
    {
        [ProtoMember(Position = 1)]
        public ActiveClientState active_client_state { get; set; }

        [ProtoMember(Position = 3)]
        public string request_trace_id { get; set; }

        [ProtoMember(Position = 4)]
        public NotificationSettings notification_settings { get; set; }

        [ProtoMember(Position = 5)]
        public long current_server_time { get; set; }


        // TODO:
        // unknown = 2
        // archive settings? ([1]) = 6
        // unknown = 7
        // optional ID of the client causing the update (3767219427742586121) ? = 8
    }

    // List of StateUpdate messages to allow pushing multiple notifications from
    // the server to the client simultaneously.
    [ProtoContract]
    internal class BatchUpdate
    {
        [ProtoMember(Position = 1)]
        public List<StateUpdate> state_update { get; set; }

    }

    [ProtoContract]
    internal class EventNotification
    {
        [ProtoMember(Position = 1)]
        public Event current_event { get; set; }

    }

    [ProtoContract]
    internal class SetFocusNotification
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public ParticipantId sender_id { get; set; }

        [ProtoMember(Position = 3)]
        public long timestamp { get; set; }

        [ProtoMember(Position = 4)]
        public FocusType type { get; set; }

        [ProtoMember(Position = 5)]
        public FocusDevice device { get; set; }

    }

    [ProtoContract]
    internal class SetTypingNotification
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public ParticipantId sender_id { get; set; }

        [ProtoMember(Position = 3)]
        public long timestamp { get; set; }

        [ProtoMember(Position = 4)]
        public TypingType type { get; set; }

    }

    [ProtoContract]
    internal class SetConversationNotificationLevelNotification
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public NotificationLevel level { get; set; }

        [ProtoMember(Position = 4)]
        public long timestamp { get; set; }


        // TODO:
        // unknown (0) = 3;
    }

    [ProtoContract]
    internal class ReplyToInviteNotification
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public ReplyToInviteType type { get; set; }


        // TODO: untested
    }

    [ProtoContract]
    internal class WatermarkNotification
    {
        [ProtoMember(Position = 1)]
        public ParticipantId sender_id { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public long latest_read_timestamp { get; set; }

    }

    [ProtoContract]
    internal class ConversationViewModification
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationView old_view { get; set; }

        [ProtoMember(Position = 3)]
        public ConversationView new_view { get; set; }

    }

    [ProtoContract]
    internal class EasterEggNotification
    {
        [ProtoMember(Position = 1)]
        public ParticipantId sender_id { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public EasterEgg easter_egg { get; set; }

    }

    // Notifies the status of other clients and mood.
    [ProtoContract]
    internal class SelfPresenceNotification
    {
        [ProtoMember(Position = 1)]
        public ClientPresenceState client_presence_state { get; set; }

        [ProtoMember(Position = 3)]
        public DoNotDisturbSetting do_not_disturb_setting { get; set; }

        [ProtoMember(Position = 4)]
        public DesktopOffSetting desktop_off_setting { get; set; }

        [ProtoMember(Position = 5)]
        public DesktopOffState desktop_off_state { get; set; }

        [ProtoMember(Position = 6)]
        public MoodState mood_state { get; set; }

    }

    [ProtoContract]
    internal class DeleteActionNotification
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 2)]
        public DeleteAction delete_action { get; set; }

    }

    [ProtoContract]
    internal class PresenceNotification
    {
        [ProtoMember(Position = 1)]
        public List<PresenceResult> presence { get; set; }

    }

    [ProtoContract]
    internal class BlockNotification
    {
        [ProtoMember(Position = 1)]
        public List<BlockStateChange> block_state_change { get; set; }

    }

    [ProtoContract]
    internal class SetNotificationSettingNotification
    {
        [ProtoMember(Position = 2)]
        public DesktopSoundSetting desktop_sound_setting { get; set; }


        // TODO
        // convert text to emoji setting
    }

    [ProtoContract]
    internal class RichPresenceEnabledStateNotification
    {
        [ProtoMember(Position = 1)]
        public List<RichPresenceEnabledState> rich_presence_enabled_state { get; set; }

    }

    [ProtoContract]
    internal class ConversationSpec
    {
        [ProtoMember(Position = 1)]
        public ConversationId conversation_id { get; set; }


        // TODO
    }

    // ----------------------------------------------------------------------------
    // Requests & Responses
    // ----------------------------------------------------------------------------

    [ProtoContract]
    internal class AddUserRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 3)]
        public List<InviteeID> invitee_id { get; set; }

        [ProtoMember(Position = 5)]
        public EventRequestHeader event_request_header { get; set; }

    }

    [ProtoContract]
    internal class AddUserResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 5)]
        public Event created_event { get; set; }

    }

    [ProtoContract]
    internal class CreateConversationRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationType type { get; set; }

        [ProtoMember(Position = 3)]
        public long client_generated_id { get; set; }

        [ProtoMember(Position = 4)]
        public string name { get; set; }

        [ProtoMember(Position = 5)]
        public List<InviteeID> invitee_id { get; set; }

    }

    [ProtoContract]
    internal class CreateConversationResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public Conversation conversation { get; set; }

        [ProtoMember(Position = 7)]
        public bool new_conversation_created { get; set; }

    }

    [ProtoContract]
    internal class DeleteConversationRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public long delete_upper_bound_timestamp { get; set; }

    }

    [ProtoContract]
    internal class DeleteConversationResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public DeleteAction delete_action { get; set; }

    }

    [ProtoContract]
    internal class EasterEggRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public EasterEgg easter_egg { get; set; }

    }

    [ProtoContract]
    internal class EasterEggResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public long timestamp { get; set; }

    }

    [ProtoContract]
    internal class GetConversationRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationSpec conversation_spec { get; set; }

        [ProtoMember(Position = 4)]
        public bool include_event { get; set; }

        [ProtoMember(Position = 6)]
        public long max_events_per_conversation { get; set; }

        [ProtoMember(Position = 7)]
        public EventContinuationToken event_continuation_token { get; set; }


        // TODO:
        // include_conversation_metadata? = 3;
        // unknown = 5;
    }

    [ProtoContract]
    internal class GetConversationResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationState conversation_state { get; set; }


        // TODO
    }

    [ProtoContract]
    internal class GetEntityByIdRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        // unknown = 2;
        [ProtoMember(Position = 2)]
        public object unknown1 { get; set; }

        [ProtoMember(Position = 3)]
        public List<EntityLookupSpec> batch_lookup_spec { get; set; }


        // TODO
        [ProtoMember(Position = 4, Optional = true)]
        public List<FieldMask> field_mask { get; set; }
    }

    [ProtoContract]
    internal class GetEntityByIdResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public List<Entity> entity { get; set; }


        // TODO
    }

    [ProtoContract]
    internal class GetSuggestedEntitiesRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 8)]
        public SuggestedContactGroupHash favorites { get; set; }

        [ProtoMember(Position = 9)]
        public SuggestedContactGroupHash contacts_you_hangout_with { get; set; }

        [ProtoMember(Position = 10)]
        public SuggestedContactGroupHash other_contacts_on_hangouts { get; set; }

        [ProtoMember(Position = 11)]
        public SuggestedContactGroupHash other_contacts { get; set; }

        [ProtoMember(Position = 12)]
        public SuggestedContactGroupHash dismissed_contacts { get; set; }

        [ProtoMember(Position = 13)]
        public SuggestedContactGroupHash pinned_favorites { get; set; }


        // TODO: possible other fields:
        // unknown bytes = 2;
        // unknown bytes = 3;
        // unknown int/enum = 4;
        // unknown bool = 5;
        //  [ProtoMember(Position=6)]
        public List<ParticipantId> unknown { get; set; }

        // unknown bool = 7;
    }
    // cgserp
    [ProtoContract]
    internal class GetSuggestedEntitiesResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public List<Entity> entity { get; set; }

        [ProtoMember(Position = 4)]
        public SuggestedContactGroup favorites { get; set; }

        [ProtoMember(Position = 5)]
        public SuggestedContactGroup contacts_you_hangout_with { get; set; }

        [ProtoMember(Position = 6)]
        public SuggestedContactGroup other_contacts_on_hangouts { get; set; }

        [ProtoMember(Position = 7)]
        public SuggestedContactGroup other_contacts { get; set; }

        [ProtoMember(Position = 8)]
        public SuggestedContactGroup dismissed_contacts { get; set; }

        [ProtoMember(Position = 9)]
        public SuggestedContactGroup pinned_favorites { get; set; }

    }

    [ProtoContract]
    internal class GetSelfInfoRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }


        // TODO
    }

    [ProtoContract]
    internal class GetSelfInfoResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public Entity self_entity { get; set; }

        [ProtoMember(Position = 3)]
        public bool is_known_minor { get; set; }

        [ProtoMember(Position = 5)]
        public DoNotDisturbSetting dnd_state { get; set; }

        [ProtoMember(Position = 6)]
        public DesktopOffSetting desktop_off_setting { get; set; }

        [ProtoMember(Position = 7)]
        public PhoneData phone_data { get; set; }

        [ProtoMember(Position = 8)]
        public List<ConfigurationBit> configuration_bit { get; set; }

        [ProtoMember(Position = 9)]
        public DesktopOffState desktop_off_state { get; set; }

        [ProtoMember(Position = 10)]
        public bool google_plus_user { get; set; }

        [ProtoMember(Position = 11)]
        public DesktopSoundSetting desktop_sound_setting { get; set; }

        [ProtoMember(Position = 12)]
        public RichPresenceState rich_presence_state { get; set; }

        [ProtoMember(Position = 19)]
        public Country default_country { get; set; }


        // TODO:
        // client_presence? = 4;
        // babel_user? = 13;
        // desktop_availability_sharing_enabled? = 14;
        // unknown = 16;
        // bool google_plus_mobile_user = 15;
        // unknown = 18;
        // unknown = 22;
    }

    [ProtoContract]
    internal class QueryPresenceRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public List<ParticipantId> participant_id { get; set; }

        [ProtoMember(Position = 3)]
        public List<FieldMask> field_mask { get; set; }

    }

    [ProtoContract]
    internal class QueryPresenceResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public List<PresenceResult> presence_result { get; set; }

    }

    [ProtoContract]
    internal class RemoveUserRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 5)]
        public EventRequestHeader event_request_header { get; set; }

    }

    [ProtoContract]
    internal class RemoveUserResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 4)]
        public Event created_event { get; set; }

    }

    [ProtoContract]
    internal class RenameConversationRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 3)]
        public string new_name { get; set; }

        [ProtoMember(Position = 5)]
        public EventRequestHeader event_request_header { get; set; }


        // TODO
    }

    [ProtoContract]
    internal class RenameConversationResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 4)]
        public Event created_event { get; set; }


        // TODO: use json to check field names?
    }

    [ProtoContract]
    internal class SearchEntitiesRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 3)]
        public string query { get; set; }

        [ProtoMember(Position = 4)]
        public long max_count { get; set; }

    }

    [ProtoContract]
    internal class SearchEntitiesResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public List<Entity> entity { get; set; }

    }

    [ProtoContract]
    internal class SendChatMessageRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public object unknow2 { get; set; }

        [ProtoMember(Position = 3)]
        public object unknow3 { get; set; }

        [ProtoMember(Position = 4)]
        public object unknow4 { get; set; }

        [ProtoMember(Position = 5)]
        public List<EventAnnotation> annotation { get; set; }

        [ProtoMember(Position = 6)]
        public MessageContent message_content { get; set; }

        [ProtoMember(Position = 7)]
        public ExistingMedia existing_media { get; set; }

        [ProtoMember(Position = 8)]
        public EventRequestHeader event_request_header { get; set; }


        // TODO: incomplete
    }

    [ProtoContract]
    internal class SendChatMessageResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 6)]
        public Event created_event { get; set; }


        // TODO:
        // unknown [] = 4;
    }

    [ProtoContract]
    internal class SetActiveClientRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        // Whether to set the client as active or inactive.
        [ProtoMember(Position = 2)]
        public bool is_active { get; set; }

        // 'email/resource'.
        [ProtoMember(Position = 3)]
        public string full_jid { get; set; }

        // Timeout in seconds for client to remain active.
        [ProtoMember(Position = 4)]
        public long timeout_secs { get; set; }

        [ProtoMember(Position = 5, Optional = true)]
        public bool unknown { get; set; }
        // TODO:
        // unknown (true) = 5;
    }

    [ProtoContract]
    internal class SetActiveClientResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

    }

    [ProtoContract]
    internal class SetConversationLevelRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }


        // TODO: implement me
    }

    [ProtoContract]
    internal class SetConversationLevelResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }


        // TODO: implement me
    }

    [ProtoContract]
    internal class SetConversationNotificationLevelRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public NotificationLevel level { get; set; }

    }

    [ProtoContract]
    internal class SetConversationNotificationLevelResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public long timestamp { get; set; }

    }

    [ProtoContract]
    internal class SetFocusRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public FocusType type { get; set; }

        [ProtoMember(Position = 4)]
        public uint timeout_secs { get; set; }

    }

    [ProtoContract]
    internal class SetFocusResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public long timestamp { get; set; }

    }

    // Allows setting one or more of the included presence-related settings.
    [ProtoContract]
    internal class SetPresenceRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public PresenceStateSetting presence_state_setting { get; set; }

        [ProtoMember(Position = 3, Optional = true)]
        public DndSetting dnd_setting { get; set; }

        [ProtoMember(Position = 5, Optional = true)]
        public DesktopOffSetting desktop_off_setting { get; set; }

        [ProtoMember(Position = 8, Optional = true)]
        public MoodSetting mood_setting { get; set; }

    }

    [ProtoContract]
    internal class SetPresenceResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

    }

    [ProtoContract]
    internal class SetTypingRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public TypingType type { get; set; }

    }

    [ProtoContract]
    internal class SetTypingResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public long timestamp { get; set; }

    }

    [ProtoContract]
    internal class SyncAllNewEventsRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        // Timestamp after which to return all new events.
        [ProtoMember(Position = 2)]
        public long last_sync_timestamp { get; set; }

        [ProtoMember(Position = 8)]
        public long max_response_size_bytes { get; set; }


        // TODO
    }

    [ProtoContract]
    internal class SyncAllNewEventsResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public long sync_timestamp { get; set; }

        [ProtoMember(Position = 3)]
        public List<ConversationState> conversation_state { get; set; }


        // TODO
    }

    [ProtoContract]
    internal class SyncRecentConversationsRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 3)]
        public long max_conversations { get; set; }

        [ProtoMember(Position = 4)]
        public long max_events_per_conversation { get; set; }

        [ProtoMember(Position = 5)]
        //        public List<SyncFilter> sync_filter { get; set; }
        public SyncFilter sync_filter { get; set; }

    }

    [ProtoContract]
    internal class SyncRecentConversationsResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

        [ProtoMember(Position = 2)]
        public long sync_timestamp { get; set; }

        [ProtoMember(Position = 3)]
        public List<ConversationState> conversation_state { get; set; }

    }

    [ProtoContract]
    internal class UpdateWatermarkRequest
    {
        [ProtoMember(Position = 1)]
        public RequestHeader request_header { get; set; }

        [ProtoMember(Position = 2)]
        public ConversationId conversation_id { get; set; }

        [ProtoMember(Position = 3)]
        public long last_read_timestamp { get; set; }

    }

    [ProtoContract]
    internal class UpdateWatermarkResponse
    {
        [ProtoMember(Position = 1)]
        public ResponseHeader response_header { get; set; }

    }

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
