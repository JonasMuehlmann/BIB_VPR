using System;
using Newtonsoft.Json.Linq;


namespace Messenger.Core.Models
{
    public class Notification
    {
        public uint Id { get; set; }
        public string RecipientId { get; set; }
        public DateTime CreationTime { get; set; }
        public JObject Message { get; set; }

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
