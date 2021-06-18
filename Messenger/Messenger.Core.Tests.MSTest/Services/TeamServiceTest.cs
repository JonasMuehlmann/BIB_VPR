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
    public class TeamServiceTest : SqlServiceTestBase
    {
        TeamService teamService;
        UserService userService;

        /// <summary>
        /// Initialize the service
        /// </summary>
        [TestInitialize]
        public void Initialize()
        {
            teamService = InitializeTestMode<TeamService>();
            userService = InitializeTestMode<UserService>();
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
                using (SqlConnection connection = AzureServiceBase.GetConnection(TEST_CONNECTION_STRING))
                {
                    string query = "SET IDENTITY_INSERT Teams ON;INSERT INTO Teams(TeamId, TeamName, TeamDescription, CreationDate, Roles) Values(9999999, 'foo', 'desc', GETDATE(), '');";

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

                using (SqlConnection connection = teamService.GetConnection())
                {
                    await connection.OpenAsync();

                    string query = "DELETE FROM messages; DELETE FROM memberships;DELETE FROM channels; DELETE FROM teams;";

                    await SqlHelpers.NonQueryAsync(query, connection);
                }

                var teams = await teamService.GetAllTeams();

                Assert.IsTrue(Enumerable.Count(teams) == 0);

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

                using (SqlConnection connection = teamService.GetConnection())
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

                Assert.IsTrue(numMembersBefore == numMembersAfter);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMember_Test()
        {
            Task.Run(async () =>
            {
                uint? teamId;

                using (SqlConnection connection = teamService.GetConnection())
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

                Assert.IsTrue(numMembersAfter + 1 == numMembersBefore);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMemberNonExistent_Test()
        {
            Task.Run(async () =>
            {
                uint? teamId;

                using (SqlConnection connection = teamService.GetConnection())
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

                Assert.IsTrue(numMembersAfter == numMembersBefore);

            }).GetAwaiter().GetResult();

        }
    }
}
