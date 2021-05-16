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

        public async Task<bool> AddMember(string userId, int teamId)
        {
            string query = $"INSERT INTO Memberships(UserId, TeamId, UserRole) VALUES('{userId}', {teamId}, 'placeholder');";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        public async Task<bool> RemoveMember(string userId, int teamId)
        {
            string query = $"DELETE FROM Memberships WHERE UserId='{userId}' AND TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

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
