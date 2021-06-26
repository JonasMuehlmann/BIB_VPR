namespace Messenger.Core.Models
{
    public class Reaction
    {
        public uint Id { get; set; }

        public string Symbol { get; set; }

        public uint UserId{ get; set; }

        public uint MessageId { get; set; }

        public Reaction()
        {
            Symbol = "";
        }

        public override string ToString()
        {
            return $"Reaction: Id={Id}, Symbol={Symbol}, UserId={UserId}, MessageId={MessageId}";
        }
    }
}
