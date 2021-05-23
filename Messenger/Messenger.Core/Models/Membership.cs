namespace Messenger.Core.Models
{
    public class Membership
    {
        public int MembershipId { get; set; }
        
        public string UserId { get; set; }
        
        public int TeamId { get; set; }

        public string UserRole { get; set; }
    }
}
