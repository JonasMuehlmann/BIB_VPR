using System;
using System.Text.Json.Serialization;

namespace Messenger.Core.Models
{
    public class NotificationMessageBase : IConvertible
    {
        public NotificationSource NotificationSource { get; set; }
        public NotificationType NotificationType { get; set; }

        public NotificationMessageBase(){}

        [JsonConstructor]
        public NotificationMessageBase(NotificationSource notificationSource, NotificationType notificationType)
        {
            NotificationSource = notificationSource;
            NotificationType = notificationType;
        }

        #region IConvertible Implementation

        static T ThrowNotSupported<T>()
        {
            var ex = ThrowNotSupported(typeof(T));
            return (T)ex;
        }

        static object ThrowNotSupported(Type type)
        {
            throw new InvalidCastException($"Converting type \"{typeof(NotificationMessageBase)}\" to type \"{type}\" is not supported.");
        }

        TypeCode IConvertible.GetTypeCode()
        {
            return TypeCode.Object;
        }

        bool     IConvertible.ToBoolean(IFormatProvider provider) => ThrowNotSupported<bool>();
        char     IConvertible.ToChar(IFormatProvider provider) => ThrowNotSupported<char>();
        sbyte    IConvertible.ToSByte(IFormatProvider provider) => ThrowNotSupported<sbyte>();
        byte     IConvertible.ToByte(IFormatProvider provider) => ThrowNotSupported<byte>();
        short    IConvertible.ToInt16(IFormatProvider provider) => ThrowNotSupported<short>();
        ushort   IConvertible.ToUInt16(IFormatProvider provider) => ThrowNotSupported<ushort>();
        int      IConvertible.ToInt32(IFormatProvider provider) => ThrowNotSupported<int>();
        uint     IConvertible.ToUInt32(IFormatProvider provider) => ThrowNotSupported<uint>();
        long     IConvertible.ToInt64(IFormatProvider provider) => ThrowNotSupported<long>();
        ulong    IConvertible.ToUInt64(IFormatProvider provider) => ThrowNotSupported<ulong>();
        float    IConvertible.ToSingle(IFormatProvider provider) => ThrowNotSupported<float>();
        double   IConvertible.ToDouble(IFormatProvider provider) => ThrowNotSupported<double>();
        decimal  IConvertible.ToDecimal(IFormatProvider provider) => ThrowNotSupported<decimal>();
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ThrowNotSupported<DateTime>();
        string   IConvertible.ToString(IFormatProvider provider) => ThrowNotSupported<string>();

        object IConvertible.ToType(Type conversionType, IFormatProvider provider)
        {
            if (conversionType == typeof(NotificationMessageBase))
            {
                return this;
            }

            // Other implementations here

            return ThrowNotSupported(conversionType);
        }

        #endregion
    }

    public class UserMentionedNotificationMessage : NotificationMessageBase
    {
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
