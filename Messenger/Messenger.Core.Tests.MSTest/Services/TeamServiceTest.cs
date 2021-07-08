using System.Threading.Tasks;
using System.Linq;
using System;
using System.Data.SqlClient;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Core.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.TeamService
    /// </summary>
    [TestClass]
    public class TeamServiceTest
    {
        [TestMethod]
        public void CreateTeam_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
               uint? teamId = await TeamService.CreateTeam(testName + "Team");
               Assert.IsNotNull(teamId);

            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Empty names are reserved for private chats
        /// </summary>
        [TestMethod]
        public void CreateTeamEmptyName_Test()
        {
            Task.Run(async () =>
            {
               uint? teamId = await TeamService.CreateTeam("");
               Assert.IsNull(teamId);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GetAllTeams_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
               Assert.IsNotNull(teamId);

                var teams = await TeamService.GetAllTeams();
                Assert.AreEqual(1, Enumerable.Count(teams));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void DeleteTeam_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var teams = await TeamService.GetAllTeams();
                Assert.AreEqual(1, Enumerable.Count(teams));

                bool didDelete = await TeamService.DeleteTeam(teamId.Value);
                Assert.IsTrue(didDelete);

                teams = await TeamService.GetAllTeams();
                Assert.AreEqual(0, Enumerable.Count(teams));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void DeleteTeamNonexistent_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teams = await TeamService.GetAllTeams();
                Assert.AreEqual(0, Enumerable.Count(teams));

                bool didDelete = await TeamService.DeleteTeam(1);
                Assert.IsFalse(didDelete);

            }).GetAwaiter().GetResult();
        }


        [TestMethod]
        public void GetAllTeamsNoneExist_Test()
        {
            Task.Run(async () =>
            {
                var teams = await TeamService.GetAllTeams();
                Assert.AreEqual(0, Enumerable.Count(teams));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddMember_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didAddMember = await TeamService.AddMember(userId, teamId.Value);
                Assert.IsTrue(didAddMember);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(1, numMembers);

            }).GetAwaiter().GetResult();


        }

        [TestMethod]
        public void AddMemberExisting_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didAddMember = await TeamService.AddMember(userId, teamId.Value);
                Assert.IsTrue(didAddMember);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(1, numMembers);

                didAddMember = await TeamService.AddMember(userId, teamId.Value);
                Assert.IsFalse(didAddMember);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMember_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didAddMember = await TeamService.AddMember(userId, teamId.Value);
                Assert.IsTrue(didAddMember);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(1, numMembers);

                bool didRemoveMember = await TeamService.RemoveMember(userId, teamId.Value);
                Assert.IsTrue(didRemoveMember);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMemberNonExistent_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didRemoveMember = await TeamService.RemoveMember(userId, teamId.Value);
                Assert.IsFalse(didRemoveMember);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

            }).GetAwaiter().GetResult();

        }

        [TestMethod]
        public void ChangeTeamName_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            var newSuffix = "After";

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Before");

                Assert.IsNotNull(teamId);

                var success = await TeamService.ChangeTeamName(teamId.Value, testName + newSuffix);

                Assert.IsTrue(success);

                var newName = (await TeamService.GetTeam(teamId.Value)).Name;

                Assert.AreEqual(newName, testName + newSuffix);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ChangeTeamDescription_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            var newSuffix = "After";

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Before");

                Assert.IsNotNull(teamId);

                var success = await TeamService.ChangeTeamDescription(teamId.Value, testName + newSuffix);

                Assert.IsTrue(success);

                var newDescription = (await TeamService.GetTeam(teamId.Value)).Description;

                Assert.AreEqual(newDescription, testName + newSuffix);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddRole_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var didAddRole = await TeamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var roles = await TeamService.ListRoles(teamId.Value);

                Assert.IsTrue(roles.Contains(testName + "Role"));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveRole_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var didAddRole = await TeamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didRemoveRole = await TeamService.RemoveRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didRemoveRole);

                var roles = await TeamService.ListRoles(teamId.Value);

                Assert.IsFalse(roles.Contains(testName + "Role"));

            }).GetAwaiter().GetResult();
        }

        // TODO: Test assigning non existent role
        [TestMethod]
        public void AssignRole_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                Assert.IsNotNull(userId);

                var didAddUserToTeam = await TeamService.AddMember(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRole = await TeamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didAssignRole = await TeamService.AssignRole(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                var roles = await TeamService.GetUsersWithRole(teamId.Value, testName + "Role");

                Assert.AreEqual(1, roles.Count);
                Assert.AreEqual(userId, roles[0].Id);

            }).GetAwaiter().GetResult();
        }

        // TODO: Test unassigning non existent role
        [TestMethod]
        public void UnassignRole_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                Assert.IsNotNull(userId);

                var didAddUserToTeam = await TeamService.AddMember(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRole = await TeamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didAssignRole = await TeamService.AssignRole(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                var didUnassignRole = await TeamService.UnAssignRole(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didUnassignRole);

                var roles = await TeamService.GetUsersWithRole(teamId.Value, testName + "Role");

                Assert.AreEqual(0, roles.Count);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GetUsersRoles_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                Assert.IsNotNull(userId);

                var didAddUserToTeam = await TeamService.AddMember(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRole = await TeamService.AddRole(testName + "Role1", teamId.Value);

                Assert.IsTrue(didAddRole);

                didAddRole = await TeamService.AddRole(testName + "Role2", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didAssignRole = await TeamService.AssignRole(testName + "Role1", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                didAssignRole = await TeamService.AssignRole(testName + "Role2", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                var roles = await TeamService.GetUsersRoles(teamId.Value, userId);

                Assert.AreEqual($"{testName}Role1,{testName}Role2", string.Join(",", roles));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GrantPermission_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRole = await TeamService.AddRole("admin", teamId.Value);
                Assert.IsTrue(didAddRole);

                var canAddRole = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsFalse(canAddRole);

                var didGrantPermission = await TeamService.GrantPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(didGrantPermission);

                var hasPermission = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(hasPermission );

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RevokePermission_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRole = await TeamService.AddRole("admin", teamId.Value);
                Assert.IsTrue(didAddRole);

                var didGrantPermission = await TeamService.GrantPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(didGrantPermission);

                var hasPermission = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(hasPermission);

                var didRevokePermission = await TeamService.RevokePermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(didRevokePermission);

                hasPermission = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsFalse(hasPermission);

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }

    }
}
