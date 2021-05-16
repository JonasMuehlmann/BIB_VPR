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

        /// <summary>
        /// Return a string representing the SQL datatype of the specified column in the specified table.
        /// </summary>
        /// <returns>Null if the specifid column does not exist in the table, it's type name otherwise</returns>
        public static string GetColumnType(string tableName, string columnName, SqlConnection connection)
        {
            SqlCommand query = new SqlCommand($"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}';", connection);

            return Convert.ToString(query.ExecuteScalar());
        }
    }
}
