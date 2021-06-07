
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Data.SqlClient;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Core.Helpers;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.TeamService
    /// </summary>
    [TestClass]
    public class PrivateChatServiceTest: SqlServiceTestBase
    {
        PrivateChatService privateChatService;
        UserService userService;

        /// <summary>
        /// Initialize the service
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
             privateChatService = InitializeTestMode<PrivateChatService>();
             userService = InitializeTestMode<UserService>();
        }


        [TestMethod]
        public void GetAllPrivateChatsNoneExist_Test()
        {
            Task.Run(async () =>
            {
                var privateChats = await privateChatService.GetAllPrivateChatsFromUser("user123");

                Assert.IsTrue(Enumerable.Count(privateChats) == 0);

            }).GetAwaiter().GetResult();
        }


        [TestMethod]
        public void CreatePrivateChat_Test()
        {
            Task.Run(async () =>
            {
                string userId1 = "user123";
                string userId2 = "user456";

                var _ = await userService.GetOrCreateApplicationUser(new User(){Id=userId1});
                _ = await userService.GetOrCreateApplicationUser(new User(){Id=userId2});

                var privateChatId = await privateChatService.CreatePrivateChat(userId1, userId2);

                Assert.IsNotNull(privateChatId);

            }).GetAwaiter().GetResult();
        }


        [TestMethod]
        public void GetAllPrivateChats_Test()
        {
            Task.Run(async () =>
            {
                var privateChats = await privateChatService.GetAllPrivateChatsFromUser("user456");

                Assert.IsNotNull(privateChats);
                Assert.IsTrue(Enumerable.Count(privateChats) > 0);

            }).GetAwaiter().GetResult();
        }
    }
}
