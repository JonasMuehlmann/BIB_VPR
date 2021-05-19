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
        /// Creates a team with the given name and description
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> CreateTeam(string teamName, string teamDescription = "")
        {
            if (teamName == string.Empty) return false;

            string query = $"INSERT INTO Teams (TeamName, TeamDescription, CreationDate) VALUES " +
                $"('{teamName}', '{teamDescription}', GETDATE());";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Deletes a team with a given team id.
        /// </summary>
        /// <param name="teamId">The id of the team to delete</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> DeleteTeam(string teamId)
        {
            string query = $"DELETE FROM Teams WHERE TeamId='{teamId}';";

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
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Teams");

                    return dataSet.Tables["Teams"].Rows.Cast<DataRow>().Select(Mapper.TeamFromDataRow);
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
        public async Task<bool> AddMember(string userId, string teamId)
        {
            string query = $"INSERT INTO Memberships(UserId, TeamId, UserRole) VALUES('{userId}', '{teamId}', 'placeholder');";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Removes a member from the team
        /// </summary>
        /// <param name="userId">The id of the user to remove from the specified team</param>
        /// <param name="teamId">The id of the team to remove the specified user from</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> RemoveMember(string userId, string teamId)
        {
            string query = $"DELETE FROM Memberships WHERE UserId='{userId}' AND TeamId='{teamId}';";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Gets all members in the team
        /// </summary>
        /// <param name="teamId">The id of the team to get members of</param>
        /// <returns>An enumerable of User objects</returns>
        public async Task<IEnumerable<User>> GetAllMembers(string teamId)
        {
            string subquery = $"SELECT UserId FROM Memberships WHERE TeamId='{teamId}'";
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
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return null;
            }
        }

        #endregion
    }
}
