namespace Messenger.Models
{
    public class Invitation
    {
        public string UserId { get; set; }

        public string TeamId { get; set; }

        public Invitation()
        {
            UserId = "";
            TeamId = "";
        }
        public override string ToString()
        {
            return $"Invitation: UserId={UserId} TeamId={TeamId}";
        }
    }
}
