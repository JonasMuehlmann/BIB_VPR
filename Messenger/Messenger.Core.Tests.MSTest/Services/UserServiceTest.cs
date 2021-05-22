using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.UserService
    /// </summary>
    [TestClass]
    public class UserServiceTest : SqlServiceTestBase
    {
        #region Private

        private UserService userService;
        private User sampleUser = new User() { Id = "123-456-abc-edf", DisplayName = "Jay Kim / PBT3H19AKI", Mail = "test.bib@edu.bib" };

        #endregion

        /// <summary>
        /// Initialize the service and the sample data
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            userService = InitializeTestMode<UserService>();
            // setting up example data for delete operation
            Task.Run(async () =>
            {
                await userService.GetOrCreateApplicationUser(sampleUser);
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should fetch the existing user from database
        /// </summary>
        [TestMethod]
        public void GetOrCreateApplicationUser_Test()
        {
            Task.Run(async () =>
            {
                var data = new User() { Id = "123-456-abc-edf" };
                User user = await userService.GetOrCreateApplicationUser(data);

                Assert.IsNotNull(user);
                Assert.AreEqual(user.Mail, "test.bib@edu.bib");
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should update username, expects true
        /// </summary>
        [TestMethod]
        public void UpdateUsername_Test()
        {
            Task.Run(async () =>
            {
                bool success = await userService.Update("7b0a54c3-f992-4bbd-abab-8028565287b3", "UserName", "Jay Kim");

                Assert.IsTrue(success);
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should update user bio, expects true
        /// </summary>
        [TestMethod]
        public void UpdateUserInfo_Test()
        {
            Task.Run(async () =>
            {
                bool success = await userService.Update("7b0a54c3-f992-4bbd-abab-8028565287b3", "Bio", "Updated bio");

                Assert.IsTrue(success);
            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should delete user from the setup, expects true
        /// </summary>
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
