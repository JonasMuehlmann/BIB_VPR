
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
    public class PrivateChatServiceTest
    {
        [TestMethod]
        public void GetAllPrivateChatsNoneExist_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var privateChats = await PrivateChatService.GetAllPrivateChatsFromUser(testName + "User");

                Assert.IsTrue(Enumerable.Count(privateChats) == 0);

            }).GetAwaiter().GetResult();
        }


        [TestMethod]
        public void CreatePrivateChat_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId1 = testName + "User1";
                string userId2 = testName + "User2";

                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id=userId1});
                _ = await UserService.GetOrCreateApplicationUser(new User(){Id=userId2});

                var privateChatId = await PrivateChatService.CreatePrivateChat(userId1, userId2);
                Assert.IsNotNull(privateChatId);

            }).GetAwaiter().GetResult();
        }


        [TestMethod]
        public void GetAllPrivateChats_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId1 = testName + "User1";
                string userId2 = testName + "User2";

                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id=userId1});
                _ = await UserService.GetOrCreateApplicationUser(new User(){Id=userId2});

                var privateChatId = await PrivateChatService.CreatePrivateChat(userId1, userId2);
                Assert.IsNotNull(privateChatId);

                var privateChats = await PrivateChatService.GetAllPrivateChatsFromUser(testName + "User1");
                Assert.IsNotNull(privateChats);
                Assert.AreEqual(1, Enumerable.Count(privateChats));

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }
    }
}
