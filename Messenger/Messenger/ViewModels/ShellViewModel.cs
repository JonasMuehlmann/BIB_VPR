using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.Views;
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

        private string _currentTeamName;

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        private Frame MainFrame { get; set; }

        private Frame SideFrame { get; set; }

        #endregion

        public string CurrentTeamName
        {
            get
            {
                return _currentTeamName;
            }
            set
            {
                Set(ref _currentTeamName, value);
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

        #endregion

        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame, Frame sideFrame)
        {
            MainFrame = frame;
            SideFrame = sideFrame;

            NavigationService.Frame = frame;
            IdentityService.LoggedOut += OnLoggedOut;

            // Sets the default side panel to TeamNavView
            OpenTeamsSidePanel();
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            IdentityService.LoggedOut -= OnLoggedOut;
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
