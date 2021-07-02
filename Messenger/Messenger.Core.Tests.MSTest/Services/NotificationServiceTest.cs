using System.Linq;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;
using System.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;


namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class NotificationServiceTest
    {
        private NotificationService notificationService = new NotificationService();
        private UserService userService = new UserService();
        private TeamService teamService = new TeamService();
        private ChannelService channelService = new ChannelService();


        [TestMethod]
        public void SendNotification_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string receiverId = (await userService.GetOrCreateApplicationUser(new User(){Id = testName + "UserReceiver"})).Id;
                Assert.IsNotNull(receiverId);
                Assert.AreNotEqual("",receiverId);

                string senderId = (await userService.GetOrCreateApplicationUser(new User(){Id = testName + "UserSender"})).Id;
                Assert.IsNotNull(senderId);
                Assert.AreNotEqual("",senderId);

                uint? teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await channelService.CreateChannel(testName + "Chanel", teamId.Value);
                Assert.IsNotNull(channelId);

                var notificationMessage = new MessageInSubscribedChannelNotificationMessage()
                {
                    NotificationType   = NotificationType.MessageInSubscribedChannel,
                    NotificationSource = NotificationSource.Channel,
                    SenderId           = senderId,
                    TeamId             = teamId.Value,
                    ChannelId          = channelId.Value
                };
                uint? notificationId = await notificationService.SendNotification(receiverId, notificationMessage);
                Assert.IsNotNull(notificationId);

                var notifications = await notificationService.RetrieveNotifications(receiverId);
                Assert.AreEqual(1, Enumerable.Count(notifications));

                var notification = notifications.First();
                var message = notification.Message.ToObject<MessageInSubscribedChannelNotificationMessage>();

                Assert.AreEqual(notificationMessage.NotificationType  , message.NotificationType);
                Assert.AreEqual(notificationMessage.NotificationSource, message.NotificationSource);
                Assert.AreEqual(notificationMessage.SenderId          , message.SenderId);
                Assert.AreEqual(notificationMessage.TeamId            , message.TeamId);
                Assert.AreEqual(notificationMessage.ChannelId         , message.ChannelId);

            }).GetAwaiter().GetResult();
        }
    }
}
