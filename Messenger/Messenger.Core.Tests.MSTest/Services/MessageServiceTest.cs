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

           Task.Run(async () =>
           {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id="user1"});
                var teamId = await TeamService.CreateTeam("MyTestTeam");

                Assert.IsNotNull(teamId);

                var result = await MessageService.CreateMessage(teamId.Value,"user1", "my message text");

                Assert.IsTrue(result.Value > 0);

           }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessages_Test()
        {
            Task.Run(async () =>
            {
            var _ = await UserService.GetOrCreateApplicationUser(new User(){Id="user1"});
                var teamId = await TeamService.CreateTeam("MyTestTeam123");

                Assert.IsNotNull(teamId);

                var result = await MessageService.CreateMessage(teamId.Value,"user1", "my message text");

                var messages = await MessageService.RetrieveMessages(teamId.Value);

                Assert.IsTrue(messages.Count > 0);
                Assert.IsNotNull(messages[0]);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessagesNoneExist_Test()
        {
            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id="user1"});
                var teamId = await TeamService.CreateTeam("MyTestTeamXYZ");

                Assert.IsNotNull(teamId);

                var messages = await MessageService.RetrieveMessages(teamId.Value);

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

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(teamId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var messages = (await MessageService.RetrieveMessages(teamId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var oldContent = messages[0].Content;

                bool success = await MessageService.EditMessage(messageId.Value, testName + "MessageNewContent");

                Assert.IsTrue(success);

                messages = (await MessageService.RetrieveMessages(teamId.Value));

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

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(teamId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);


                var  numMessagesBefore = (await MessageService.RetrieveMessages(teamId.Value)).Count();

                var success = await MessageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(teamId.Value)).Count();

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

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(teamId.Value, userId, testName + "Message");
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

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(teamId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var didRemoveReaction = await MessageService.RemoveReaction(messageId.Value, userId, "🈚");
                Assert.IsTrue(didRemoveReaction);

                var reactions = await MessageService.RetrieveReactions(messageId.Value);

                Assert.AreEqual(0, reactions.Count());

            }).GetAwaiter().GetResult();
        }
    }
}
