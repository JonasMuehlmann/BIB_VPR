namespace Messenger.Core.Models
{
    /// <summary>
    /// Holds information about a mentionable entity
    /// </summary>
    public class Mentionable
    {
        public MentionTarget TargetType { get; set; }

        public string TargetId { get; set; }

        public string TargetName { get; set; }

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
