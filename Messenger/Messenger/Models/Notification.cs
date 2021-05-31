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
        public string KindOfMessage { get; set; }
        public string WhereSent { get; set; }


    }

    public class NotificationManager
    {
        ///<summary>
        ///GetNotifications() fügt Testdaten zu einer Liste hinzu und gibt dann diese Liste wieder
        ///</summary>
        ///<returns>
        ///gibt eine Liste von Testdaten wieder
        ///</returns>
        public static List<Notification> GetNotifications()
        {
            //Test Daten
            List<Notification> notifications = new List<Notification>();

            notifications.Add(new Notification { User = "Mark", WhenSent = new DateTime(2021, 5, 18), KindOfMessage = "replied in", WhereSent = "Private Chat" });
            notifications.Add(new Notification { User = "Lisa", WhenSent = new DateTime(2020, 9, 28), KindOfMessage = "mentioned you in", WhereSent = "Team2::Channel2" });
            notifications.Add(new Notification { User = "Anton", WhenSent = new DateTime(2021, 1, 4), KindOfMessage = "replied in", WhereSent = "Team2::Channel2" });
            notifications.Add(new Notification { User = "Anton", WhenSent = new DateTime(2021, 1, 4), KindOfMessage = "replied in", WhereSent = "Team2::Channel2" });
            notifications.Add(new Notification { User = "Anton", WhenSent = new DateTime(2021, 1, 4), KindOfMessage = "replied in", WhereSent = "Team2::Channel2" });

            return notifications;
        }
    }
}
