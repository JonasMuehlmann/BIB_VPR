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
using Messenger.Views.DialogBoxes;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ShellViewModel : Observable
    {
        #region Private

        private Team _currentTeam;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        #endregion

        public Team CurrentTeam
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

        public ICommand OpenUserProfileCommand => new RelayCommand(() => NavigationService.Open<SettingsPage>());

        public ICommand OpenTeamManagerCommand => new RelayCommand(() => NavigationService.Open<TeamManagePage>());

        public ICommand ChangeTeamDetailsCommand => new RelayCommand(RefactorTeamDetails);

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

        private async void OnTeamSwitched(object sender, IEnumerable<MessageViewModel> messages)
        {
            var team = await ChatHubService.GetCurrentTeam();

            if (team == null)
            {
                return;
            }

            bool isPrivateChat = team.Name == string.Empty;

            if (isPrivateChat)
            {
                var partnerName = team.Members.FirstOrDefault().DisplayName;
                CurrentTeam = new Team() { Name = partnerName, Description = team.Description };
            }
            else
            {
                CurrentTeam = team;
            }
        }

        private void OnTeamUpdated(object sender, Team team)
        {
            CurrentTeam = null;
            CurrentTeam = team;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            IdentityService.LoggedOut -= OnLoggedOut;
        }

        private async void RefactorTeamDetails()
        {
            if (ChatHubService.CurrentUser == null)
            {
                return;
            }

            //Get the current Team
            var team = await ChatHubService.GetCurrentTeam();

            // Opens the dialog box for the input
            var dialog = new ChangeTeamDialog()
            {
                TeamName = team.Name,
                TeamDescription = team.Description
            };

            // Create team on confirm
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await ChatHubService.UpdateTeam(dialog.TeamName, dialog.TeamDescription);
            }
        }
    }
}
