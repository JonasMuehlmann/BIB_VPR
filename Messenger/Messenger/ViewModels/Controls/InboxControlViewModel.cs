using Messenger.Helpers;
using Messenger.Models;
using System;
using System.Collections.ObjectModel;

namespace Messenger.ViewModels.Controls
{
    public class InboxControlViewModel : Observable
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
        public InboxControlViewModel()
        {
            Notifications = new ObservableCollection<Notification>();
            Notifications.Clear();
            // TODO: Load actual data
            Notifications.Add(new Notification()
            {
                User = "John Smith",
                KindOfMessage = NotificationType.Mention,
                WhereSent = "[Team] bib International College",
                WhenSent = new DateTime(2021, 4, 2, 8, 42, 5)
            });
        }
    }
}
