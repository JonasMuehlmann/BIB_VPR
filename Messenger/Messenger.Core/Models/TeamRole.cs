namespace Messenger.Core.Models
{
    public class TeamRole
    {
        public uint Id {get; set;}
        public string Role {get; set;}
        public uint TeamId {get; set;}

        /// <summary>
        /// Hex
        /// </summary>
        public string Color { get; set; }

        public TeamRole()
        {
            Role = "";
        }

        public override string ToString()
        {
            return $"TeamRole: {Id}, Role={Role}, TeamId={TeamId}";
        }
    }
}
