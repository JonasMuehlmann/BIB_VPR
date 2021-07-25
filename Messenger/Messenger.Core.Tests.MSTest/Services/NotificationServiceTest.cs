using System.Linq;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
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
        [TestMethod]
        public void SendNotification_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string receiverId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "UserReceiver"})).Id;
                Assert.IsNotNull(receiverId);
                Assert.AreNotEqual("",receiverId);

                string senderId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "UserSender"})).Id;
                Assert.IsNotNull(senderId);
                Assert.AreNotEqual("",senderId);

                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Chanel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, senderId, testName + "Message");
                Assert.IsNotNull(messageId);

                var notificationMessage = new JObject
                {
                    {"notificationType"   , NotificationType.MessageInSubscribedChannel.ToString()},
                    {"notificationSource" , NotificationSource.Channel.ToString()},
                    {"messageId"          , messageId.Value.ToString()},
                    {"senderId"           , senderId},
                    {"teamId"             , teamId.Value},
                    {"channelId"          , channelId.Value}
                };
                uint? notificationId = await NotificationService.SendNotification(receiverId, notificationMessage);
                Assert.IsNotNull(notificationId);

                var notifications = await NotificationService.RetrieveNotifications(receiverId);
                Assert.AreEqual(1, Enumerable.Count(notifications));

                var notification = notifications.First();
                var message = notification.Message;

                Assert.AreEqual(notificationMessage.ToString(), message.ToString());

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveNotification_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string receiverId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "UserReceiver"})).Id;
                Assert.IsNotNull(receiverId);
                Assert.AreNotEqual("",receiverId);

                string senderId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "UserSender"})).Id;
                Assert.IsNotNull(senderId);
                Assert.AreNotEqual("",senderId);

                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Chanel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, senderId, testName + "Message");
                Assert.IsNotNull(messageId);

                var notificationMessage = new JObject
                {
                    {"notificationType"   , NotificationType.MessageInSubscribedChannel.ToString()},
                    {"notificationSource" , NotificationSource.Channel.ToString()},
                    {"messageId"          , messageId.Value.ToString()},
                    {"senderId"           , senderId},
                    {"teamId"             , teamId.Value},
                    {"channelId"          , channelId.Value}
                };
                uint? notificationId = await NotificationService.SendNotification(receiverId, notificationMessage);
                Assert.IsNotNull(notificationId);

                var didRemoveNotification = await NotificationService.RemoveNotification(notificationId.Value);
                Assert.IsTrue(didRemoveNotification);

                var notifications = await NotificationService.RetrieveNotifications(receiverId);

                Assert.AreEqual(0, Enumerable.Count(notifications));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddMute_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "UserSender"})).Id;
                Assert.IsNotNull( userId);
                Assert.AreNotEqual("", userId);

                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Chanel", teamId.Value);
                Assert.IsNotNull(channelId);

                var messageId = (await MessageService.CreateMessage(channelId.Value, userId, testName + "Message"));
                Assert.IsNotNull(messageId);

                var notification = await NotificationMessageBuilder.MakeMessageInSubscribedChannelNotificationMessage(messageId.Value);

                Assert.IsTrue(await NotificationService.CanSendNotification(notification, userId));

                var muteId = await NotificationService.AddMute(NotificationType.MessageInSubscribedChannel, NotificationSource.Channel, channelId.Value.ToString(), userId, userId);
                Assert.IsNotNull(muteId);

                Assert.IsFalse(await NotificationService.CanSendNotification(notification, userId));

            }).GetAwaiter().GetResult();
        }
    }
}
