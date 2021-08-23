namespace Messenger.Core.Models
{
    /// <summary>
    /// Holds information about a mentionable entity and is intended to
    /// be used in dialogs to insert a mention in a message
    /// </summary>
    public class Mentionable
    {
        /// <summary>
        /// The type of entity to mention
        /// </summary>
        public MentionTarget TargetType { get; set; }

        /// <summary>
        /// The id of the concrete entity to mention
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// The name of the concrete entity to mention
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
        public Mentionable()
        {
            TargetId   = "";
            TargetName   = "";
        }

        public override string ToString()
        {
            return $"Mentionble: TargetType={TargetType}, TargetId={TargetId}, TargetName={TargetName}";
        }
    }
}
