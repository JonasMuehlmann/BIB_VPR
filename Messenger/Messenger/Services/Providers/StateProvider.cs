using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Helpers.MessageHelpers;
using Messenger.Helpers.TeamHelpers;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Services.Providers
{
    public class StateProvider : Observable
    {
        private TeamViewModel _selectedTeam;

        private ChannelViewModel _selectedChannel;

        private UserViewModel _currentUser;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        public readonly TeamManager TeamManager;

        public readonly MessageManager MessageManager;

        public TeamViewModel SelectedTeam
        {
            get { return _selectedTeam; }
            set { Set(ref _selectedTeam, value); }
        }

        public ChannelViewModel SelectedChannel
        {
            get { return _selectedChannel; }
            set { Set(ref _selectedChannel, value); }
        }

        public UserViewModel CurrentUser
        {
            get { return _currentUser; }
            set { Set(ref _currentUser, value); }
        }

        private StateProvider()
        {
            TeamManager = new TeamManager(this);
            MessageManager = new MessageManager();
        }

        public static async Task<StateProvider> Initialize(UserViewModel user)
        {
            StateProvider provider = new StateProvider() { CurrentUser = user };

            /** LOAD TEAMS/PRIVATE CHATS FROM DATABASE **/
            await provider.TeamManager.Initialize(user);

            /** LOAD MESSAGES FOR TEAMS **/
            if (provider.TeamManager.MyTeams.Count() > 0)
            {
                await provider.MessageManager.CreateEntry(provider.TeamManager.MyTeams);
            }

            /** LOAD MESSAGES FOR PRIVATE CHATS **/
            if (provider.TeamManager.MyChats.Count() > 0)
            {
                await provider.MessageManager.CreateEntry(provider.TeamManager.MyChats);
            }

            return provider;
        }
    }
}
