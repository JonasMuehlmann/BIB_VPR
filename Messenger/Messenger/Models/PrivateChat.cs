using Messenger.Core.Models;
using System.Linq;

namespace Messenger.Models
{
    /// <summary>
    /// This extended model of a Team is mainly used to ease bindings in XAML
    /// </summary>
    public class PrivateChat : Team
    {
        public User Partner { get; set; }

        /// <summary>
        /// Creates a PrivateChat object from the given team data
        /// </summary>
        /// <param name="teamData">Team data from the service</param>
        /// <returns>A complete PrivateChat object</returns>
        public static PrivateChat CreatePrivateChatFromTeamData(Team teamData)
        {
            bool isMemberDataValid = teamData.Members != null
                && teamData.Members.Count > 0;

            if (!isMemberDataValid)
            {
                return null;
            }

            User partner = teamData.Members.FirstOrDefault();

            return new PrivateChat()
            {
                Id = teamData.Id,
                Name = string.Empty,
                Description = teamData.Description,
                CreationDate = teamData.CreationDate,
                Partner = partner
            };
        }
    }
}
