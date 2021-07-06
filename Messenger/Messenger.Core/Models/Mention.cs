namespace Messenger.Core.Models
{
    public class Mention
    {
        public uint Id { get; set; }

        public string TargetType { get; set; }

        public string TargetId { get; set; }

        public Mention()
        {
            TargetType = "";
            TargetId   = "";
        }

        public override string ToString()
        {
            return $"Mention: Id={Id}, TargetType={TargetType}, TargetId={TargetId}";
        }
    }
}
