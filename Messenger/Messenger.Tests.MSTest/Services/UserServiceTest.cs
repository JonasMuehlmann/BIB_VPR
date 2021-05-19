using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class UserServiceTest : SqlServiceTestBase
    {
        #region Private

        private UserService userService;

        #endregion

        [TestInitialize]
        public void Initialize()
        {
            userService = InitializeTestMode<UserService>();
        }

        [TestMethod]
        public void GetOrCreateApplicationUser_Test()
        {
            Task.Run(async () =>
            {
                var data = new User() 
                { 
                    Id = "123-456-abc-edf",
                    DisplayName = "Jay Kim / PBT3H19AKI",
                    Mail = "test.bib@edu.bib"
                };

                User user = await userService.GetOrCreateApplicationUser(data);

                Assert.IsNotNull(user);
                Assert.AreEqual(user.Id, "123-456-abc-edf");
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void UpdateUsername_Test()
        {
            Task.Run(async () =>
            {
                bool success = await userService.Update("7b0a54c3-f992-4bbd-abab-8028565287b3", "UserName", "Jay Kim");

                Assert.IsTrue(success);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void UpdateUserInfo_Test()
        {
            Task.Run(async () =>
            {
                bool success = await userService.Update("7b0a54c3-f992-4bbd-abab-8028565287b3", "Bio", "Updated bio");

                Assert.IsTrue(success);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void DeleteUser_Test()
        {
            Task.Run(async () =>
            {
                bool success = await userService.DeleteUser("123-456-abc-edf");

                Assert.IsTrue(success);
            }).GetAwaiter().GetResult();
        }
    }
}
