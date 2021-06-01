using System;
using System.Collections.Generic;
using Messenger.Helpers;
using Messenger.Models;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class NotificationNavViewModel : Observable
    {
        private ShellViewModel _shellViewModel;

        public List<Notification> Notifications { get; set; }

        public ShellViewModel ShellViewModel { get {
                return _shellViewModel;
            }
            set {
                _shellViewModel = value;
                _shellViewModel.ChatName = "Chatname";
            } }

        public NotificationNavViewModel()
        {
            Notifications = NotificationManager.GetNotifications();
        }

    }
}
