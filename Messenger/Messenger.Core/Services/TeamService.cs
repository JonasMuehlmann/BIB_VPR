using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Core.Services
{
    public class TeamService : AzureServiceBase
    {
        /// <summary>
        /// Create a team with a given name and description.
        /// </summary>
        /// <param name="teamName">A name to give the team</param>
        /// <param name="teamDescription">A description to set for the team, defaults to an empty string</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> CreateTeam(string teamName, string teamDescription = "")
        {
            if (teamName == string.Empty) return false;

            string query = $"INSERT INTO Teams (TeamName, TeamDescription, CreationDate) VALUES " +
                $"('{teamName}', '{teamDescription}', GETDATE());";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Delete a team witha given team id.
        /// </summary>
        /// <param name="teamId">The id of the team to delete</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> DeleteTeam(int teamId)
        {
            string query = $"DELETE FROM Teams WHERE TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Return a list of teams.
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

                    List<Team> teams = new List<Team>();
                    foreach (DataRow row in dataSet.Tables["Teams"].Rows)
                    {
                        teams.Add(new Team()
                        {
                            Id = Convert.ToInt32(row["TeamId"]),
                            Description = row["TeamDescription"].ToString(),
                            Name = row["TeamName"].ToString(),
                            CreationDate = Convert.ToDateTime(row["CreationDate"].ToString())
                        });
                    }

                    return teams;
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Add a member with a specified user id to a team with a specified team id.
        /// </summary>
        /// <param name="userId">The id of the user to add to the specified team</param>
        /// <param name="teamid">The id of the team to add the specified user to</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> AddMember(string userId,int teamId)
        {
            string query = $"INSERT INTO Memberships(UserId, TeamId, UserRole) VALUES('{userId}', {teamId}, 'placeholder');";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Remove a member with a specified user id from a team with a specified team id.
        /// </summary>
        /// <param name="userid">The id of the user to remove from the specified team</param>
        /// <param name="teamId">The id of the team to remove the specified user from</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> RemoveMember(string userId,int teamId)
        {
            string query = $"DELETE FROM Memberships WHERE UserId='{userId}' AND TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

    }
}
