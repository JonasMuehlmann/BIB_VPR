using System;
using Newtonsoft.Json.Linq;


namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a Notification
    /// </summary>
    public class Notification
    {
        /// <summary>
        /// The unique id of the notification
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The id of the user who receives the notification
        /// </summary>
        public string RecipientId { get; set; }

        /// <summary>
        /// The time at which the notification was created
        /// </summary>
        public DateTime CreationTime { get; set; }


        /// <summary>
        /// A JSON representation of the actual notification content
        /// </summary>
        public JObject Message { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
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
