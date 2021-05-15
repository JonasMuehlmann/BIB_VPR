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
        public async Task<bool> CreateTeam(string teamName, string teamDescription = "")
        {
            if (teamName == string.Empty) return false;

            string query = $"INSERT INTO Teams (TeamName, TeamDescription, CreationDate) VALUES " +
                $"('{teamName}', '{teamDescription}', GETDATE());";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        public async Task<bool> DeleteTeam(int teamId)
        {
            string query = $"DELETE FROM Teams WHERE TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

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
    }
}
