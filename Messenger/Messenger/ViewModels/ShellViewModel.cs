using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;

namespace Messenger.ViewModels
{
    public class ShellViewModel : Observable
    {
        #region Private

        private TeamViewModel _currentTeam;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        #endregion

        public TeamViewModel CurrentTeam
        {
            get
            {
                return _currentTeam;
            }
            set
            {
                Set(ref _currentTeam, value);
            }
        }

        private string _currentPageName;

        public string CurrentPageName
        {
            get
            {
                return _currentPageName;
            }
            set
            {
                Set(ref _currentPageName, value);
            }
        }

        #region Commands

        public ICommand NavigateToTeamsCommmand => new RelayCommand(() => NavigationService.Navigate<TeamNavPage>());

        public ICommand NavigateToChatsCommand => new RelayCommand(() => NavigationService.Navigate<ChatNavPage>());

        public ICommand NavigateToNotificationsCommand => new RelayCommand(() => NavigationService.Navigate<NotificationNavPage>());

        #endregion

        public ShellViewModel()
        {
            Initialize();
        }

        public void Initialize()
        {
            IdentityService.LoggedOut += OnLoggedOut;
            ChatHubService.TeamSwitched += OnTeamSwitched;
            ChatHubService.TeamUpdated += OnTeamUpdated;
        }

        private void OnTeamSwitched(object sender, IEnumerable<MessageViewModel> messages)
        {
            var team = ChatHubService.CurrentTeam;

            if (team == null)
            {
                return;
            }

            bool isPrivateChat = team.TeamName == string.Empty;

            if (isPrivateChat)
            {
                var partnerName = team.Members.FirstOrDefault().DisplayName;
                CurrentTeam = new TeamViewModel()
                {
                    TeamName = partnerName,
                    Description = team.Description
                };
            }
            else
            {
                CurrentTeam = team;
            }
        }

        private void OnTeamUpdated(object sender, TeamViewModel team)
        {
            CurrentTeam = null;
            CurrentTeam = team;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            IdentityService.LoggedOut -= OnLoggedOut;
        }
    }
}
