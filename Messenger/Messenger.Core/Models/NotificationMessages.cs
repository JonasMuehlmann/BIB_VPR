using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Messenger.Core.Models
{
    public class NotificationMessageBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationSource NotificationSource { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public NotificationType NotificationType { get; set; }

        public NotificationMessageBase(){}

        [JsonConstructor]
        public NotificationMessageBase(NotificationSource notificationSource, NotificationType notificationType)
        {
            NotificationSource = notificationSource;
            NotificationType = notificationType;
        }

            }

    public class UserMentionedNotificationMessage : NotificationMessageBase
    {
        [JsonConverter(typeof(StringEnumConverter))]
        public MentionTarget MentionTarget { get; set; }
        public string MentionTargetName { get; set; }

        public UserMentionedNotificationMessage(){}

        [JsonConstructor]
        public UserMentionedNotificationMessage(NotificationSource notificationSource,
                                                NotificationType notificationType,
                                                MentionTarget mentionTarget,
                                                string mentionTargetName) : base(notificationSource, notificationType)
        {
            MentionTarget = mentionTarget;
            MentionTargetName = mentionTargetName;
        }
    }

    public abstract class  TeamActionNotificationMessage : NotificationMessageBase
    {
        public uint TeamId { get; set; }

        public TeamActionNotificationMessage(){}

        [JsonConstructor]
        public TeamActionNotificationMessage(NotificationSource notificationSource,
                                             NotificationType notificationType,
                                             uint teamId) : base(notificationSource, notificationType)
        {
            TeamId = teamId;
        }
    }

    public abstract class MessageReceivedNotificationMessage : TeamActionNotificationMessage
    {
        public string SenderId { get; set; }

        public MessageReceivedNotificationMessage(){}

        [JsonConstructor]
        public MessageReceivedNotificationMessage(NotificationSource notificationSource,
                                                  NotificationType notificationType,
                                                  uint teamId,
                                                  string senderId) : base(notificationSource, notificationType, teamId)
        {
            SenderId = senderId;
        }

    }

    public class MessageInSubscribedTeamNotificationMessage : MessageReceivedNotificationMessage
    {
        public MessageInSubscribedTeamNotificationMessage(){}

        [JsonConstructor]
        public MessageInSubscribedTeamNotificationMessage(NotificationSource notificationSource,
                                                          NotificationType notificationType,
                                                          uint teamId,
                                                          string senderId) : base(notificationSource, notificationType, teamId, senderId) {}
    }

    public class MessageInSubscribedChannelNotificationMessage : MessageReceivedNotificationMessage
    {
        public uint ChannelId { get; set; }

        public MessageInSubscribedChannelNotificationMessage(){}

        [JsonConstructor]
        public MessageInSubscribedChannelNotificationMessage(NotificationSource notificationSource,
                                                             NotificationType notificationType,
                                                             uint teamId,
                                                             string senderId,
                                                             uint channelId) : base(notificationSource, notificationType, teamId, senderId)
        {
            ChannelId = channelId;
        }
    }



    public class MessageInPrivateChatNotificationMessage : MessageReceivedNotificationMessage
    {
        public string PartnerId { get; set; }

        public MessageInPrivateChatNotificationMessage(){}

        [JsonConstructor]
        public MessageInPrivateChatNotificationMessage(NotificationSource notificationSource,
                                                       NotificationType notificationType,
                                                       uint teamId,
                                                       string senderId,
                                                       string partnerId) : base(notificationSource, notificationType, teamId, senderId)
        {
            PartnerId = partnerId;
        }

    }

    public class InvitedToTeamNotificationMessage : TeamActionNotificationMessage
    {
        public InvitedToTeamNotificationMessage(){}

        [JsonConstructor]
        public InvitedToTeamNotificationMessage(NotificationSource notificationSource,
                                                NotificationType notificationType,
                                                uint teamId) : base(notificationSource, notificationType, teamId) {}
    }

    public class RemovedFromTeamNotificationMessage : TeamActionNotificationMessage {

        public  RemovedFromTeamNotificationMessage(){}

        [JsonConstructor]
        public RemovedFromTeamNotificationMessage(NotificationSource notificationSource,
                                                  NotificationType notificationType,
                                                  uint teamId) : base(notificationSource, notificationType, teamId) {}

    }

    public class ReactionToMessageNotificationMessage : NotificationMessageBase
    {
        public string Reaction { get; set; }

        public ReactionToMessageNotificationMessage(){}

        [JsonConstructor]
        public ReactionToMessageNotificationMessage(NotificationSource notificationSource,
                                                    NotificationType notificationType,
                                                    string reaction) : base(notificationSource, notificationType)
        {
            Reaction = reaction;
        }
    }
}
