using System;
using System.Collections.Generic;

namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a Message
    /// </summary>
    public class Message
    {
        /// <summary>
        ///	The unique id of the message
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        ///	The id of the messages's sender
        /// </summary>
        public string SenderId { get; set; }

        /// <summary>
        /// The actual content of the message
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// The time at which the message was created
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// The id of the channel the message was sent to
        /// </summary>
        public uint RecipientId { get; set; }

        /// <summary>
        /// The optional parent of the message (which this one is a reply to)
        /// </summary>
        public uint? ParentMessageId { get; set; }

        /// <summary>
        /// The id of the user who sent the message
        /// </summary>
        public User Sender { get; set; }

        /// <summary>
        /// The list of blob names representing the message attachments
        /// </summary>
        public List<string> AttachmentsBlobName { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null
        /// and Lists get initialized to an empty one instead of null)
        /// </summary>
        public Message()
        {
            SenderId = "";
            Content = "";
            AttachmentsBlobName = new List<string>();
        }

        public override string ToString()
        {
            return $"Message: Id={Id}, SenderId={SenderId}, Content={Content}, CreationTime={CreationTime.ToString()}, RecipientId={RecipientId}, ParentMessageId={ParentMessageId}, AttachmentBlobNames=[{string.Join(", ", AttachmentsBlobName)}]";
        }
    }
}
