using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Models
{
    class Notification
    {
        public string User { get; set; }
        public DateTime whenSent { get; set; }
        public string kindOfMessage { get; set; }
        public string whereSent { get; set; }

        public Notification(string user, DateTime whenSent, string kindOfMessage, string whereSent)
        {
            User = user;
            this.whenSent = whenSent;
            this.kindOfMessage = kindOfMessage;
            this.whereSent = whereSent;
        }
    }
}
