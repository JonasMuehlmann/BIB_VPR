using System.Threading.Tasks;
using System.Linq;
using System;
using System.Data.SqlClient;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Core.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.TeamService
    /// </summary>
    [TestClass]
    public class MessageServiceTest: SqlServiceTestBase
    {
        MessageService messageService;
        UserService userService;
        TeamService teamService;

        /// <summary>
        /// Initialize the service
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            messageService = InitializeTestMode<MessageService>();
            userService = InitializeTestMode<UserService>();
            teamService = InitializeTestMode<TeamService>();
        }


        [TestMethod]
        public void CreateMessage_Test()
        {

           Task.Run(async () =>
           {
                var _ = await userService.GetOrCreateApplicationUser(new User(){Id="user1"});
                var teamId = await teamService.CreateTeam("MyTestTeam");

                Assert.IsNotNull(teamId);

                var result = await messageService.CreateMessage(teamId.Value,"user1", "my message text");

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

                var result = await messageService.CreateMessage(teamId.Value,"user1", "my message text");

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

                var messages = await messageService.RetrieveMessages(teamId.Value);

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
                string userId = (await userService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;

                Assert.IsNotNull(userId);

                uint? messageId = await messageService.CreateMessage(teamId.Value, userId, testName + "Message");

                Assert.IsNotNull(messageId);

                var messages = (await messageService.RetrieveMessages(teamId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var oldContent = messages[0].Content;

                bool success = await messageService.EditMessage(messageId.Value, testName + "MessageNewContent");

                Assert.IsTrue(success);

                messages = (await messageService.RetrieveMessages(teamId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var newContent = messages[0].Content;


                Assert.AreEqual(oldContent + "NewContent", newContent);

            }).GetAwaiter().GetResult();
        }
    }
}
