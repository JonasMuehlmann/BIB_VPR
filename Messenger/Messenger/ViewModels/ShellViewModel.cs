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

        private IdentityService IdentityService => Singleton<IdentityService>.Instance;

        #endregion

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
        }

        private void OnLoggedOut(object sender, EventArgs e)
        {
            IdentityService.LoggedOut -= OnLoggedOut;
        }
    }
}
