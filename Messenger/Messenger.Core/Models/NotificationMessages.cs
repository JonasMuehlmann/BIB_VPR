using System;

namespace Messenger.Core.Models
{
    public abstract class NotificationMessageBase : IConvertible
    {
        NotificationSource NotificationSource { get; set; }
        NotificationType NotificationType { get; set; }

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

        bool IConvertible.ToBoolean(IFormatProvider provider) => ThrowNotSupported<bool>();
        char IConvertible.ToChar(IFormatProvider provider) => ThrowNotSupported<char>();
        sbyte IConvertible.ToSByte(IFormatProvider provider) => ThrowNotSupported<sbyte>();
        byte IConvertible.ToByte(IFormatProvider provider) => ThrowNotSupported<byte>();
        short IConvertible.ToInt16(IFormatProvider provider) => ThrowNotSupported<short>();
        ushort IConvertible.ToUInt16(IFormatProvider provider) => ThrowNotSupported<ushort>();
        int IConvertible.ToInt32(IFormatProvider provider) => ThrowNotSupported<int>();
        uint IConvertible.ToUInt32(IFormatProvider provider) => ThrowNotSupported<uint>();
        long IConvertible.ToInt64(IFormatProvider provider) => ThrowNotSupported<long>();
        ulong IConvertible.ToUInt64(IFormatProvider provider) => ThrowNotSupported<ulong>();
        float IConvertible.ToSingle(IFormatProvider provider) => ThrowNotSupported<float>();
        double IConvertible.ToDouble(IFormatProvider provider) => ThrowNotSupported<double>();
        decimal IConvertible.ToDecimal(IFormatProvider provider) => ThrowNotSupported<decimal>();
        DateTime IConvertible.ToDateTime(IFormatProvider provider) => ThrowNotSupported<DateTime>();
        string IConvertible.ToString(IFormatProvider provider) => ThrowNotSupported<string>();

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
        public MentionTarget MentionTarget {get; set;}
        public string MentionTargetName { get; set; }

    }

    public abstract class  TeamActionNotificationMessage : NotificationMessageBase
    {
        public uint TeamId {get; set; }
    }

    public abstract class MessageReceivedNotificationMessage : TeamActionNotificationMessage
    {
        public string SenderId {get; set; }
    }

    public class MessageInSubscribedTeamNotificationMessage : MessageReceivedNotificationMessage {}

    public class MessageInSubscribedChannelNotificationMessage : MessageReceivedNotificationMessage
    {
        public uint ChannelId {get; set; }
    }



    public class MessageInPrivateChatNotificationMessage : MessageReceivedNotificationMessage
    {
        public string PartnerId {get; set; }
    }

    public class InvitedToTeamNotificationMessage : TeamActionNotificationMessage {}

    public class RemovedFromTeamNotificationMessage : TeamActionNotificationMessage {}

    public class ReactionToMessageNotificationMessage : NotificationMessageBase
    {
        public string Reaction { get; set; }
    }
}
