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
        TeamService teamService;
        UserService userService;

        /// <summary>
        /// Initialize the service
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            teamService = new TeamService();
            userService = new UserService();
        }

        [TestMethod]
        public void CreateTeam_Test()
        {
            Task.Run(async () =>
            {
               uint? teamId = await teamService.CreateTeam("MyExampleTeam");

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
               uint? teamId = await teamService.CreateTeam("");

               Assert.IsNull(teamId);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GetAllTeams_Test()
        {
            Task.Run(async () =>
            {
                var teams = await teamService.GetAllTeams();

                Assert.IsTrue(Enumerable.Count(teams) > 0);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void DeleteTeam_Test()
        {
            Task.Run(async () =>
            {
                using (SqlConnection connection = AzureServiceBase.GetDefaultConnection())
                {
                    string query = "SET IDENTITY_INSERT Teams ON;INSERT INTO Teams(TeamId, TeamName, TeamDescription, CreationDate) Values(9999999, 'foo', 'desc', GETDATE());";

                    connection.Open();
                    SqlCommand cmd = new SqlCommand(query, connection);
                    bool result = Convert.ToBoolean(cmd.ExecuteNonQuery());
                }

                // FIX: Tests like this one depend on other tests having run before it
                bool success = await teamService.DeleteTeam(9999999u);


                Assert.IsTrue(success);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void DeleteTeamNonexistent_Test()
        {
            Task.Run(async () =>
            {
                // FIX: Tests like this one depend on other tests having run before it
                bool success = await teamService.DeleteTeam(9999999u);

                Assert.IsFalse(success);

            }).GetAwaiter().GetResult();
        }


        [TestMethod]
        public void GetAllTeamsNoneExist_Test()
        {
            Task.Run(async () =>
            {

                using (SqlConnection connection = TeamService.GetDefaultConnection())
                {
                    await connection.OpenAsync();

                    string query = "DELETE FROM Reactions;"
                                 + "DELETE FROM Notifications;"
                                 + "DELETE FROM Messages;"
                                 + "DELETE FROM Memberships;"
                                 + "DELETE FROM Channels;"
                                 + "DELETE FROM Role_permissions;"
                                 + "DELETE FROM User_roles;"
                                 + "DELETE FROM Team_roles;"
                                 + "DELETE FROM Teams;"
                                 + "DELETE FROM Users;";


                    await SqlHelpers.NonQueryAsync(query);
                }

                var teams = await teamService.GetAllTeams();

                Assert.AreEqual(Enumerable.Count(teams), 0);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddMember_Test()
        {
            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam("abc");
                Assert.IsNotNull(teamId);

                string userId = (await userService.GetOrCreateApplicationUser(
                            new User(){Id = "myTestUserId"}
                            )).Id;

                int numMembersBefore = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                bool success = await teamService.AddMember(userId, teamId.Value);
                Assert.IsTrue(success);

                int numMembersAfter = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                Assert.IsTrue(numMembersBefore < numMembersAfter);

            }).GetAwaiter().GetResult();


        }

        [TestMethod]
        public void AddMemberExisting_Test()
        {
            Task.Run(async () =>
            {
                uint? teamId;

                using (SqlConnection connection = TeamService.GetDefaultConnection())
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("SELECT TeamId FROM teams WHERE TeamName='abc';", connection);
                    teamId = Convert.ToUInt32(cmd.ExecuteScalar());
                }

                Assert.IsNotNull(teamId);

                string userId = (await userService.GetOrCreateApplicationUser(
                            new User(){Id = "myTestUserId"}
                            )).Id;

                int numMembersBefore = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                bool success = await teamService.AddMember(userId, teamId.Value);
                Assert.IsTrue(success);

                int numMembersAfter = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                Assert.AreEqual(numMembersBefore, numMembersAfter);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMember_Test()
        {
            Task.Run(async () =>
            {
                uint? teamId;

                using (SqlConnection connection = TeamService.GetDefaultConnection())
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("SELECT TeamId FROM teams WHERE TeamName='abc';", connection);
                    teamId = Convert.ToUInt32(cmd.ExecuteScalar());
                }

                Assert.IsNotNull(teamId);

                int numMembersBefore = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                bool success = await teamService.RemoveMember("myTestUserId", teamId.Value);
                Assert.IsTrue(success);

                int numMembersAfter = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                Assert.AreEqual(numMembersAfter + 1, numMembersBefore);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMemberNonExistent_Test()
        {
            Task.Run(async () =>
            {
                uint? teamId;

                using (SqlConnection connection = TeamService.GetDefaultConnection())
                {
                    connection.Open();

                    SqlCommand cmd = new SqlCommand("SELECT TeamId FROM teams WHERE TeamName='abc';", connection);
                    teamId = Convert.ToUInt32(cmd.ExecuteScalar());
                }

                Assert.IsNotNull(teamId);

                int numMembersBefore = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                bool success = await teamService.RemoveMember("myTestUserId", teamId.Value);
                Assert.IsFalse(success);

                int numMembersAfter = Enumerable.Count(await teamService.GetAllMembers(teamId.Value));

                Assert.AreEqual(numMembersAfter, numMembersBefore);

            }).GetAwaiter().GetResult();

        }

        [TestMethod]
        public void ChangeTeamName_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            var newSuffix = "After";

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Before");

                Assert.IsNotNull(teamId);

                var success = await teamService.ChangeTeamName(teamId.Value, testName + newSuffix);

                Assert.IsTrue(success);

                var newName = (await teamService.GetTeam(teamId.Value)).Name;

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
                uint? teamId = await teamService.CreateTeam(testName + "Before");

                Assert.IsNotNull(teamId);

                var success = await teamService.ChangeTeamDescription(teamId.Value, testName + newSuffix);

                Assert.IsTrue(success);

                var newDescription = (await teamService.GetTeam(teamId.Value)).Description;

                Assert.AreEqual(newDescription, testName + newSuffix);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddRole_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var didAddRole = await teamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var roles = await teamService.ListRoles(teamId.Value);

                Assert.IsTrue(roles.Contains(testName + "Role"));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveRole_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var didAddRole = await teamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didRemoveRole = await teamService.RemoveRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didRemoveRole);

                var roles = await teamService.ListRoles(teamId.Value);

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
                var teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var userId = (await userService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                Assert.IsNotNull(userId);

                var didAddUserToTeam = await teamService.AddMember(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRole = await teamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didAssignRole = await teamService.AssignRole(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                var roles = await teamService.GetUsersWithRole(teamId.Value, testName + "Role");

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
                var teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var userId = (await userService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                Assert.IsNotNull(userId);

                var didAddUserToTeam = await teamService.AddMember(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRole = await teamService.AddRole(testName + "Role", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didAssignRole = await teamService.AssignRole(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                var didUnassignRole = await teamService.UnAssignRole(testName + "Role", userId, teamId.Value);

                Assert.IsTrue(didUnassignRole);

                var roles = await teamService.GetUsersWithRole(teamId.Value, testName + "Role");

                Assert.AreEqual(0, roles.Count);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GetUsersRoles_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var userId = (await userService.GetOrCreateApplicationUser(new User(){Id=testName + "User"})).Id;

                Assert.IsNotNull(userId);

                var didAddUserToTeam = await teamService.AddMember(userId, teamId.Value);

                Assert.IsTrue(didAddUserToTeam);

                var didAddRole = await teamService.AddRole(testName + "Role1", teamId.Value);

                Assert.IsTrue(didAddRole);

                didAddRole = await teamService.AddRole(testName + "Role2", teamId.Value);

                Assert.IsTrue(didAddRole);

                var didAssignRole = await teamService.AssignRole(testName + "Role1", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                didAssignRole = await teamService.AssignRole(testName + "Role2", userId, teamId.Value);

                Assert.IsTrue(didAssignRole);

                var roles = await teamService.GetUsersRoles(teamId.Value, userId);

                Assert.AreEqual($"{testName}Role1,{testName}Role2", string.Join(",", roles));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void GrantPermission_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRole = await teamService.AddRole("admin", teamId.Value);
                Assert.IsTrue(didAddRole);

                var canAddRole = await teamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsFalse(canAddRole);

                var didGrantPermission = await teamService.GrantPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(didGrantPermission);

                var hasPermission = await teamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(hasPermission );

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RevokePermission_Test()
        {

            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var teamId = await teamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var didAddRole = await teamService.AddRole("admin", teamId.Value);
                Assert.IsTrue(didAddRole);

                var didGrantPermission = await teamService.GrantPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(didGrantPermission);

                var hasPermission = await teamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(hasPermission);

                var didRevokePermission = await teamService.RevokePermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsTrue(didRevokePermission);

                hasPermission = await teamService.HasPermission(teamId.Value, "admin", Permissions.CanAddRole);
                Assert.IsFalse(hasPermission);

            }).GetAwaiter().GetResult();
        }

    }
}
