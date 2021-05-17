namespace Messenger.Core.Models
{
    /// <summary>
    /// A subclass of Message representing a message written in a Team
    /// </summary>
    public class TeamMessage : Message
    {
        /// <summary>
        /// Team where the message was posted
        /// </summary>
        public int TeamId { get; set; }

        /// <summary>
        /// Null in case of a Top-level message
        /// </summary>
        public int? ParentMessageId { get; set; }
    }
}
