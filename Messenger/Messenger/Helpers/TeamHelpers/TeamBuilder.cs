using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers.TeamHelpers
{
    public static class TeamBuilder
    {
        public static async Task<IEnumerable<Team>> GetTeamsFromDatabase(UserViewModel currentUser)
        {
            return await MessengerService.GetTeams(currentUser.Id);
        }

        public static async Task<TeamViewModel> Build(this Team team, string userId)
        {
            TeamViewModel withMembers = await Map(team).WithMembers(userId);
            TeamViewModel withChannels = await withMembers.WithChannels();
            TeamViewModel withTeamRoles = await withChannels.WithTeamRoles();

            return DetermineTeamType(withTeamRoles);
        }

        public static async Task<IEnumerable<TeamViewModel>> Build(this IEnumerable<Team> teams, string userId)
        {
            List<TeamViewModel> result = new List<TeamViewModel>();

            foreach (Team team in teams)
            {
                var viewModel = await Build(team, userId);
                result.Add(viewModel);
            }

            return result;
        }

        #region Team Extension

        /// <summary>
        /// Determines the type of team and sets the corresponding member models
        /// </summary>
        /// <param name="team">Team object to reference to</param>
        /// <param name="members">List of members to set</param>
        public static async Task<TeamViewModel> WithMembers(this TeamViewModel viewModel, string currentUserId)
        {
            var members = await MessengerService.GetTeamMembers(viewModel.Id);

            if (string.IsNullOrEmpty(viewModel.TeamName))
            {
                /* PRIVATE CHAT HAS ONLY PARTNER AS MEMBER */
                IEnumerable<MemberViewModel> chatPartner = Map(members.Where(m => m.Id != currentUserId));

                viewModel.Members = new ObservableCollection<MemberViewModel>(chatPartner);
            }
            else
            {
                List<MemberViewModel> mapped = Map(members).ToList();

                /* LOAD MEMBER ROLES */
                foreach (MemberViewModel member in mapped)
                {
                    await member.WithMemberRoles(viewModel.Id);
                }

                viewModel.Members = new ObservableCollection<MemberViewModel>(mapped);
            }

            return viewModel;
        }

        /// <summary>
        /// Loads the children channels of the team
        /// </summary>
        /// <param name="viewModel">ViewModel of the team</param>
        /// <returns>TeamViewModel with updated channels</returns>
        public static async Task<TeamViewModel> WithChannels(this TeamViewModel viewModel)
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

        public static async Task<TeamViewModel> WithTeamRoles(this TeamViewModel viewModel)
        {
            IList<TeamRole> teamRoles = await TeamService.ListRoles(viewModel.Id);

            if (teamRoles == null || teamRoles.Count() <= 0)
            {
                return viewModel;
            }

            foreach (TeamRole teamRole in teamRoles)
            {
                TeamRoleViewModel teamRoleViewModel = await Map(teamRole).WithPermissions();

                viewModel.TeamRoles.Add(teamRoleViewModel);
            }

            return viewModel;
        }

        #endregion

        #region Member Extension

        public static async Task<MemberViewModel> WithMemberRoles(this MemberViewModel viewModel, uint teamId)
        {
            /** LOAD CUSTOM ROLES FOR THE TEAM **/
            IEnumerable<string> roles = await TeamService.GetUsersRoles(teamId, viewModel.Id);

            if (roles == null || roles.Count() <= 0)
            {
                return viewModel;
            }

            /** LOAD PERMISSIONS FOR EACH ROLE **/
            foreach (string role in roles)
            {
                TeamRoleViewModel memberRole = await Map(teamId, role).WithPermissions();

                viewModel.MemberRoles.Add(memberRole);
            }

            viewModel.TeamId = teamId;

            return viewModel;
        }

        #endregion

        #region TeamRole Extension

        public static async Task<TeamRoleViewModel> WithPermissions(this TeamRoleViewModel viewModel)
        {
            IList<Permissions> permissions = await TeamService.GetPermissionsOfRole(viewModel.TeamId, viewModel.Title);

            if (permissions != null && permissions.Count() > 0)
            {
                viewModel.Permissions = permissions.ToList();
            }

            return viewModel;
        }

        #endregion

        #region Mappers

        public static TeamViewModel Map(Team team)
        {
            return new TeamViewModel()
            {
                Id = team.Id,
                TeamName = team.Name,
                Description = team.Description,
                CreationDate = team.CreationDate,
                Members = new ObservableCollection<MemberViewModel>(),
                Channels = new ObservableCollection<ChannelViewModel>(),
                TeamRoles = new ObservableCollection<TeamRoleViewModel>()
            };
        }

        public static TeamRoleViewModel Map(TeamRole teamRole)
        {
            return new TeamRoleViewModel()
            {
                Id = teamRole.Id,
                Title = teamRole.Role,
                TeamId = teamRole.TeamId,
                Permissions = new List<Permissions>()
            };
        }

        public static TeamRoleViewModel Map(uint teamId, string roleTitle)
        {
            return new TeamRoleViewModel()
            {
                Title = roleTitle,
                TeamId = teamId,
                Permissions = new List<Permissions>()
            };
        }

        public static ChannelViewModel Map(Channel channel)
        {
            return new ChannelViewModel()
            {
                ChannelId = channel.ChannelId,
                TeamId = channel.TeamId,
                ChannelName = channel.ChannelName
            };
        }

        public static MemberViewModel Map(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new MemberViewModel()
            {
                Id = user.Id,
                Name = user.DisplayName,
                NameId = user.NameId,
                Bio = user.Bio,
                Mail = user.Mail,
                MemberRoles = new List<TeamRoleViewModel>()
            };
        }

        public static IEnumerable<MemberViewModel> Map(IEnumerable<User> users)
        {
            return users.Select(Map);
        }

        #endregion

        #region Helpers

        public static TeamViewModel DetermineTeamType(TeamViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.TeamName))
            {
                return new PrivateChatViewModel(viewModel);
            }
            else
            {
                return viewModel;
            }
        }

        #endregion
    }
}
