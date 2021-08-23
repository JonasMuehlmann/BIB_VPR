namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a NotificationMute used to mute notifications
    /// </summary>
    public class NotificationMute
    {
        /// <summary>
        /// The unique id of the NotificationMute
        /// </summary>
        public uint                Id                      { get; set; }

        /// <summary>
        /// The type of notification to mute
        /// </summary>
        public NotificationType?   NotificationType        { get; set; }

        /// <summary>
        /// The type of notification source to mute
        /// </summary>
        public NotificationSource? NotificationSourceType  { get; set; }

        /// <summary>
        /// The id of the concrete notification source to mute
        /// </summary>
        public string              NotificationSourceValue { get; set; }

        /// <summary>
        /// The id of the user who triggered the sending through his action to mute
        /// </summary>
        public string              SenderId                { get; set; }

        /// <summary>
        /// The id of the user who mutes the notification
        /// </summary>
        public string              UserId                  { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
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
