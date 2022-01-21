using System;

namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a Mention
    /// </summary>
    public class Mention
    {
        /// <summary>
        /// The unique id of the mention
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// The type of entity to mention
        /// </summary>
        public MentionTarget TargetType { get; set; }

        /// <summary>
        /// The id of the entity to mention
        /// </summary>
        public string TargetId { get; set; }

        /// <summary>
        /// The id of the user mention an entity
        /// </summary>
        public string MentionerId { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
        public Mention()
        {
            TargetId   = "";
            MentionerId = "";
        }

        public override string ToString()
        {
            return $"Mention: Id={Id}, TargetType={TargetType}, TargetId={TargetId}, MentionerId={MentionerId}";
        }
    }
};
