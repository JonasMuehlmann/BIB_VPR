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
        public void CreateTeamImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

               uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
               Assert.IsNotNull(teamId);

            }).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Empty names are reserved for private chats
        /// </summary>
        [TestMethod]
        public void CreateTeamImplEmptyName_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

               uint? teamId = await TeamService.CreateTeamImpl("");
               Assert.IsNull(teamId);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GetAllTeams_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
               Assert.IsNotNull(teamId);

                var teams = await TeamService.GetAllTeams();
                Assert.AreEqual(1, Enumerable.Count(teams));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void DeleteTeamImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var teams = await TeamService.GetAllTeams();
                Assert.AreEqual(1, Enumerable.Count(teams));

                bool didDelete = await TeamService.DeleteTeamImpl(teamId.Value);
                Assert.IsTrue(didDelete);

                teams = await TeamService.GetAllTeams();
                Assert.AreEqual(0, Enumerable.Count(teams));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void DeleteTeamImplNonexistent_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teams = await TeamService.GetAllTeams();
                Assert.AreEqual(0, Enumerable.Count(teams));

                bool didDelete = await TeamService.DeleteTeamImpl(1);
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
        public void AddMemberImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didAddMemberImpl = await TeamService.AddMemberImpl(userId, teamId.Value);
                Assert.IsTrue(didAddMemberImpl);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(1, numMembers);

            }).GetAwaiter().GetResult();


        }

        [TestMethod]
        public void AddMemberImplExisting_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didAddMemberImpl = await TeamService.AddMemberImpl(userId, teamId.Value);
                Assert.IsTrue(didAddMemberImpl);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(1, numMembers);

                didAddMemberImpl = await TeamService.AddMemberImpl(userId, teamId.Value);
                Assert.IsFalse(didAddMemberImpl);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMemberImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didAddMemberImpl = await TeamService.AddMemberImpl(userId, teamId.Value);
                Assert.IsTrue(didAddMemberImpl);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(1, numMembers);

                bool didRemoveMemberImpl = await TeamService.RemoveMemberImpl(userId, teamId.Value);
                Assert.IsTrue(didRemoveMemberImpl);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMemberImplNonExistent_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                int numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

                bool didRemoveMemberImpl = await TeamService.RemoveMemberImpl(userId, teamId.Value);
                Assert.IsFalse(didRemoveMemberImpl);

                numMembers = Enumerable.Count(await TeamService.GetAllMembers(teamId.Value));
                Assert.AreEqual(0, numMembers);

            }).GetAwaiter().GetResult();

        }

        [TestMethod]
        public void ChangeTeamNameImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            var newSuffix = "After";

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Before");

                Assert.IsNotNull(teamId);

                var success = await TeamService.ChangeTeamNameImpl(teamId.Value, testName + newSuffix);

                Assert.IsTrue(success);

                var newName = (await TeamService.GetTeam(teamId.Value)).Name;

                Assert.AreEqual(newName, testName + newSuffix);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ChangeTeamDescriptionImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            var newSuffix = "After";

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                uint? teamId = await TeamService.CreateTeamImpl(testName + "Before");
                Assert.IsNotNull(teamId);

                var success = await TeamService.ChangeTeamDescriptionImpl(teamId.Value, testName + newSuffix);

                Assert.IsTrue(success);

                var newDescription = (await TeamService.GetTeam(teamId.Value)).Description;

                Assert.AreEqual(newDescription, testName + newSuffix);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddRoleImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRoleImpl = await TeamService.AddRoleImpl(testName + "Role", teamId.Value, "FFFFFF");
                Assert.IsNotNull(didAddRoleImpl);

                var roles = await TeamService.ListRoles(teamId.Value);
                Assert.AreEqual(1, roles.Where((teamRole) => teamRole.Role == testName + "Role").Count());

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveRoleImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");

                Assert.IsNotNull(teamId);

                var didAddRoleImpl = await TeamService.AddRoleImpl(testName + "Role", teamId.Value, "FFFFFF");

                Assert.IsNotNull(didAddRoleImpl);

                var didRemoveRoleImpl = await TeamService.RemoveRoleImpl(testName + "Role", teamId.Value);

                Assert.IsNotNull(didRemoveRoleImpl);

                var roles = await TeamService.ListRoles(teamId.Value);

                Assert.AreEqual(0, roles.Where((teamRole) => teamRole.Role == testName + "Role").Count());

            }).GetAwaiter().GetResult();
        }

        // TODO: Test assigning non existent role
        [TestMethod]
        public void AssignRoleImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddUserToTeam = await TeamService.AddMemberImpl(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRoleImpl = await TeamService.AddRoleImpl(testName + "Role", teamId.Value, "FFFFFF");

                Assert.IsNotNull(didAddRoleImpl);

                var didAssignRoleImpl = await TeamService.AssignRoleImpl(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didAssignRoleImpl);

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
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddUserToTeam = await TeamService.AddMemberImpl(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRoleImpl = await TeamService.AddRoleImpl(testName + "Role", teamId.Value, "FFFFFF");

                Assert.IsNotNull(didAddRoleImpl);

                var didAssignRoleImpl = await TeamService.AssignRoleImpl(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didAssignRoleImpl);

                var didUnassignRole = await TeamService.UnAssignRoleImpl(testName + "Role", userId, teamId.Value);

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
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddUserToTeam = await TeamService.AddMemberImpl(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var roleId1 = await TeamService.AddRoleImpl(testName + "Role1", teamId.Value, "FFFFFF");

                Assert.IsNotNull(roleId1);

                var roleId2 = await TeamService.AddRoleImpl(testName + "Role2", teamId.Value, "FFFFFF");

                Assert.IsNotNull(roleId2);

                var didAssignRoleImpl = await TeamService.AssignRoleImpl(testName + "Role1", userId, teamId.Value);

                Assert.IsTrue(didAssignRoleImpl);

                didAssignRoleImpl = await TeamService.AssignRoleImpl(testName + "Role2", userId, teamId.Value);

                Assert.IsTrue(didAssignRoleImpl);

                var roles = await TeamService.GetUsersRoles(teamId.Value, userId);

                var testRole1 = new TeamRole(){Id=roleId1.Value, Role=testName + "Role1", TeamId=teamId.Value, Color="FFFFFF"};
                var testRole2 = new TeamRole(){Id=roleId2.Value, Role=testName + "Role2", TeamId=teamId.Value, Color="FFFFFF"};
                Assert.AreEqual($"{testRole1},{testRole2}", string.Join(",", roles));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GrantPermissionImpl_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRoleImpl = await TeamService.AddRoleImpl("admin", teamId.Value, "FFFFFF");
                Assert.IsNotNull(didAddRoleImpl);

                var canAddRoleImpl = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsFalse(canAddRoleImpl);

                var didGrantPermissionImpl = await TeamService.GrantPermissionImpl(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsNotNull(didGrantPermissionImpl);

                var hasPermission = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(hasPermission );

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RevokePermissionImpl_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRoleImpl = await TeamService.AddRoleImpl("admin", teamId.Value, "FFFFFF");
                Assert.IsNotNull(didAddRoleImpl);

                var didGrantPermissionImpl = await TeamService.GrantPermissionImpl(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsNotNull(didGrantPermissionImpl);

                var hasPermission = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(hasPermission);

                var didRevokePermissionImpl = await TeamService.RevokePermissionImpl(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsNotNull(didRevokePermissionImpl);

                hasPermission = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsFalse(hasPermission);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ListPermissionsOfRole_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;
                Assert.IsNotNull(userId);

                var teamId = await TeamService.CreateTeamImpl(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRoleImpl = await TeamService.AddRoleImpl("admin", teamId.Value, "FFFFFF");
                Assert.IsNotNull(didAddRoleImpl);

                var canAddRoleImpl = await TeamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsFalse(canAddRoleImpl);

                var didGrantPermissionImpl = await TeamService.GrantPermissionImpl(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsNotNull(didGrantPermissionImpl);

                var permissions = await TeamService.GetPermissionsOfRole(teamId.Value, "admin");
                Assert.AreEqual(1, permissions.Count());
                Assert.AreEqual(Permissions.CanAddRole, permissions[0]);

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }

    }
}
