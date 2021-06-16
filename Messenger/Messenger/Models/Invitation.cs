namespace Messenger.Models
{
    public class Invitation
    {
        public string UserId { get; set; }

        public uint TeamId { get; set; }

        public Invitation(string userId, uint teamId)
        {
            UserId = userId;
            TeamId = teamId;
        }

        public override string ToString()
        {
            return $"Invitation: UserId={UserId} TeamId={TeamId}";
        }
    }
}
