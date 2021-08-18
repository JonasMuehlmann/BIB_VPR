using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.ViewModels.DataViewModels
{
    public class NotificationViewModel : DataViewModel
    {
        private uint _id;

        private string _recipientId;

        private DateTime _creationTime;

        private JObject _message;

        private NotificationMessageViewModel _messageViewModel;

        public uint Id
        {
            get { return _id; }
            set { Set(ref _id, value);  }
        }

        public string RecipientId
        {
            get { return _recipientId; }
            set { Set(ref _recipientId, value); }
        }

        public DateTime CreationTime
        {
            get { return _creationTime; }
            set { Set(ref _creationTime, value); }
        }

        public NotificationMessageViewModel MessageViewModel
        {
            get { return _messageViewModel; }
            set { Set(ref _messageViewModel, value); }
        }

        public NotificationViewModel()
        {

        }

        public NotificationViewModel(Notification data)
        {
            Id = data.Id;
            RecipientId = data.RecipientId;
            CreationTime = data.CreationTime;

            _message = data.Message;

            if (Enum.TryParse(typeof(NotificationType), _message["notificationType"].Value<string>(), out object result))
            {
                NotificationType type = (NotificationType)result;

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
                        MessageViewModel.AsInvitedToTeamNotificationMessage(_message);
                        break;
                    case NotificationType.RemovedFromTeam:
                        break;
                    case NotificationType.ReactionToMessage:
                        break;
                    default:
                        break;
                }
            }
        }
    }
}
