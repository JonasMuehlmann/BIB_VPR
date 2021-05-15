using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;

/* This data should be deleted before Release */
namespace Messenger.Core.Services
{
    /// <summary>
    /// Sample class to show database mapping
    /// </summary>
    public class SampleTeam
    {
        public int TeamId { get; set; }
        public string TeamName { get; set; }
        public string TeamDescription { get; set; }
        public DateTime CreationDate { get; set; }
    }

    /// <summary>
    /// Sample service of CRUD operations(Team)
    /// </summary>
    public class AzureTestDataService : AzureServiceBase
    {
        public async Task<int> CreateTeam(string teamName, string teamDescription)
        {
            // 1. Create query
            string query = @"INSERT INTO Teams (TeamName, TeamDescription, CreationDate) "
                + @"VALUES (@TeamName, @TeamDescription, @CreationDate)";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    // 2. Open connection
                    await connection.OpenAsync();

                    // 3. Create command
                    SqlCommand command = new SqlCommand(query, connection);

                    // 4. Replace parameters with values(reference the database for the right datatypes)
                    command.Parameters.Add("@TeamName", SqlDbType.NVarChar, 64).Value = teamName;
                    command.Parameters.Add("@TeamDescription", SqlDbType.NVarChar, 64).Value = teamDescription;
                    command.Parameters.Add("@CreationDate", SqlDbType.Date).Value = DateTime.Now;

                    // 5. Execute non query(returns 1 on success)
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                // *. Error handling
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Gets list of all teams
        /// </summary>
        /// <returns>Array/List of teams</returns>
        public async Task<IEnumerable<SampleTeam>> GetAllTeams()
        {
            // 1. Create query
            const string query = @"SELECT * FROM Teams";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    // 2. Open connection
                    await connection.OpenAsync();

                    // 3. Create adapter
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    // 4. Fill into dataset
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Teams");

                    // 5. Destructure dataset
                    List<SampleTeam> teams = new List<SampleTeam>();
                    foreach (DataRow row in dataSet.Tables["Teams"].Rows)
                    {
                        // 6. Map to model
                        teams.Add(new SampleTeam()
                        {
                            TeamId = Convert.ToInt32(row["TeamId"]),
                            TeamName = row["TeamName"].ToString(),
                            TeamDescription = row["TeamDescription"].ToString(),
                            CreationDate = Convert.ToDateTime(row["CreationDate"].ToString())
                        });
                    }

                    // 7. Returns list of teams
                    return teams;
                }
            }
            catch (Exception e)
            {
                // *. Error handling
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return null;
            }
        }

        /// <summary>
        /// Updates team with given id
        /// </summary>
        /// <param name="team">team object to update with</param>
        /// <returns>1(Success)/0(Fail)</returns>
        public async Task<int> UpdateTeam(SampleTeam team)
        {
            // 1. Create query
            string query = $"UPDATE Teams SET TeamName={team.TeamName}, TeamDescription={team.TeamDescription}"
                + $"WHERE TeamId={team.TeamId}";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    // 2. Open connection
                    await connection.OpenAsync();

                    // 3. Create command
                    SqlCommand command = new SqlCommand(query, connection);

                    // 4. Execute non query(returns 1 on success)
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                // *. Error handling
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return 0;
            }
        }

        /// <summary>
        /// Deletes team with given id
        /// </summary>
        /// <param name="teamId">Id to delete</param>
        /// <returns>1(Success)/0(Fail)</returns>
        public async Task<int> DeleteTeam(int teamId)
        {
            // 1. Create query
            string query = $"DELETE FROM Teams WHERE TeamId={teamId}";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    // 2. Open connection
                    await connection.OpenAsync();

                    // 3. Create command
                    SqlCommand command = new SqlCommand(query, connection);

                    // 4. Execute non query (returns 1 on success)
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception e)
            {
                // *. Error handling
                Debug.WriteLine($"Database Exception: {e.Message}/{e.InnerException?.Message}");
                return 0;
            }
        }
    }
}
