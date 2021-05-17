namespace Messenger.Core.Models
{
    /// <summary>
    /// A subclass of Message representing a message sent in a private conversation between two users
    /// </summary>
    public class PrivateMessage : Message
    {
        /// <summary>
        /// The id of the user who has received the message
        /// </summar>
        public string RecipientId { get; set; }
    }
}
