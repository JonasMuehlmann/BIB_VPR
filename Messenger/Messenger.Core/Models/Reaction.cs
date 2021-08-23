namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a reaction to a message
    /// </summary>
    public class Reaction
    {
        /// <summary>
        /// The unique id of the reaction made to a message
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The symbol someone reacted to a message with
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The id of the user who reacted to a message
        /// </summary>
        public string UserId{ get; set; }

        /// <summary>
        /// The id of the message a user reacted to
        /// </summary>
        public uint MessageId { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
        public Reaction()
        {
            Symbol = "";
            UserId = "";
        }

        public override string ToString()
        {
            return $"Reaction: Id={Id}, Symbol={Symbol}, UserId={UserId}, MessageId={MessageId}";
        }
    }
}
