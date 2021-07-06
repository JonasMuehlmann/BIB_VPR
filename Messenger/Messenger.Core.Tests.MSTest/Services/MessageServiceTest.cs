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
    public class MessageServiceTest
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
            channelService = new ChannelService();
        }


        [TestMethod]
        public void CreateMessage_Test()
        {

           Task.Run(async () =>
           {
                var _ = await userService.GetOrCreateApplicationUser(new User(){Id="user1"});

                var teamId = await teamService.CreateTeam("MyTestTeam");
                Assert.IsNotNull(teamId);

                var channelId = await channelService.CreateChannel("MyTestChannel", teamId.Value);
                Assert.IsNotNull(channelId);

                var result = await messageService.CreateMessage(channelId.Value,"user1", "my message text");

                Assert.IsTrue(result.Value > 0);

           }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessages_Test()
        {
            Task.Run(async () =>
            {
            var _ = await userService.GetOrCreateApplicationUser(new User(){Id="user1"});
                var teamId = await teamService.CreateTeam("MyTestTeam123");
                Assert.IsNotNull(teamId);

                var channelId = await channelService.CreateChannel("MyTestChannel123", teamId.Value);
                Assert.IsNotNull(channelId);

                var result = await messageService.CreateMessage(channelId.Value,"user1", "my message text");

                var messages = await messageService.RetrieveMessages(teamId.Value);

                Assert.IsTrue(messages.Count > 0);
                Assert.IsNotNull(messages[0]);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessagesNoneExist_Test()
        {
            Task.Run(async () =>
            {
                var _ = await userService.GetOrCreateApplicationUser(new User(){Id="user1"});
                var teamId = await teamService.CreateTeam("MyTestTeamXYZ");
                Assert.IsNotNull(teamId);

                var channelId = await channelService.CreateChannel("MyTestChannelXYZ", teamId.Value);
                Assert.IsNotNull(channelId);

                var messages = await messageService.RetrieveMessages(channelId.Value);

                Assert.IsTrue(messages.Count == 0);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RenameMessage_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await userService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await messageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var messages = (await messageService.RetrieveMessages(channelId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var oldContent = messages[0].Content;

                bool success = await messageService.EditMessage(messageId.Value, testName + "MessageNewContent");

                Assert.IsTrue(success);

                messages = (await messageService.RetrieveMessages(channelId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var newContent = messages[0].Content;


                Assert.AreEqual(oldContent + "NewContent", newContent);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMessage_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await userService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await messageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);


                var  numMessagesBefore = (await messageService.RetrieveMessages(channelId.Value)).Count();

                var success = await messageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await messageService.RetrieveMessages(channelId.Value)).Count();

                Assert.IsTrue(numMessagesAfter < numMessagesBefore);


            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddReaction_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await userService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await messageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await messageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var reactions = await messageService.RetrieveReactions(messageId.Value);

                Assert.AreEqual(reactions.Count(), 1);
                Assert.AreEqual(reactions.ToList()[0].Symbol, "🈚");

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveReaction_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await userService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await messageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await messageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var didRemoveReaction = await messageService.RemoveReaction(messageId.Value, userId, "🈚");
                Assert.IsTrue(didRemoveReaction);

                var reactions = await messageService.RetrieveReactions(messageId.Value);

                Assert.AreEqual(0, reactions.Count());

            }).GetAwaiter().GetResult();
        }
    }
}
