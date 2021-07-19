namespace Messenger.Core.Models
{
    public class NotificationMute
    {
        public uint               Id                      { get; set; }
        public NotificationType   NotificationType        { get; set; }
        public NotificationSource NotificationSourceType  { get; set; }
        public string             NotificationSourceValue { get; set; }
        public string             SenderId                { get; set; }
        public string             UserId                  { get; set; }

        public NotificationMute()
        {
            NotificationSourceValue = "";
            SenderId = "";
            UserId = "";
        }

        public override string ToString()
        {
            return $"NotificationMute: Id={Id} NotificationType={NotificationType.ToString()} NotificationSourceType={NotificationSourceType.ToString()} NotificationSourceValue={NotificationSourceValue}, SenderId={SenderId}, UserId={UserId}";
        }
    }
}
