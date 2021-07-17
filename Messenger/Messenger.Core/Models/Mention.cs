namespace Messenger.Core.Models
{
    public class Mention
    {
        public uint Id { get; set; }

        public MentionTarget TargetType { get; set; }

        public string TargetId { get; set; }

        public Mention()
        {
            TargetId   = "";
        }

        public override string ToString()
        {
            return $"Mention: Id={Id}, TargetType={TargetType}, TargetId={TargetId}";
        }
    }
}
