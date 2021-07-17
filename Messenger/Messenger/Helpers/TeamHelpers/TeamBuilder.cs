using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Helpers.TeamHelpers
{
    public class TeamBuilder
    {
        private readonly TeamFactory _factory;

        public TeamBuilder(TeamFactory factory)
        {
            _factory = factory;
        }

        public async Task<TeamViewModel> Build(Team team, string userId)
        {
            TeamViewModel viewModel = _factory.CreateBaseViewModel(team);

            var withMembers = await LoadMembers(viewModel, userId);

            var withChannels = await LoadChannels(withMembers);

            return _factory.GetViewModel(withChannels);
        }

        public async Task<IEnumerable<TeamViewModel>> Build(IEnumerable<Team> teams, string userId)
        {
            List<TeamViewModel> result = new List<TeamViewModel>();

            foreach (Team team in teams)
            {
                var viewModel = await Build(team, userId);
                result.Add(viewModel);
            }

            return result;
        }

        /// <summary>
        /// Determines the type of team and sets the corresponding member models
        /// </summary>
        /// <param name="team">Team object to reference to</param>
        /// <param name="members">List of members to set</param>
        private async Task<TeamViewModel> LoadMembers(TeamViewModel viewModel, string currentUserId)
        {
            var members = await MessengerService.LoadTeamMembers((uint)viewModel.Id);

            if (string.IsNullOrEmpty(viewModel.TeamName)) // Is private chat
            {
                var chatPartner = Map(members.Where(m => m.Id != currentUserId));

                viewModel.Members = new ObservableCollection<Member>(chatPartner);
            }
            else
            {
                List<Member> mapped = Map(members).ToList();

                foreach (Member member in mapped)
                {
                    IList<MemberRole> roles = await GetMemberRoles((uint)viewModel.Id, member);

                    if (roles != null)
                    {
                        member.MemberRoles = roles.ToList();
                    }
                }

                viewModel.Members = new ObservableCollection<Member>(mapped);
            }

            return viewModel;
        }

        public async Task<IList<MemberRole>> GetMemberRoles(uint teamId, Member member)
        {
            List<MemberRole> memberRoles = new List<MemberRole>();

            var roles = await MessengerService.GetRolesList(teamId, member.Id);

            if (roles == null || roles.Count() <= 0)
            {
                return null;
            }

            foreach (string role in roles)
            {
                IList<Permissions> permissions = await TeamService.GetPermissionsOfRole(teamId, role);

                memberRoles.Add(
                    new MemberRole()
                    {
                        Title = role.ToUpper(),
                        TeamId = teamId,
                        Permissions = permissions.ToList()
                    });
            }

            return memberRoles;
        }

        /// <summary>
        /// Loads the children channels of the team
        /// </summary>
        /// <param name="viewModel">ViewModel of the team</param>
        /// <returns>TeamViewModel with updated channels</returns>
        public async Task<TeamViewModel> LoadChannels(TeamViewModel viewModel)
        {
            var channels = await MessengerService.GetChannelsForTeam((uint)viewModel.Id);

            if (channels.Count() > 0)
            {
                var channelViewModels = channels.Select(Map);

                viewModel.Channels.Clear();
                foreach (ChannelViewModel channelViewModel in channelViewModels)
                {
                    viewModel.Channels.Add(channelViewModel);
                }
            }

            return viewModel;
        }

        public async Task<TeamViewModel> LoadChannels(uint teamId, string userId)
        {
            Team team = await MessengerService.GetTeam(teamId);
            TeamViewModel viewModel = await Build(team, userId);

            return await LoadChannels(viewModel);
        }

        public ChannelViewModel Map(Channel channel)
        {
            return new ChannelViewModel()
            {
                ChannelId = channel.ChannelId,
                TeamId = channel.TeamId,
                ChannelName = channel.ChannelName
            };
        }

        public Member Map(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new Member()
            {
                Id = user.Id,
                Name = user.DisplayName,
                NameId = user.NameId,
                Bio = user.Bio,
                Mail = user.Mail,
                MemberRoles = new List<MemberRole>()
            };
        }

        private IEnumerable<Member> Map(IEnumerable<User> users)
        {
            return users.Select(Map);
        }
    }
}
