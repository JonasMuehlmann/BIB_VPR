
using System.Threading.Tasks;
using System.Linq;
using Messenger.Core.Models;
using Messenger.Core.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.TeamService
    /// </summary>
    [TestClass]
    public class MentionServiceTest
    {
        MessageService messageService;
        UserService userService;
        TeamService teamService;
        ChannelService channelService;

        /// <summary>
        /// Initialize the service
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            messageService = new MessageService();
            userService =    new UserService();
            teamService =    new TeamService();
            mentionService = new MentionService();
            channelService = new ChannelService();
        }

        [TestMethod]
        public void CreateMention_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await userService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await messageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? mentionId = await mentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMention_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await userService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await messageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? mentionId = await mentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

                bool didRemove = await mentionService.RemoveMention(mentionId.Value);
                Assert.IsTrue(didRemove);

            }).GetAwaiter().GetResult();
        }

    }
}
