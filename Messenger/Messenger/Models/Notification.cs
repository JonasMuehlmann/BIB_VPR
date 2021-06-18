using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Models
{
    public class Notification
    {
        public string User { get; set; }

        public DateTime WhenSent { get; set; }

        public NotificationType KindOfMessage { get; set; }

        public string WhereSent { get; set; }
    }
}
