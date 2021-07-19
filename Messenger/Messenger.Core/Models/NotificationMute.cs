namespace Messenger.Core.Models
{
    public class NotificationMute
    {
        public uint               Id       { get; set; }
        public NotificationType   Type     { get; set; }
        public NotificationSource Source   { get; set; }
        public string             SenderId { get; set; }

        public NotificationMute()
        {
            SenderId = "";
        }

        public override string ToString()
        {
            return $"NotificationMute: Id={Id} Type={Type.ToString()} Source={Source.ToString()}, SenderId={SenderId}";
        }
    }
}
