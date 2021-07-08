
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
            Task.Run(async () =>
            {
                var privateChats = await PrivateChatService.GetAllPrivateChatsFromUser("user1235");

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

                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id=userId1});
                _ = await UserService.GetOrCreateApplicationUser(new User(){Id=userId2});

                var privateChatId = await PrivateChatService.CreatePrivateChat(userId1, userId2);

                Assert.IsNotNull(privateChatId);

            }).GetAwaiter().GetResult();
        }


        [TestMethod]
        public void GetAllPrivateChats_Test()
        {
            Task.Run(async () =>
            {
                var privateChats = await PrivateChatService.GetAllPrivateChatsFromUser("user456");

                Assert.IsNotNull(privateChats);
                Assert.IsTrue(Enumerable.Count(privateChats) > 0);

            }).GetAwaiter().GetResult();
        }
    }
}
