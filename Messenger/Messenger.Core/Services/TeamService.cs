using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Core.Services
{
    public class TeamService : AzureServiceBase
    {
        #region Teams Management

        /// <summary>
        /// Creates a team with the given name and description and retrieve the sent messages id.
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>The id of the created team if it was created successfully, null otherwise</returns>
        public async Task<int?> CreateTeam(string teamName, string teamDescription = "")
        {
            if (teamName == string.Empty)
            {
                return null;
            }

            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();
                string query = $"INSERT INTO Teams (TeamName, TeamDescription, CreationDate) VALUES " +
                $"('{teamName}', '{teamDescription}', GETDATE()); SELECT SCOPE_IDENTITY();";


                SqlCommand scalarQuery = new SqlCommand(query, connection);

                return Convert.ToInt32(scalarQuery.ExecuteScalar());
            }
        }

        /// <summary>
        /// Deletes a team with a given team id.
        /// </summary>
        /// <param name="teamId">The id of the team to delete</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> DeleteTeam(int teamId)
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

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    return SqlHelpers.GetRows("Teams", adapter).Select(Mapper.TeamFromDataRow);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return null;
            }
        }
        /// <summary>
        /// Returns a list of teams a specified user is a member of.
        /// </summary>
        /// <param name="userId">The id of the user whose teams to list</param>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllTeams(string userId)
        {
            string query = $"SELECT TeamId, TeamName, TeamDescription, CreationDate FROM Teams t LEFT JOIN Memberships m ON (t.TeamId = m.TeamId) WHERE m.UserId = '{userId}';";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    return SqlHelpers.GetRows("Teams",adapter).Select(Mapper.TeamFromDataRow);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
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
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> AddMember(string userId, int teamId)
        {
            string query = $"INSERT INTO Memberships(UserId, TeamId, UserRole) VALUES('{userId}', {teamId}, 'placeholder');";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Removes a member from the team
        /// </summary>
        /// <param name="userId">The id of the user to remove from the specified team</param>
        /// <param name="teamId">The id of the team to remove the specified user from</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> RemoveMember(string userId, int teamId)
        {
            string query = $"DELETE FROM Memberships WHERE UserId='{userId}' AND TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Gets all members in the team
        /// </summary>
        /// <param name="teamId">The id of the team to get members of</param>
        /// <returns>An enumerable of User objects</returns>
        public async Task<IEnumerable<User>> GetAllMembers(int teamId)
        {
            string subquery = $"SELECT UserId FROM Memberships WHERE TeamId={teamId}";
            string query = $"SELECT * FROM Users WHERE UserId IN ({subquery})";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    return SqlHelpers.GetRows("Users", adapter).Select(Mapper.UserFromDataRow);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return null;
            }
        }

        #endregion
    }
}
