using System;


namespace Messenger.Core.Models
{
    public class Notification<T>  where T: NotificationMessageBase
    {
        public uint Id { get; set; }
        public string RecipientId { get; set; }
        public DateTime CreationTime { get; set; }
        public T Message { get; set; }

        public Notification()
        {
            RecipientId = "";
        }

        public override string ToString()
        {
            return $"Notification: RecipientId={RecipientId}, CreationTime={CreationTime}, Message={Message}";
        }
    }
}
