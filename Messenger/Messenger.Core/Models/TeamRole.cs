namespace Messenger.Core.Models
{
    /// <summary>
    /// The source code side representation of a Role defined in a Team
    /// </summary>
    public class TeamRole
    {
        /// <summary>
        /// The unique id of the role
        /// </summary>
        public uint Id {get; set;}

        /// <summary>
        /// The actual name of the rome
        /// </summary>
        public string Role {get; set;}

        /// <summary>
        /// The id of the team the role is defined in
        /// </summary>
        public uint TeamId {get; set;}

        /// <summary>
        /// The hex color code used when displaying the role
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Default initialize all members(string values get initialized to "" instead of null)
        /// </summary>
        public TeamRole()
        {
            Role = "";
        }

        public override string ToString()
        {
            return $"TeamRole: {Id}, Role={Role}, TeamId={TeamId}, Color={Color}";
        }
    }
}
