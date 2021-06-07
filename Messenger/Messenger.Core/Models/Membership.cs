namespace Messenger.Core.Models
{
    public class Membership
    {
        public uint MembershipId { get; set; }

        public string UserId { get; set; }

        public uint TeamId { get; set; }

        public string UserRole { get; set; }

        public override string ToString()
        {
            return $"MembershipId={MembershipId}, UserId={UserId}, TeamId={TeamId}, UserRole={UserRole}";
        }
    }
}
