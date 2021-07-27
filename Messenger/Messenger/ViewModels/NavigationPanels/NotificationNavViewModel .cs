using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Messenger.Commands;
using Messenger.Helpers;
using Messenger.Models;

namespace Messenger.ViewModels
{
    public class NotificationNavViewModel : Observable
    {
        private ObservableCollection<Notification> _notifications;

        public ObservableCollection<Notification> Notifications
        {
            get
            {
                return _notifications;
            }
            set
            {
                Set(ref _notifications, value);
            }
        }

        public ICommand ClearInboxCommand => new RelayCommand(ClearInbox);

        public ICommand RefreshInboxCommand => new RelayCommand(RefreshInbox);

        public NotificationNavViewModel()
        {
            Notifications = new ObservableCollection<Notification>();
            Notifications.Clear();
            // TODO: Load actual data from ChatHubService
            Notifications.Add(new Notification()
            {
                User = "John Smith",
                KindOfMessage = NotificationType.Mention,
                WhereSent = "[Team] bib International College",
                WhenSent = new DateTime(2021, 4, 2, 8, 42, 5)
            });
        }

        /// <summary>
        /// Clears the current inbox
        /// </summary>
        private void ClearInbox()
        {
            Notifications.Clear();
        }

        /// <summary>
        /// Manually loads the notifications to inbox
        /// </summary>
        private void RefreshInbox()
        {
            Notifications.Clear();

            // TODO: Load actual data from ChatHubService
            Notifications.Add(new Notification()
            {
                User = "Jay Kim",
                KindOfMessage = NotificationType.TeamJoined,
                WhereSent = "[Team] FHDW",
                WhenSent = new DateTime(2021, 4, 2, 8, 42, 5)
            });
        }
    }
}
