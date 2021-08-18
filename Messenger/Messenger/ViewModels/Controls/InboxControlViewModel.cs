using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Messenger.ViewModels.Controls
{
    public class InboxControlViewModel : Observable
    {
        private ObservableCollection<NotificationViewModel> _notifications;

        public ObservableCollection<NotificationViewModel> Notifications
        {
            get { return _notifications; }
            set { Set(ref _notifications, value); }
        }

        public InboxControlViewModel()
        {
            Initialize();

            App.EventProvider.NotificationsLoaded += OnNotificationsLoaded;
            App.EventProvider.NotificationReceived += OnNotificationReceived;
        }

        private void Initialize()
        {
            Notifications = new ObservableCollection<NotificationViewModel>();

            if (App.StateProvider != null && App.StateProvider.Notifications != null)
            {
                LoadFromCache();
            }
        }

        private void LoadFromCache()
        {
            Notifications.Clear();

            foreach (NotificationViewModel viewModel in App.StateProvider.Notifications)
            {
                Notifications.Add(viewModel);
            }
        }

        #region Events

        private void OnNotificationsLoaded(object sender, BroadcastArgs e)
        {
            var notifications = e.Payload as IReadOnlyCollection<NotificationViewModel>;

            if (notifications != null && notifications.Count > 0)
            {
                Notifications.Clear();

                foreach (NotificationViewModel notification in notifications)
                {
                    Notifications.Add(notification);
                }
            }
        }

        private void OnNotificationReceived(object sender, BroadcastArgs e)
        {
            NotificationViewModel notification = e.Payload as NotificationViewModel;

            if (notification == null) return;

            if (e.Reason == BroadcastReasons.Created)
            {
                _notifications.Add(notification);
            }
        }

        #endregion
    }
}
