namespace Messenger.Core.Models
{
    public class Reaction
    {
        public uint Id { get; set; }

        public string Symbol { get; set; }

        public string UserId{ get; set; }

        public uint MessageId { get; set; }

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
