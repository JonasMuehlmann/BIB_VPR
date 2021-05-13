using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;


namespace Messenger.Core.Helpers
{
    public class SqlHelpers
    {
        /// <summary>
        /// Run the specified query on the specified connection.
        /// </summary>
        /// <param name="query">A query to run</param>
        /// <param name="connection">An sql connection to run the query on</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public static async Task<bool> NonQueryAsync(string query, SqlConnection connection)
        {
            try
            {
                await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);

                command.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}");

                return false;
            }
            finally
            {
                connection.Dispose();
            }

            return true;
        }
    }
}