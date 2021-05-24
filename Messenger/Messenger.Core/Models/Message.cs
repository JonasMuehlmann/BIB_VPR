using System;


namespace Messenger.Core.Models
{
    /// <summary>
    /// An abstract class of a message used asa base for Privatemessage and Teammessage
    /// </summary>
    public class Message
    {
        public uint Id { get; set; }

        public string SenderId { get; set; }

        public string Content { get; set; }

        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Team where the message was posted
        /// </summary>
        public uint RecipientId { get; set; }

        /// <summary>
        /// Null in case of a Top-level message
        /// </summary>
        public uint? ParentMessageId { get; set; }
}
}
