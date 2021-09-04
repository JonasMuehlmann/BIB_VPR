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
        [TestMethod]
        public void CreateMessage_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"});

                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var result = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message");
                Assert.IsTrue(result.Value > 0);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessages_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"});

                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var result = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message");

                var messages = await MessageService.RetrieveMessages(teamId.Value);
                Assert.IsTrue(messages.Count > 0);
                Assert.IsNotNull(messages[0]);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessagesNoneExist_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"});

                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var messages = await MessageService.RetrieveMessages(channelId.Value);

                Assert.IsTrue(messages.Count == 0);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RenameMessage_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var messages = (await MessageService.RetrieveMessages(channelId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var oldContent = messages[0].Content;

                bool success = await MessageService.EditMessage(messageId.Value, testName + "MessageNewContent");

                Assert.IsTrue(success);

                messages = (await MessageService.RetrieveMessages(channelId.Value));

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
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);


                var  numMessagesBefore = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                var success = await MessageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                Assert.IsTrue(numMessagesAfter < numMessagesBefore);


            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMessageWithReplies_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? replyMessageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message", messageId);
                Assert.IsNotNull(replyMessageId);

                var  numMessagesBefore = (await MessageService.RetrieveMessages(channelId.Value)).Count();
                Assert.AreEqual(2, numMessagesBefore);

                var success = await MessageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(channelId.Value)).Count();
                Assert.AreEqual(0, numMessagesAfter);


            }).GetAwaiter().GetResult();
        }
        [TestMethod]
        public void RemoveMessageWithReactions_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var  numMessagesBefore = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                var success = await MessageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                Assert.IsTrue(numMessagesAfter < numMessagesBefore);


            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddReaction_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var reactions = await MessageService.RetrieveReactions(messageId.Value);

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
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var didRemoveReaction = await MessageService.RemoveReaction(messageId.Value, userId, "🈚");
                Assert.IsTrue(didRemoveReaction);

                var reactions = await MessageService.RetrieveReactions(messageId.Value);

                Assert.AreEqual(0, reactions.Count());

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveReplies_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"});

                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var parentMessageId = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message");
                Assert.IsNotNull(parentMessageId);

                var childMessageId1 = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message", parentMessageId.Value);
                Assert.IsNotNull(childMessageId1);

                var childMessageId2 = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message", parentMessageId.Value);
                Assert.IsNotNull(childMessageId2);

                var repliesToParentMessage = await MessageService.RetrieveReplies(parentMessageId.Value);

                Assert.AreEqual(repliesToParentMessage.Count, 2);

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }
    }
}
