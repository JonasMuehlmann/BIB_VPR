using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;
using System.Data.SqlClient;
using System.Collections.Generic;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.UserService
    /// </summary>
    [TestClass]
    public class UserServiceTest
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
            userService = new UserService();

            UserService.logger.Information("Creating example user!");

            // setting up example data for delete operation
            Task.Run(async () =>
            {
                await userService.GetOrCreateApplicationUser(sampleUser);
            }).GetAwaiter().GetResult();

            UserService.logger.Information("Finished creating example user!");
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

                Assert.AreEqual(1u, retrievedUser.NameId);

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
                var data = new User() { Id = "1234", DisplayName = "foobar" };
                User retrievedUser = await userService.GetOrCreateApplicationUser(data);

                Assert.AreEqual(2u, retrievedUser.NameId);

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

        [TestMethod]
        public void SearchUser_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            Task.Run(async () =>
            {
                List<User> users = new List<User>{
                                       new User(){Id="Id1",  NameId=1, DisplayName=$"{testName}1"}
                                     , new User(){Id="Id2",  NameId=2, DisplayName=$"{testName}1"}
                                     , new User(){Id="Id3",  NameId=1, DisplayName=$"The{testName}2"}
                                     , new User(){Id="Id4",  NameId=1, DisplayName=$"Another{testName}"}
                                     , new User(){Id="Id5",  NameId=1, DisplayName=$"YetAnother{testName}"}
                                     , new User(){Id="Id6",  NameId=1, DisplayName=$"ThisIsA{testName}"}
                                     , new User(){Id="Id7",  NameId=1, DisplayName=$"A{testName}ThisBe"}
                                     , new User(){Id="Id8",  NameId=1, DisplayName=$"SomeText"}
                                     , new User(){Id="Id9",  NameId=1, DisplayName=$"Yeet"}
                                     , new User(){Id="Id10", NameId=1, DisplayName=$"Oi mate"}
                                     , new User(){Id="Id11", NameId=1, DisplayName=$"Deez Nuts {testName}"}
                                     , new User(){Id="Id12", NameId=1, DisplayName=$"  "}
                                     , new User(){Id="Id13", NameId=1, DisplayName=$"jdhsjdhjdhj{testName}dksdskdjkdjsk"}
                                     , new User(){Id="Id14", NameId=1, DisplayName=$"ksjdksjdahdj"}
                                     , new User(){Id="Id15", NameId=1, DisplayName=$"jdhsjdhjdhj {testName} dksdskdjkdjsk"}
                                 };

                var userMatchString = $"{testName}1#000001,{testName}1#000002,The{testName}2#000001,Another{testName}#000001,ThisIsA{testName}#000001,A{testName}ThisBe#000001,YetAnother{testName}#000001,Deez Nuts {testName}#000001,jdhsjdhjdhj{testName}dksdskdjkdjsk#000001,jdhsjdhjdhj {testName} dksdskdjkdjsk#000001";

                foreach (var user in users)
                {
                    Assert.IsNotNull(await userService.GetOrCreateApplicationUser(user));
                }

                var userMatches = await userService.SearchUser(testName);
                Assert.IsNotNull(userMatches);

                Assert.AreEqual(userMatchString, string.Join(",", userMatches));

            }).GetAwaiter().GetResult();

        }
        [AssemblyCleanup]
        public static void Cleanup()
        {
            // Reset DB
            string query = @"
                             DELETE FROM Notifications;
                             DELETE FROM Reactions;
                             DELETE FROM Messages;
                             DELETE FROM Memberships;
                             DELETE FROM Channels;
                             DELETE FROM Role_permissions;
                             DELETE FROM User_roles;
                             DELETE FROM Team_roles;
                             DELETE FROM Teams;
                             DELETE FROM Users;
                             DBCC CHECKIDENT (Notifications,    RESEED, 0);
                             DBCC CHECKIDENT (Memberships,      RESEED, 0);
                             DBCC CHECKIDENT (Messages,         RESEED, 0);
                             DBCC CHECKIDENT (Channels,         RESEED, 0);
                             DBCC CHECKIDENT (Team_roles,       RESEED, 0);
                             DBCC CHECKIDENT (User_roles,       RESEED, 0);
                             DBCC CHECKIDENT (Role_permissions, RESEED, 0);
                             DBCC CHECKIDENT (Reactions, RESEED, 0);
                             DBCC CHECKIDENT (Teams,            RESEED, 0);";

            using (SqlConnection connection = AzureServiceBase.GetDefaultConnection())
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void ChangeBio_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var userId = testName + "UserId";

                var user = await userService.GetOrCreateApplicationUser(new User(){Id = userId,DisplayName = testName + "UserName", Bio=testName + "Bio"});
                Assert.IsNotNull(user);

                string oldBio = user.Bio;
                Assert.AreEqual(oldBio, testName + "Bio");

                var success = await userService.UpdateUserBio(userId, oldBio + "New");
                Assert.IsTrue(success);

                user = await userService.GetOrCreateApplicationUser(new User(){Id = userId});
                Assert.IsNotNull(user);

                string newBio = user.Bio;

                Assert.AreEqual(oldBio + "New", newBio);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ChangeMail_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var userId = testName + "UserId";

                var user = await userService.GetOrCreateApplicationUser(new User(){Id = userId,DisplayName = testName + "UserName", Mail=testName + "Mail"});
                Assert.IsNotNull(user);

                string oldEmail = user.Mail;
                Assert.AreEqual(oldEmail, testName + "Mail");

                var success = await userService.UpdateUserMail(userId, oldEmail + "New");
                Assert.IsTrue(success);

                user = await userService.GetOrCreateApplicationUser(new User(){Id = userId});
                Assert.IsNotNull(user);

                string newMail = user.Mail;

                Assert.AreEqual(oldEmail + "New", newMail);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ChangePhoto_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var userId = testName + "UserId";

                var user = await userService.GetOrCreateApplicationUser(new User(){Id = userId,DisplayName = testName + "UserName", Photo=testName + "Photo"});
                Assert.IsNotNull(user);

                string oldEmail = user.Photo;
                Assert.AreEqual(oldEmail, testName + "Photo");

                var success = await userService.UpdateUserPhoto(userId, oldEmail + "New");
                Assert.IsTrue(success);

                user = await userService.GetOrCreateApplicationUser(new User(){Id = userId});
                Assert.IsNotNull(user);

                string newPhoto = user.Photo;

                Assert.AreEqual(oldEmail + "New", newPhoto);

            }).GetAwaiter().GetResult();
        }

    }
}
