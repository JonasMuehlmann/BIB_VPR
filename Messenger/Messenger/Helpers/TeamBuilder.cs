using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Helpers
{
    public class TeamBuilder
    {
        private readonly UserViewModel _user;

        public TeamBuilder(UserViewModel user)
        {
            _user = user;
        }

        public async Task<TeamViewModel> Build(Team team)
        {
            TeamViewModel viewModel = Map(team);

            var withMembers = await LoadMembers(viewModel, _user.Id);

            var withChannels = await LoadChannels(withMembers);

            return withChannels;
        }

        public async Task<List<TeamViewModel>> Build(IEnumerable<Team> teams)
        {
            List<TeamViewModel> result = new List<TeamViewModel>();

            foreach (Team team in teams)
            {
                var viewModel = await Build(team);
                result.Add(viewModel);
            }

            return result;
        }

        private TeamViewModel Map(Team team)
        {
            return new TeamViewModel()
            {
                Id = team.Id,
                TeamName = team.Name,
                Description = team.Description,
                CreationDate = team.CreationDate,
                Members = new ObservableCollection<User>(),
                Channels = new ObservableCollection<ChannelViewModel>()
            };
        }

        private ChannelViewModel Map(Channel channel)
        {
            return new ChannelViewModel()
            {
                ChannelId = channel.ChannelId,
                TeamId = channel.TeamId,
                ChannelName = channel.ChannelName
            };
        }

        /// <summary>
        /// Determines the type of team and sets the corresponding member models
        /// </summary>
        /// <param name="team">Team object to reference to</param>
        /// <param name="members">List of members to set</param>
        private async Task<TeamViewModel> LoadMembers(TeamViewModel viewModel, string currentUserId)
        {
            var members = await MessengerService.LoadTeamMembers((uint)viewModel.Id);

            if (string.IsNullOrEmpty(viewModel.TeamName))
            {
                viewModel.Members = new ObservableCollection<User>(members.Where(m => m.Id != currentUserId));
            }
            else
            {
                viewModel.Members = new ObservableCollection<User>(members);
            }

            return viewModel;
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

        public async Task<TeamViewModel> LoadChannels(uint teamId)
        {
            Team team = await MessengerService.GetTeam(teamId);
            TeamViewModel viewModel = await Build(team);

            return await LoadChannels(viewModel);
        }
    }
}
