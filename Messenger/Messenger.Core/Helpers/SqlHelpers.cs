using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
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
                if (connection.State != ConnectionState.Open) await connection.OpenAsync();
                SqlCommand command = new SqlCommand(query, connection);

                await command.ExecuteNonQueryAsync();
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
        /// <param name="tableName">A table to check a columns type of</param>
        /// <param name="columnName">A column to check the type of</param>
        /// <param name="connection">An sql connection to run the query on</param>
        /// <returns>Null if the specifid column does not exist in the table, it's type name otherwise</returns>
        public static string GetColumnType(string tableName, string columnName, SqlConnection connection)
        {
            SqlCommand query = new SqlCommand(
                    $"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}';"
                    ,connection
            );

            return Convert.ToString(query.ExecuteScalar());
        }

        /// <summary>
        /// Return an enumerable of data rows
        /// </summary>
        /// <param name="tableName">Name of the table to read from</param>
        /// <param name="adapter">Instance of adapter with an opened connection</param>
        /// <returns>An enumerable of data rows</returns>
        public static IEnumerable<DataRow> GetRows(string tableName, SqlDataAdapter adapter)
        {
            var dataSet = new DataSet();
            adapter.Fill(dataSet, tableName);

            return dataSet.Tables[tableName].Rows.Cast<DataRow>();
        }


        /// <summary>
        /// In a private chat, retrieve the conversation partner's user id
        /// </summary>
        /// <param name="teamId">the id of the team belonging to the private chat</param>
        /// <param name="connection">A connection to the used sql database</param>
        /// <returns>The user id of the conversation partner</returns>
        public static string GetPartner(string teamId, SqlConnection connection)
        {
            // NOTE: Private Chats currently only support 1 Members
            string query = "SELECT UserId  FROM Memberships m LEFT JOIN Teams t ON m.TeamId = t.TeamId"
                         + $"WHERE t.TeamId != '{teamId}'";

            SqlCommand scalarQuery = new SqlCommand(query, connection);
            var        otherUser   = scalarQuery.ExecuteScalar();

            return otherUser is DBNull ? null : otherUser.ToString();
        }
    }
}
