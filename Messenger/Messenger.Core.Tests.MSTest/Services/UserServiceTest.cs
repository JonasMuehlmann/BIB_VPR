﻿using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Core.Helpers;
using System.Data.SqlClient;
using System;
using System.Collections.Generic;

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
                var data = new User() { Id = "1234", DisplayName = "foobar" };
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

        [TestMethod]
        public void SearchUser_Test()
        {
            Task.Run(async () =>
            {
                List<User> users = new List<User>{
                                       new User(){Id="Id1", NameId=0, DisplayName="UserName1"}
                                     , new User(){Id="Id2", NameId=1, DisplayName="UserName1"}
                                     , new User(){Id="Id3", NameId=0, DisplayName="TheUserName2"}
                                     , new User(){Id="Id4", NameId=0, DisplayName="AnotherUserName"}
                                     , new User(){Id="Id5", NameId=0, DisplayName="YetAnotherUserName"}
                                     , new User(){Id="Id6", NameId=0, DisplayName="ThisIsAUserName"}
                                     , new User(){Id="Id7", NameId=0, DisplayName="AUserNameThisBe"}
                                     , new User(){Id="Id8", NameId=0, DisplayName="SomeText"}
                                     , new User(){Id="Id9", NameId=0, DisplayName="Yeet"}
                                     , new User(){Id="Id10", NameId=0, DisplayName="Oi mate"}
                                     , new User(){Id="Id11", NameId=0, DisplayName="Deez Nuts UserName"}
                                     , new User(){Id="Id12", NameId=0, DisplayName="  "}
                                     , new User(){Id="Id13", NameId=0, DisplayName="jdhsjdhjdhjuserNamedksdskdjkdjsk"}
                                     , new User(){Id="Id14", NameId=0, DisplayName="ksjdksjdahdj"}
                                     , new User(){Id="Id15", NameId=0, DisplayName="jdhsjdhjdhj uSeRName dksdskdjkdjsk"}
                                 };

                var userMatchString = "UserName1#000000,UserName1#000001,TheUserName2#000000,AnotherUserName#000000,ThisIsAUserName#000000,AUserNameThisBe#000000,YetAnotherUserName#000000,Deez Nuts UserName#000000,jdhsjdhjdhjuserNamedksdskdjkdjsk#000000,jdhsjdhjdhj uSeRName dksdskdjkdjsk#000000";

                foreach (var user in users)
                {
                    Assert.IsNotNull(await userService.GetOrCreateApplicationUser(user));
                }

                var userMatches = await userService.SearchUser("UserName");
                Assert.IsNotNull(userMatches);

                Assert.AreEqual(userMatchString, string.Join(",", userMatches));

            }).GetAwaiter().GetResult();

        }
        [AssemblyCleanup]
        public static void Cleanup()
        {
            // Reset DB
            string query = "DELETE FROM Messages;"
                         + "DELETE FROM Memberships;"
                         + "DELETE FROM Channels;"
                         + "DELETE FROM Teams;"
                         + "DELETE FROM Users;"
                         + "DBCC CHECKIDENT (Memberships, RESEED, 0);"
                         + "DBCC CHECKIDENT (Messages, RESEED, 0);"
                         + "DBCC CHECKIDENT (Channels, RESEED, 0);"
                         + "DBCC CHECKIDENT (Teams, RESEED, 0);";

            using (SqlConnection connection = AzureServiceBase.GetConnection(TEST_CONNECTION_STRING))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
