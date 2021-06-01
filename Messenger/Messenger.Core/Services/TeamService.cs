using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Messenger.Core.Services
{
    public class TeamService : AzureServiceBase
    {
        #region Teams Management

        /// <summary>
        /// Creates a team with the given name and description and retrieve the new team's id.
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>The id of the created team if it was created successfully, null otherwise</returns>
        public async Task<uint?> CreateTeam(string teamName, string teamDescription = "")
        {
            string query = $"INSERT INTO Teams (TeamName, TeamDescription, CreationDate) VALUES " +
                $"('{teamName}', '{teamDescription}', GETDATE()); SELECT SCOPE_IDENTITY();";

            if (teamName == string.Empty)
            {
                return null;
            }

            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                SqlCommand scalarQuery = new SqlCommand(query, connection);

                return Convert.ToUInt32(scalarQuery.ExecuteScalar());
            }
        }

        /// <summary>
        /// Deletes a team with a given team id.
        /// </summary>
        /// <param name="teamId">The id of the team to delete</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at leasat one query, false otherwise</returns>
        public async Task<bool> DeleteTeam(uint teamId)
        {
            string query = $"DELETE FROM Teams WHERE TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Returns a list of teams.
        /// </summary>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllTeams()
        {
            string query = @"SELECT TeamId, TeamName, TeamDescription, CreationDate FROM Teams;";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    return SqlHelpers
                        .MapToList(Mapper.TeamFromDataRow, new SqlDataAdapter(query, connection));
                }
            }
            catch (SqlException e)
            {
                HandleException(e);
                return null;
            }
        }

        /// <summary>
        /// Returns a list of teams a specified user is a member of.
        /// </summary>
        /// <param name="userId">The id of the user whose teams to list</param>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllTeamsByUserId(string userId)
        {
            string query = $"SELECT t.TeamId, TeamName, TeamDescription, CreationDate FROM Teams t LEFT JOIN Memberships m ON (t.TeamId = m.TeamId) WHERE m.UserId = '{userId}';";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    return SqlHelpers
                        .MapToList(Mapper.TeamFromDataRow, new SqlDataAdapter(query, connection));
                }
            }
            catch (SqlException e)
            {
                HandleException(e);
                return null;
            }
        }

        #endregion

        #region Members Management

        /// <summary>
        /// Adds a new member to the team
        /// </summary>
        /// <param name="userId">The id of the user to add to the specified team</param>
        /// <param name="teamId">The id of the team to add the specified user to</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public async Task<bool> AddMember(string userId, uint teamId)
        {
            string query = $"INSERT INTO Memberships(UserId, TeamId, UserRole) VALUES('{userId}', '{teamId}', 'placeholder');";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Removes a member from the team
        /// </summary>
        /// <param name="userId">The id of the user to remove from the specified team</param>
        /// <param name="teamId">The id of the team to remove the specified user from</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public async Task<bool> RemoveMember(string userId, uint teamId)
        {
            string query = $"DELETE FROM Memberships WHERE UserId='{userId}' AND TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Gets all members in the team
        /// </summary>
        /// <param name="teamId">The id of the team to get members of</param>
        /// <returns>An enumerable of User objects</returns>
        public async Task<IEnumerable<User>> GetAllUsersByTeamId(uint teamId)
        {
            string subquery = $"SELECT UserId FROM Memberships WHERE TeamId={teamId}";
            string query = $"SELECT * FROM Users WHERE UserId IN ({subquery})";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    return SqlHelpers
                        .MapToList(Mapper.UserFromDataRow, new SqlDataAdapter(query, connection));
                }
            }
            catch (SqlException e)
            {
                HandleException(e);
                return null;
            }
        }

        /// <summary>
        /// Gets all memberships of a user
        /// </summary>
        /// <param name="userId">User id for the current user</param>
        /// <returns>A list of membership objects</returns>
        public async Task<IList<Membership>> GetAllMembershipByUserId(string userId)
        {
            string query = $"SELECT * FROM Memberships WHERE UserId='{userId}'";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    return SqlHelpers
                        .MapToList(Mapper.MembershipFromDataRow, new SqlDataAdapter(query, connection));
                }
            }
            catch (SqlException e)
            {
                HandleException(e);
                return null;
            }
        }

        public async Task<IEnumerable<User>> GetAllMembers(uint teamId)
        {
            string subquery = $"SELECT UserId FROM Memberships WHERE TeamId={teamId}";
            string query = $"SELECT * FROM Users WHERE UserId IN ({subquery})";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Users");

                    return dataSet.Tables["Users"].Rows.Cast<DataRow>().Select(Mapper.UserFromDataRow);
                }
            }
            catch (SqlException e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return null;
            }
        }
        #endregion
    }
}
