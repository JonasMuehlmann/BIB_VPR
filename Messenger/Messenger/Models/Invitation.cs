namespace Messenger.Models
{
    public class Invitation
    {
        public string UserId { get; set; }

        public uint TeamId { get; set; }

        public Invitation()
        {

        }

        public override string ToString()
        {
            return $"Invitation: UserId={UserId} TeamId={TeamId}";
        }
    }
}
