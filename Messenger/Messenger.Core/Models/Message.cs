using System;


namespace Messenger.Core.Models
{
    /// <summary>
    /// An abstract class of a message used asa base for Privatemessage and Teammessage
    /// </summary>
    public abstract class Message
    {
        public int Id { get; set; }

        public string SenderId { get; set; }

        public string Content { get; set; }

        public DateTime CreationTime { get; set; }
    }
}