﻿using Messenger.Core.Models;
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

namespace Messenger.Helpers
{
    public class TeamBuilder
    {
        public TeamBuilder()
        {
        }

        public async Task<TeamViewModel> Build(Team team, string userId)
        {
            TeamViewModel viewModel = Map(team);

            var withMembers = await LoadMembers(viewModel, userId);

            var withChannels = await LoadChannels(withMembers);

            return withChannels;
        }

        public async Task<List<TeamViewModel>> Build(IEnumerable<Team> teams, string userId)
        {
            List<TeamViewModel> result = new List<TeamViewModel>();

            foreach (Team team in teams)
            {
                var viewModel = await Build(team, userId);
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
                IsPrivateChat = string.IsNullOrEmpty(team.Name),
                Members = new ObservableCollection<User>(),
                Channels = new ObservableCollection<ChannelViewModel>()
            };
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
                viewModel.Members = new ObservableCollection<User>(members.Where(m => m.Id != currentUserId));
            }
            else
            {
                foreach (User member in members)
                {
                    var roles = await MessengerService.GetRolesList((uint)viewModel.Id, currentUserId);

                    if (roles != null)
                    {
                        // TODO
                        var memberRoles = roles.Select(role => new MemberRole()
                        {
                            Title = role,
                            TeamId = (uint)viewModel.Id,
                        });
                    }
                }

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

        public async Task<TeamViewModel> LoadChannels(uint teamId, string userId)
        {
            Team team = await MessengerService.GetTeam(teamId);
            TeamViewModel viewModel = await Build(team, userId);

            return await LoadChannels(viewModel);
        }
    }
}
