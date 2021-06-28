using System;


namespace Messenger.Core.Models
{
    public class Notification
    {
        public int Id { get; set; }
        public string RecipientId { get; set; }
        public DateTime CreationTime { get; set; }
        public string Message { get; set; }

        public Notification()
        {
            RecipientId = "";
            Message = "";
        }

        public override string ToString()
        {
            return $"Notification: RecipientId={RecipientId}, CreationTime={CreationTime}, Message={Message}";
        }
    }
}
