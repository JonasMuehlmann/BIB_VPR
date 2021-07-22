using Messenger.Helpers;
using Messenger.Helpers.MessageHelpers;
using Messenger.Helpers.TeamHelpers;
using Messenger.ViewModels.DataViewModels;
using System;

namespace Messenger.Services.Providers
{
    public class StateProvider : Observable
    {
        private TeamViewModel _selectedTeam;

        private ChannelViewModel _selectedChannel;

        private UserViewModel _currentUser;

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

        public StateProvider()
        {
            TeamManager = new TeamManager();
            MessageManager = new MessageManager();
        }
    }
}
