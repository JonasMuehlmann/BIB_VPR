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
        public void CreateMessageImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"})).Id;

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var result = await MessageService.CreateMessageImpl(channelId.Value, testName + "User", testName + "Message");
                Assert.IsTrue(result.Value > 0);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessages_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"})).Id;

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var result = await MessageService.CreateMessageImpl(channelId.Value, testName + "User", testName + "Message");

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
                var userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"})).Id;

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
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
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessageImpl(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var messages = (await MessageService.RetrieveMessages(channelId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var oldContent = messages[0].Content;

                bool success = await MessageService.EditMessageImpl(messageId.Value, testName + "MessageNewContent");

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
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessageImpl(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);


                var  numMessagesBefore = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                var success = await MessageService.DeleteMessageImpl(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                Assert.IsTrue(numMessagesAfter < numMessagesBefore);


            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddReactionImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessageImpl(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReactionImpl(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var reactions = await MessageService.RetrieveReactions(messageId.Value);

                Assert.AreEqual(reactions.Count(), 1);
                Assert.AreEqual(reactions.ToList()[0].Symbol, "🈚");

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveReactionImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessageImpl(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReactionImpl(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var didRemoveReactionImpl = await MessageService.RemoveReactionImpl(messageId.Value, userId, "🈚");
                Assert.IsTrue(didRemoveReactionImpl);

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
                var userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"})).Id;

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannelImpl(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var parentMessageId = await MessageService.CreateMessageImpl(channelId.Value, testName + "User", testName + "Message");
                Assert.IsNotNull(parentMessageId);

                var childMessageId1 = await MessageService.CreateMessageImpl(channelId.Value, testName + "User", testName + "Message", parentMessageId.Value);
                Assert.IsNotNull(childMessageId1);

                var childMessageId2 = await MessageService.CreateMessageImpl(channelId.Value, testName + "User", testName + "Message", parentMessageId.Value);
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
