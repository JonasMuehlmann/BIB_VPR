using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Core.Helpers;
using System.Data.SqlClient;
using System;

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
        private User sampleUser = new User() {
            Id = "123-456-abc-edf",
            DisplayName = "Jay Kim / PBT3H19AKI",
            Mail = "test.bib@edu.bib"
            };

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
        public void GetOrCreateApplicationUserExisting_Test()
        {
            Task.Run(async () =>
            {
                var data = new User() { Id = "123-456-abc-edf", DisplayName="testUser"};
                User retrievedUser = await userService.GetOrCreateApplicationUser(data);


                Assert.IsNotNull(retrievedUser);
                Assert.AreEqual(retrievedUser.Mail, "test.bib@edu.bib");

            }).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Should fetch the existing user from database
        /// </summary>
        [TestMethod]
        public void GetOrCreateApplicationUserFirstNameId_Test()
        {
            Task.Run(async () =>
            {
                var data = new User { Id = "xyz", DisplayName = "foobar" };

                User retrievedUser = await userService.GetOrCreateApplicationUser(data);

                Assert.AreEqual(0u, retrievedUser.NameId);

            }).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Should fetch the existing user from database
        /// </summary>
        [TestMethod]
        public void GetOrCreateApplicationUserSecondNameId_Test()
        {
            Task.Run(async () =>
            {
                var data = new User() { Id = "123", DisplayName = "foobar" };
                User retrievedUser = await userService.GetOrCreateApplicationUser(data);

                Assert.AreEqual(1u, retrievedUser.NameId);

            }).GetAwaiter().GetResult();
        }


         public void GetOrCreateApplicationUserNew_Test()
         {
             string id = "123-456-abc-edg";

             User referenceUser = new User{
                 Id = id
             };

             Task.Run(async () =>
             {
                 var data = new User() { Id =  id};
                 User createdUser = await userService.GetOrCreateApplicationUser(data);

                 Assert.AreEqual(createdUser.ToString(), referenceUser.ToString());

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
                string id = "123-456-abc-edf";

                User userBefore = await userService.GetUser(id);
                string newName = "JayKim94";
                userBefore.DisplayName = newName;

                bool success = await userService.UpdateUsername(id, newName);

                User userAfter = await userService.GetUser(id);

                Assert.IsTrue(success);
                Assert.AreEqual(userBefore.ToString(), userAfter.ToString());

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
                bool success = await userService.Update("123-456-abc-edf", "Bio", "Updated bio");

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
                string id = "123-456-abc-edf";
                bool success = await userService.DeleteUser(id);


                Assert.IsTrue(success);
                Assert.IsNull(await userService.GetUser(id));

            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should delete user from the setup, expects true
        /// </summary>
        [TestMethod]
        public void DeleteNonExistentUser_Test()
        {
            Task.Run(async () =>
            {
                string id = "djdsdjksdjskdjskdjdksj";
                bool success = await userService.DeleteUser(id);


                Assert.IsFalse(success);

            }).GetAwaiter().GetResult();
        }

        [AssemblyCleanup]
        public static void Cleanup()
        {
            // Reset DB
            string query = "DELETE FROM Memberships;"
                         + "DELETE FROM Messages;"
                         + "DELETE FROM Teams;"
                         + "DELETE FROM Users;";

            using (SqlConnection connection = AzureServiceBase.GetConnection(TEST_CONNECTION_STRING))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                bool result = Convert.ToBoolean(cmd.ExecuteNonQuery());

                Assert.IsTrue(result);
            }
        }
    }
}
