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
                Debug.WriteLine($"Database Exception: {e.Message}");
                return null;
            }
        }
    }
}
