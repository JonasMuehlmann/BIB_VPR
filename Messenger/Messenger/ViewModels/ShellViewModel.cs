﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.Views;

using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ShellViewModel : Observable
    {
        //private ICommand _loadedCommand;
        //private ICommand _itemInvokedCommand;
        private ICommand _userProfileCommand;
        private ICommand _TeamCommand;
        private ICommand _ChatCommand;
        private ICommand _NotificationCommand;
        private UserViewModel _user;
        private Frame MainFrame { get; set; }
        private Frame SideFrame { get; set; }

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        //TODO Load again
        //public ICommand LoadedCommand => _loadedCommand ?? (_loadedCommand = new RelayCommand(OnLoaded));

        //public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.NavigationViewItemInvokedEventArgs>(OnItemInvoked));

        public ICommand UserProfileCommand => _userProfileCommand ?? (_userProfileCommand = new RelayCommand(OnUserProfile));
        public ICommand TeamCommand => _TeamCommand ?? (_TeamCommand = new RelayCommand(OpenTeamsSidePanel));
        public ICommand ChatCommand => _ChatCommand ?? (_ChatCommand = new RelayCommand(OpenChatSidePanel));
        public ICommand NotificationCommand => _NotificationCommand ?? (_NotificationCommand = new RelayCommand(OpenNotificationSidePanel));
        public UserViewModel User
        {
            get { return _user; }
            set { Set(ref _user, value); }
        }

        public ShellViewModel()
        {
        }

        public void Initialize(Frame frame)
        {
            //_keyboardAccelerators = keyboardAccelerators;
            MainFrame = frame;
            NavigationService.Frame = frame;
            OnLoaded();
            IdentityService.LoggedOut += OnLoggedOut;
            UserDataService.UserDataUpdated += OnUserDataUpdated;
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

        private void OnUserProfile()
        {
            MainFrame.Navigate(typeof(SettingsPage), null);
        }

        private void SideNavigation(Type page)
        {
            SideFrame.Navigate(page, null);
        }

        private void OpenTeamsSidePanel() {
            SideNavigation(typeof(SettingsPage));
        }

        private void OpenChatSidePanel() {
            SideNavigation(typeof(SettingsPage));
        }

        private void OpenNotificationSidePanel() {
            SideNavigation(typeof(SettingsPage));
        }
    }
}
