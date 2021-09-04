using Messenger.Core.Models;
using Messenger.Models;
using System;
using Windows.UI.Xaml.Data;

namespace Messenger.Helpers.Converters
{
    /// <summary>
    /// Returns appropriate message for each notification type
    /// </summary>
    public class NotificationTypeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null
                || !(value is NotificationType))
            {
                return false;
            }

            NotificationType type = (NotificationType)value;

            string message = string.Empty;

            switch (type)
            {
                case NotificationType.UserMentioned:
                    break;
                case NotificationType.MessageInSubscribedChannel:
                    break;
                case NotificationType.MessageInSubscribedTeam:
                    break;
                case NotificationType.MessageInPrivateChat:
                    break;
                case NotificationType.InvitedToTeam:
                    message = "You are invited to a new team!";
                    break;
                case NotificationType.RemovedFromTeam:
                    break;
                case NotificationType.ReactionToMessage:
                    break;
                default:
                    break;
            }

            return message;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return false;
        }
    }
}
