using Messenger.ViewModels.DataViewModels;
using Microsoft.Toolkit.Uwp.Notifications;
using System;
using Windows.UI.Notifications;

namespace Messenger.Services
{
    internal partial class ToastNotificationsService
    {
        public void ShowNotificationLoggedIn(UserViewModel user)
        {
            // Create the toast content
            var content = new ToastContentBuilder()
                .AddText($"Welcome back {user.Name}!")
                .AddText("Connection looks good :D")
                .AddAttributionText("Via Windows 10")
                .GetToastContent();

            // Add the content to the toast
            var toast = new ToastNotification(content.GetXml())
            {
                // TODO WTS: Set a unique identifier for this notification within the notification group. (optional)
                // More details at https://docs.microsoft.com/uwp/api/windows.ui.notifications.toastnotification.tag
                Tag = "ToastTag"
            };

            // And show the toast
            ShowToastNotification(toast);
        }
    }
}
