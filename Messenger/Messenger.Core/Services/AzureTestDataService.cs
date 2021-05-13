using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Core.Services
{
    public class SampleTeam
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamDescription { get; set; }
        public DateTime CreationDate { get; set; }
    }

    public class AzureTestDataService : AzureServiceBase
    {
        /// <summary>
        /// Gets list of all teams
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<SampleTeam>> GetAllTeams()
        {
            const string query = @"SELECT * FROM Teams";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataSet dataSet = new DataSet();

                    adapter.Fill(dataSet, "Teams");

                    List<SampleTeam> teams = new List<SampleTeam>();
                    foreach (DataRow row in dataSet.Tables["Teams"].Rows)
                    {
                        teams.Add(new SampleTeam()
                        {
                            TeamId = Convert.ToInt32(row["TeamId"]),
                            TeamName = row["TeamName"].ToString(),
                            TeamDescription = row["TeamDescription"].ToString(),
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
        /// Deletes team with given id
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns>1(Success)/0(Fail)</returns>
        public async Task<int> DeleteTeam(int teamId)
        {
            string query = @"DELETE FROM Teams WHERE TeamId=" + teamId;

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlCommand command = new SqlCommand(query, connection);

                    // Returns 1 if successful
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return 0;
            }
        }
    }
}
