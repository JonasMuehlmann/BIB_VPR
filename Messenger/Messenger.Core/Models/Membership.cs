namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a User's Membership in a team
    /// </summary>
    public class Membership
    {
        /// <summary>
        /// The unique id of the Membership
        /// </summary>
        public uint MembershipId { get; set; }

        /// <summary>
        /// The id of the user the membership belongs to
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// The id of the team the user is a member of
        /// </summary>
        public uint TeamId { get; set; }

        /// <summary>
        /// The roles the user has in the team
        /// </summary>
        public string UserRole { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
        public Membership()
        {
            UserId = "";
            UserRole = "";
        }
        public override string ToString()
        {
            return $"MembershipId={MembershipId}, UserId={UserId}, TeamId={TeamId}, UserRole={UserRole}";
        }
    }
}
