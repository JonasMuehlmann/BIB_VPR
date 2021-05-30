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
        //private ICommand _loadedCommand;
        //private ICommand _itemInvokedCommand;
        private ICommand _userProfileCommand;
        private ICommand _teamCommand;
        private ICommand _chatCommand;
        private ICommand _notificationCommand;
        private UserViewModel _user;
        private Frame MainFrame { get; set; }
        private Frame SideFrame { get; set; }

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        //TODO Load again
        //public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        //public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        //chat headline
        private string _chatName;

        public string ChatName
        {
            get { return _chatName; }

            set { Set(ref _chatName, value); }
        }


        public ICommand UserProfileCommand => _userProfileCommand ?? (_userProfileCommand = new RelayCommand(OpenSetttingsMainPanel));
        public ICommand TeamCommand => _teamCommand ?? (_teamCommand = new RelayCommand(OpenTeamsSidePanel));
        public ICommand ChatCommand => _chatCommand ?? (_chatCommand = new RelayCommand(OpenChatSidePanel));
        public ICommand NotificationCommand => _notificationCommand ?? (_notificationCommand = new RelayCommand(OpenNotificationSidePanel));

        public UserViewModel User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame, Frame sideFrame)
        {
            //_keyboardAccelerators = keyboardAccelerators;
            MainFrame = frame;
            SideFrame = sideFrame;
            NavigationService.Frame = frame;
            OnLoaded();
            IdentityService.LoggedOut += OnLoggedOut;
            UserDataService.UserDataUpdated += OnUserDataUpdated;

            //load default pages
            OpenChatSidePanel();
        }

        private async void OnLoaded()
        {
            User = await UserDataService.GetUserAsync();
        }

        private void OnUserDataUpdated(object sender, UserViewModel userData)
        {
            User = userData;
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            UserDataService.UserDataUpdated -= OnUserDataUpdated;
            IdentityService.LoggedOut -= OnLoggedOut;
        }


        #region mainPageNavigation
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
        #endregion


        #region sidePageNavigation

        /// <summary>
        /// Opens the side Navigationpanels and the MainChatPanel
        /// </summary>
        /// <param name="page"></param>
        private void SideNavigation(Type page)
        {
            SideFrame.Navigate(page, this);
            //if (MainFrame.SourcePageType.Name != "ChatPage") {
                OpenChatMainPage();
            //}
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
