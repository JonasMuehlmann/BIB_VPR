using System.Linq;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;


namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class NotificationMessageBuilderTest
    {
        [TestMethod]
        public void MakeMessageInSubscribedChannelNotificationMessage_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "User", DisplayName = testName + "User"})).Id;
                Assert.IsNotNull(userId);
                Assert.AreNotEqual("",userId);

                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var notificationMessageBuilt = await NotificationMessageBuilder.MakeMessageInSubscribedChannelNotificationMessage(messageId.Value);

                var notificationMessageReference = new JObject()
                {
                    {"notificationType",   NotificationType.MessageInSubscribedChannel.ToString()},
                    {"notificationSource", NotificationSource.Channel.ToString()},
                    {"messageId"          ,messageId.Value.ToString()},
                    {"senderName",         testName + "User"},
                    {"channelName",        testName + "Channel"},
                    {"channelId",          channelId.Value},
                    {"teamName",           testName + "Team"}
                };

                Assert.AreEqual(notificationMessageReference.ToString(), notificationMessageBuilt.ToString());

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void MakeMessageInSubscribedTeamNotificationMessage_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "User", DisplayName = testName + "User"})).Id;
                Assert.IsNotNull(userId);
                Assert.AreNotEqual("",userId);

                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var notificationMessageBuilt = await NotificationMessageBuilder.MakeMessageInSubscribedTeamNotificationMessage(messageId.Value);

                var notificationMessageReference = new JObject()
                {
                    {"notificationType",   NotificationType.MessageInSubscribedTeam.ToString()},
                    {"notificationSource", NotificationSource.Team.ToString()},
                    {"messageId"          ,messageId.Value.ToString()},
                    {"senderName",         testName + "User"},
                    {"channelName",        testName + "Channel"},
                    {"channelId",          channelId.Value},
                    {"teamName",           testName + "Team"}
                };

                Assert.AreEqual(notificationMessageReference.ToString(), notificationMessageBuilt.ToString());

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void MakeMessageInPrivateChatNotificationMessage_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId1 = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "User1"})).Id;
                Assert.IsNotNull(userId1);
                Assert.AreNotEqual("",userId1);

                string userId2 = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "User2"})).Id;
                Assert.IsNotNull(userId2);
                Assert.AreNotEqual("",userId2);


                uint? teamId = await PrivateChatService.CreatePrivateChat(userId1, userId2);
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId1, testName + "Message");
                Assert.IsNotNull(messageId);

                var notificationMessageBuilt = await NotificationMessageBuilder.MakeMessageInPrivateChatNotificationMessage(messageId.Value);

                var notificationMessageReference = new JObject()
                {
                    {"notificationType",   NotificationType.MessageInPrivateChat.ToString()},
                    {"notificationSource", NotificationSource.PrivateChat.ToString()},
                    {"messageId"          , messageId.Value.ToString()},
                    {"partnerName",        testName + "User1"},
                    {"channelId",          channelId.Value}
                };

                Assert.AreEqual(notificationMessageReference.ToString(), notificationMessageBuilt.ToString());

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void MakeInvitedToTeamNotificationMessage_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "User"})).Id;
                Assert.IsNotNull(userId);
                Assert.AreNotEqual("",userId);

                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var notificationMessageBuilt = await NotificationMessageBuilder.MakeInvitedToTeamNotificationMessage(teamId.Value);

                var notificationMessageReference = new JObject()
                {
                    {"notificationType",   NotificationType.InvitedToTeam.ToString()},
                    {"notificationSource", NotificationSource.Team.ToString()},
                    {"teamName",           testName + "Team"},
                    {"teamId",             teamId.Value},
                };

                Assert.AreEqual(notificationMessageReference.ToString(), notificationMessageBuilt.ToString());

            }).GetAwaiter().GetResult();
        }
    }
}
