namespace Messenger.Core.Models
{
    public class Membership
    {
        public uint MembershipId { get; set; }
        
        public string UserId { get; set; }
        
        public uint TeamId { get; set; }

        public string UserRole { get; set; }
    }
}
