using Messenger.Core.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.ViewModels.DataViewModels
{
    public class NotificationMessageViewModel : DataViewModel
    {
        private NotificationType _type;

        private NotificationSource _source;

        private string _teamName;

        public NotificationType Type
        {
            get { return _type; }
            set { Set(ref _type, value); }
        }

        public NotificationSource Source
        {
            get { return _source; }
            set { Set(ref _source, value); }
        }

        public string TeamName
        {
            get { return _teamName; }
            set { Set(ref _teamName, value); }
        }

        public NotificationMessageViewModel()
        {

        }

        public void AsInvitedToTeamNotificationMessage(JObject message)
        {
            var type = Enum.Parse(typeof(NotificationType), message["notificationType"].Value<string>());

            if (type != null)
            {
                Type = (NotificationType)type;
            }

            var source = Enum.Parse(typeof(NotificationSource), message["notificationSource"].Value<string>());

            if (type != null)
            {
                Source = (NotificationSource)source;
            }

            string teamName = message["teamName"].Value<string>();

            if (!string.IsNullOrEmpty(teamName))
            {
                TeamName = teamName;
            }
        }
    }
}
