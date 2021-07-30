using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;

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

            if (withChannels is PrivateChatViewModel)
            {
                return withChannels;
            }
            else
            {
                return await withChannels.WithTeamRoles();
            }
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
            IEnumerable<User> members = await MessengerService.GetTeamMembers(viewModel.Id);

            viewModel = DetermineTeamType(viewModel);

            if (viewModel is PrivateChatViewModel)
            {
                PrivateChatViewModel chatViewModel = (PrivateChatViewModel)viewModel;

                /* PRIVATE CHAT HAS ONLY PARTNER AS MEMBER */
                chatViewModel.Partner = Map(members.Where(m => m.Id != currentUserId)).SingleOrDefault();

                return chatViewModel;
            }
            else
            {
                List<MemberViewModel> mapped = Map(members).ToList();

                /* LOAD MEMBER ROLES */
                foreach (MemberViewModel member in mapped)
                {
                    member.TeamId = viewModel.Id;

                    await member.WithMemberRoles();
                }

                viewModel.Members = new ObservableCollection<MemberViewModel>(mapped);

                return viewModel;
            }
        }

        /// <summary>
        /// Loads the children channels of the team
        /// </summary>
        /// <param name="viewModel">ViewModel of the team</param>
        /// <returns>TeamViewModel with updated channels</returns>
        public static async Task<TeamViewModel> WithChannels(this TeamViewModel viewModel)
        {
            IEnumerable<ChannelViewModel> channelViewModels = Map(await MessengerService.GetChannelsForTeam(viewModel.Id));

            if (viewModel is PrivateChatViewModel)
            {
                PrivateChatViewModel chatViewModel = (PrivateChatViewModel)viewModel;

                chatViewModel.MainChannel = channelViewModels.SingleOrDefault();

                return chatViewModel;
            }
            else
            {
                viewModel.Channels.Clear();

                foreach (ChannelViewModel channelViewModel in channelViewModels)
                {
                    viewModel.Channels.Add(channelViewModel);
                }

                return viewModel;
            }
        }

        public static async Task<TeamViewModel> WithTeamRoles(this TeamViewModel viewModel)
        {
            if (viewModel is PrivateChatViewModel)
            {
                return (PrivateChatViewModel)viewModel;
            }

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

        public static async Task<MemberViewModel> WithMemberRoles(this MemberViewModel viewModel)
        {
            /** LOAD CUSTOM ROLES FOR THE TEAM **/
            IEnumerable<TeamRole> allRoles = await TeamService.ListRoles(viewModel.TeamId);
            IEnumerable<TeamRoleViewModel> allRoleViewModels = await Map(allRoles).WithPermissions();

            IEnumerable<TeamRole> userRoles = await TeamService.GetUsersRoles(viewModel.TeamId, viewModel.Id);

            if (userRoles == null || userRoles.Count() <= 0)
            {
                foreach (TeamRoleViewModel assignable in allRoleViewModels)
                {
                    viewModel.AssignableMemberRoles.Add(assignable);
                }

                return viewModel;
            }

            /** LOAD PERMISSIONS FOR EACH ROLE **/
            foreach (TeamRoleViewModel roleViewModel in allRoleViewModels)
            {
                if (userRoles.Any(u => u.Id == roleViewModel.Id))
                {
                    viewModel.MemberRoles.Add(roleViewModel);
                }
                else
                {
                    viewModel.AssignableMemberRoles.Add(roleViewModel);
                }
            }

            return viewModel;
        }

        #endregion

        #region TeamRole Extension

        public static async Task<TeamRoleViewModel> WithPermissions(this TeamRoleViewModel viewModel)
        {
            IList<Permissions> permissions = await TeamService.GetPermissionsOfRole(viewModel.TeamId, viewModel.Title);

            if (permissions == null || permissions.Count() <= 0)
            {
                return viewModel;    
            }

            viewModel.Permissions.Clear();

            foreach (Permissions permission in permissions)
            {
                viewModel.Permissions.Add(permission);
            }

            return viewModel;
        }

        public static async Task<IEnumerable<TeamRoleViewModel>> WithPermissions(this IEnumerable<TeamRoleViewModel> viewModels)
        {
            foreach (TeamRoleViewModel viewModel in viewModels)
            {
                await viewModel.WithPermissions();
            }

            return viewModels;
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
                Title = string.Concat(teamRole.Role.Substring(0, 1).ToUpper(), teamRole.Role.Substring(1)),
                TeamId = teamRole.TeamId,
                Permissions = new ObservableCollection<Permissions>(),
                Color = teamRole.Color.ToColor(),
            };
        }

        public static IEnumerable<TeamRoleViewModel> Map(IEnumerable<TeamRole> teamRoles)
        {
            return teamRoles.Select(Map);
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

        public static IEnumerable<ChannelViewModel> Map(IEnumerable<Channel> channels)
        {
            List<ChannelViewModel> viewModels = new List<ChannelViewModel>();

            foreach (Channel channel in channels)
            {
                viewModels.Add(Map(channel));
            }

            return viewModels;
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
                MemberRoles = new List<TeamRoleViewModel>(),
                AssignableMemberRoles = new List<TeamRoleViewModel>()
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
