using Messenger.Core.Models;
using System.Linq;

namespace Messenger.Models
{
    public class PrivateChat : Team
    {
        public User Partner { get; set; }

        public static PrivateChat CreatePrivateChatFromTeamData(Team teamData)
        {
            bool isMemberDataValid = teamData.Members != null
                && teamData.Members.Count > 0;

            User partner = isMemberDataValid ? teamData.Members.FirstOrDefault() : null;

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
