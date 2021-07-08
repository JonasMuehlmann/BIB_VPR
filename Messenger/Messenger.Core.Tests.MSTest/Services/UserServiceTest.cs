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
        /// <summary>
        /// Should fetch the existing user from database
        /// </summary>
        [TestMethod]
        public void GetOrCreateApplicationUserExisting_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var data = new User(){Id=testName+"UserId", DisplayName=testName+"UserName"};

                User retrievedUser = await UserService.GetOrCreateApplicationUser(data);
                Assert.AreEqual(testName + "UserId", retrievedUser.Id);

                retrievedUser = await UserService.GetOrCreateApplicationUser(data);
                Assert.AreEqual(testName + "UserId", retrievedUser.Id);

            }).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Should fetch the existing user from database
        /// </summary>
        [TestMethod]
        public void GetOrCreateApplicationNonExisting_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var data = new User(){Id=testName+"UserId", DisplayName=testName+"UserName"};

                User retrievedUser = await UserService.GetOrCreateApplicationUser(data);
                Assert.AreEqual(testName + "UserId", retrievedUser.Id);

            }).GetAwaiter().GetResult();

        }


        /// <summary>
        /// Should fetch the existing user from database
        /// </summary>
        [TestMethod]
        public void GetOrCreateApplicationUserSecondNameId_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var user1 = new User(){Id=testName+"UserId1", DisplayName=testName+"UserName"};
                var user2 = new User(){Id=testName+"UserId2", DisplayName=testName+"UserName"};

                User retrievedUser = await UserService.GetOrCreateApplicationUser(user1);
                Assert.AreEqual(1u, retrievedUser.NameId);

                retrievedUser = await UserService.GetOrCreateApplicationUser(user2);
                Assert.AreEqual(2u, retrievedUser.NameId);

            }).GetAwaiter().GetResult();
        }


        /// <summary>
        /// Should update username, expects true
        /// </summary>
        [TestMethod]
        public void UpdateUsername_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var user = new User(){Id=testName+"UserId", DisplayName=testName+"UserName"};

                User retrievedUser = await UserService.GetOrCreateApplicationUser(user);
                Assert.AreEqual(testName + "UserId", retrievedUser.Id);

                bool didRename = await UserService.UpdateUsername(testName + "UserId", testName + "UserNameAfter");
                Assert.IsTrue(didRename);

                User userAfter = await UserService.GetUser(testName + "UserId");
                Assert.AreEqual(testName + "UserNameAfter", userAfter.DisplayName);

            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should update user bio, expects true
        /// </summary>
        [TestMethod]
        public void UpdateUserInfo_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var user = new User(){Id=testName+"UserId", DisplayName=testName+"UserName"};
                User retrievedUser = await UserService.GetOrCreateApplicationUser(user);
                Assert.AreEqual(testName + "UserId", retrievedUser.Id);

                bool didChangeBio = await UserService.Update(testName + "UserId", "Bio", testName + "BioAfter");
                Assert.IsTrue(didChangeBio);

                User userAfter = await UserService.GetUser(testName + "UserId");
                Assert.AreEqual(testName + "BioAfter", userAfter.Bio);

            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should delete user from the setup, expects true
        /// </summary>
        [TestMethod]
        public void DeleteUser_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var user = new User(){Id=testName+"UserId", DisplayName=testName+"UserName"};
                User retrievedUser = await UserService.GetOrCreateApplicationUser(user);
                Assert.AreEqual(testName + "UserId", retrievedUser.Id);

                bool didDelete = await UserService.DeleteUser(testName + "UserId");
                Assert.IsTrue(didDelete);

                Assert.IsNull(await UserService.GetUser(testName + "UserId"));

            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Should delete user from the setup, expects true
        /// </summary>
        [TestMethod]
        public void DeleteNonExistentUser_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string id = testName + "UserId";
                bool  didDelete = await UserService.DeleteUser(testName + "UserId");

                Assert.IsFalse(didDelete);

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
                    Assert.IsNotNull(await UserService.GetOrCreateApplicationUser(user));
                }

                var userMatches = await UserService.SearchUser(testName);
                Assert.IsNotNull(userMatches);

                Assert.AreEqual(userMatchString, string.Join(",", userMatches));

            }).GetAwaiter().GetResult();

        }


        [TestMethod]
        public void ChangeBio_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var userId = testName + "UserId";

                var user = await UserService.GetOrCreateApplicationUser(new User(){Id = userId,DisplayName = testName + "UserName", Bio=testName + "Bio"});
                Assert.IsNotNull(user);

                string oldBio = user.Bio;
                Assert.AreEqual(oldBio, testName + "Bio");

                var success = await UserService.UpdateUserBio(userId, oldBio + "New");
                Assert.IsTrue(success);

                user = await UserService.GetOrCreateApplicationUser(new User(){Id = userId});
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

                var user = await UserService.GetOrCreateApplicationUser(new User(){Id = userId,DisplayName = testName + "UserName", Mail=testName + "Mail"});
                Assert.IsNotNull(user);

                string oldEmail = user.Mail;
                Assert.AreEqual(oldEmail, testName + "Mail");

                var success = await UserService.UpdateUserMail(userId, oldEmail + "New");
                Assert.IsTrue(success);

                user = await UserService.GetOrCreateApplicationUser(new User(){Id = userId});
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

                var user = await UserService.GetOrCreateApplicationUser(new User(){Id = userId,DisplayName = testName + "UserName", Photo=testName + "Photo"});
                Assert.IsNotNull(user);

                string oldEmail = user.Photo;
                Assert.AreEqual(oldEmail, testName + "Photo");

                var success = await UserService.UpdateUserPhoto(userId, oldEmail + "New");
                Assert.IsTrue(success);

                user = await UserService.GetOrCreateApplicationUser(new User(){Id = userId});
                Assert.IsNotNull(user);

                string newPhoto = user.Photo;

                Assert.AreEqual(oldEmail + "New", newPhoto);

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }
    }
}
