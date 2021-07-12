using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Helpers
{
    public class TeamBuilder
    {
        public async Task<TeamViewModel> Build(Team team, UserViewModel currentUser)
        {
            throw new NotImplementedException();

            //var teams = await MessengerService.LoadTeams(currentUser.Id);

            //await SetMembers(teams, currentUser.Id);
            //teams = await GetChannelsForAllTeams(teams);
        }

        /// <summary>
        /// Determines the type of team and sets the corresponding member models
        /// </summary>
        /// <param name="team">Team object to reference to</param>
        /// <param name="members">List of members to set</param>
        private async Task SetMembers(IEnumerable<Team> teams, string currentUserId)
        {
            throw new NotImplementedException();

            //foreach (Team team in teams)
            //{
            //    var members = await MessengerService.LoadTeamMembers(team.Id);
            //    // If it is a private chat, exclude current user id
            //    if (string.IsNullOrEmpty(team.Name))
            //    {
            //        team.Members = members
            //            .Where(m => m.Id != currentUserId)
            //            .ToList();
            //    }
            //    else
            //    {
            //        team.Members = members.ToList();
            //    }
            //}
        }
    }
}
