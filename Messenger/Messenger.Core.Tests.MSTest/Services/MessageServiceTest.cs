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
                var result = await messageService.CreateMessage(teamId.Value,"user1", "my message text");

                Assert.IsTrue(result.Value > 0);

           }).GetAwaiter().GetResult();
        }

    }
}
