using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.Views;
using Messenger.Views.DialogBoxes;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace Messenger.ViewModels
{
    public class ShellViewModel : Observable
    {
        #region Private

        private ICommand _userProfileCommand;

        private ICommand _navigateToTeamsCommmand;

        private ICommand _navigateToChatsCommand;

        private ICommand _navigateToNotificationsCommand;

        private ICommand _teamManagerCommand;

        private ICommand _changeTeamDetailsCommand;

        private Team _currentTeam;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        public UserViewModel CurrentUser => ChatHubService.CurrentUser;

        private Frame MainFrame { get; set; }

        private Frame SideFrame { get; set; }

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

        public ICommand NavigateToTeamsCommmand => _navigateToTeamsCommmand ?? (_navigateToTeamsCommmand = new RelayCommand(OpenTeamsSidePanel));

        public ICommand NavigateToChatsCommand => _navigateToChatsCommand ?? (_navigateToChatsCommand = new RelayCommand(OpenChatSidePanel));

        public ICommand NavigateToNotificationsCommand => _navigateToNotificationsCommand ?? (_navigateToNotificationsCommand = new RelayCommand(OpenNotificationSidePanel));

        public ICommand OpenUserProfileCommand => _userProfileCommand ?? (_userProfileCommand = new RelayCommand(OpenSetttingsMainPanel));

        public ICommand OpenTeamManagerCommand => _teamManagerCommand ?? (_teamManagerCommand = new RelayCommand(OpenTeamManagePage));

        public ICommand ChangeTeamDetailsCommand => _changeTeamDetailsCommand ?? (_changeTeamDetailsCommand = new RelayCommand(RefactorTeamDetails));

        #endregion

        public ShellViewModel()
        {
            //ChatHubService = new ChatHubService();
        }

        public void Initialize(Frame frame, Frame sideFrame)
        {
            MainFrame = frame;
            SideFrame = sideFrame;

            NavigationService.Frame = frame;
            IdentityService.LoggedOut += OnLoggedOut;
            ChatHubService.TeamSwitched += OnTeamSwitched;
            ChatHubService.TeamUpdated += OnTeamChanged;

            // Sets the default side panel to TeamNavView
            OpenTeamsSidePanel();
        }

        private async void OnTeamSwitched(object sender, IEnumerable<Message> messages)
        {
            var team = await ChatHubService.GetCurrentTeam();
            CurrentTeam = team;
        }

        /// <summary>
        /// when somebody changed the team name or the team description
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="team"></param>
        private void OnTeamChanged(object sender, Team team)
        {
            CurrentTeam = team;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            IdentityService.LoggedOut -= OnLoggedOut;
        }

        private async void RefactorTeamDetails() {
            if (CurrentUser == null)
            {
                return;
            }

            // Opens the dialog box for the input
            var dialog = new ChangeTeamDialog();

            //Get the current Team
            var team = await ChatHubService.GetCurrentTeam();
            dialog.TeamName = team.Name;
            dialog.TeamDescription = team.Description;

            ContentDialogResult result = await dialog.ShowAsync();


            // Create team on confirm
            if (result == ContentDialogResult.Primary)
            {
                await ChatHubService.UpdateTeam(dialog.TeamName, dialog.TeamDescription);
            }
        }


        #region Main Page Navigation

        private void MainNavigation(Type page)
        {
            MainFrame.Navigate(page, this);
        }

        private void OpenSetttingsMainPanel()
        {
            MainNavigation(typeof(SettingsPage));
        }

        private void OpenChatMainPage()
        {
            MainNavigation(typeof(ChatPage));
        }

        private void OpenTeamManagePage()
        {
            MainNavigation(typeof(TeamManagePage));
        }

        #endregion


        #region Side Page Navigation

        /// <summary>
        /// Opens the side Navigationpanels and the MainChatPanel
        /// </summary>
        /// <param name="page"></param>
        private void SideNavigation(Type page)
        {
            SideFrame.Navigate(page, this);
            OpenChatMainPage();
            CurrentPageName = page.Name;
        }
        private void OpenTeamsSidePanel()
        {
            SideNavigation(typeof(TeamNavPage));
        }

        private void OpenChatSidePanel()
        {
            SideNavigation(typeof(ChatNavPage));
        }

        private void OpenNotificationSidePanel()
        {
            SideNavigation(typeof(NotificationNavPage));
        }

        #endregion
    }
}
