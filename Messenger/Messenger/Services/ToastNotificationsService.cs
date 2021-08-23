using System;
using System.Threading.Tasks;
using Messenger.Activation;
using Messenger.Core.Helpers;
using Serilog;
using Windows.ApplicationModel.Activation;
using Windows.UI.Notifications;

namespace Messenger.Services
{
    /// <summary>
    /// Sends a toast notification via Windows 10
    /// </summary>
    internal partial class ToastNotificationsService : ActivationHandler<ToastNotificationActivatedEventArgs>
    {
        private ILogger _logger => GlobalLogger.Instance;

        public void ShowToastNotification(ToastNotification toastNotification)
        {
            try
            {
                ToastNotificationManager.CreateToastNotifier().Show(toastNotification);
            }
            catch (Exception e)
            {
                _logger.Error($"Error while displaying Toast Notification: {e.Message}");
            }
        }

        protected override async Task HandleInternalAsync(ToastNotificationActivatedEventArgs args)
        {
            //// TODO WTS: Handle activation from toast notification
            //// More details at https://docs.microsoft.com/windows/uwp/design/shell/tiles-and-notifications/send-local-toast

            await Task.CompletedTask;
        }
    }
}
