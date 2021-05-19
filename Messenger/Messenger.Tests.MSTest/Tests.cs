using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Messenger.Tests.MSTest
{
    [TestClass]
    public class Tests
    {
        #region Private

        private const string TEST_CONNECTION_STRING = @"Server=tcp:vpr.database.windows.net,1433;Initial Catalog=TEST_VPR_DATABASE;Persist Security Info=False;User ID=pbt3h19a;Password=uMb7ZXAA5TjajDw;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
        private UserService userService;

        #endregion

        [TestInitialize]
        public void Setup()
        {
            userService = new UserService();
            userService.SetTestMode(TEST_CONNECTION_STRING);
        }

        [TestMethod]
        public void GetOrCreateApplicationUser_Test()
        {
            Task.Run(async () =>
            {
                var data = new User() 
                { 
                    Id = "123-456-abc-edf",
                    DisplayName = "Jay Kim",
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
                bool success = await userService.Update("7b0a54c3-f992-4bbd-abab-8028565287b3", "Bio", "Test Bio");

                Assert.IsTrue(success);
            }).GetAwaiter().GetResult();
        }
    }
}
