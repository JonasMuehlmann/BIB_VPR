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
        private UserService userService;

        [TestInitialize]
        public void Setup()
        {
            userService = new UserService();
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
                bool success = await userService.Update("13a3993dc6752b11", "Bio", "Test Bio");

                Assert.IsTrue(success);
            }).GetAwaiter().GetResult();
        }
    }
}
