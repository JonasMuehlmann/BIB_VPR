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
        public void MakeMessageInSubscribedChannelNotificationMessage_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id = testName + "User"})).Id;
                Assert.IsNotNull(userId);
                Assert.AreNotEqual("",userId);

                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Chanel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var notificationMessageBuilt = NotificationMessageBuilder.MakeMessageInSubscribedChannelNotificationMessage(messageId.Value);

                var notificationMessageReference = new JObject()
                {
                    {"notificationType",   NotificationType.MessageInSubscribedChannel.ToString()},
                    {"notificationSource", NotificationSource.Channel.ToString()},
                    {"senderName",         testName + "User"},
                    {"channelName",        testName + "Channel"},
                    {"teamName",           testName + "Team"}
                };

                Assert.AreEqual(notificationMessageReference, notificationMessageBuilt);

            }).GetAwaiter().GetResult();

        }
    }
}
