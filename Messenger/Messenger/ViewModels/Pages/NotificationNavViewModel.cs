using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Messenger.Commands;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.Controls;

namespace Messenger.ViewModels.Pages
{
    public class NotificationNavViewModel : Observable
    {
        public InboxControlViewModel InboxControlViewModel { get; set; }

        public ICommand ClearInboxCommand => new RelayCommand(ClearInbox);

        public ICommand RefreshInboxCommand => new RelayCommand(RefreshInbox);

        public NotificationNavViewModel()
        {
            InboxControlViewModel = new InboxControlViewModel();
        }

        /// <summary>
        /// Clears the current inbox
        /// </summary>
        private void ClearInbox()
        {
            InboxControlViewModel.Notifications.Clear();
        }

        /// <summary>
        /// Manually loads the notifications to inbox
        /// </summary>
        private void RefreshInbox()
        {
            InboxControlViewModel.Notifications.Clear();

            // TODO: Load actual data from ChatHubService
            InboxControlViewModel.Notifications.Add(new Notification()
            {
                User = "Jay Kim",
                KindOfMessage = NotificationType.TeamJoined,
                WhereSent = "[Team] FHDW",
                WhenSent = new DateTime(2021, 4, 2, 8, 42, 5)
            });
        }
    }
}
