using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;


namespace Messenger.Core.Helpers
{
    public class SqlHelpers
    {
        static ILogger logger = GlobalLogger.Instance;

        /// <summary>
        /// Run the specified query on the specified connection.
        /// </summary>
        /// <param name="query">A query to run</param>
        /// <param name="connection">An sql connection to run the query on</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public static async Task<bool> NonQueryAsync(string query, SqlConnection connection)
        {
            LogContext.PushProperty("Method","NonQueryAsync");
            LogContext.PushProperty("SourceContext", "SqlHelpers");
            logger.Information($"Function called with parameters query={query}");

            try
            {
                if (connection.State != ConnectionState.Open)
                {
                    await connection.OpenAsync();
                }
                SqlCommand command = new SqlCommand(query, connection);

                var result = Convert.ToBoolean(await command.ExecuteNonQueryAsync());

                logger.Information($"Return value: {result}");

                return result;

            }
            catch (SqlException e)
            {
                logger.Information(e,"Return value: false");

                return false;
            }
            finally
            {
                connection.Dispose();
            }
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
            LogContext.PushProperty("Method","GetColumnType");
            LogContext.PushProperty("SourceContext", "SqlHelpers");
            logger.Information($"Function called with parameters tableName={tableName}, columnName={columnName}");

            SqlCommand query = new SqlCommand(
                    $"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}';"
                    ,connection
            );

            try
            {
                logger.Information($"Running the following query: {query}");

                var result = query.ExecuteScalar();

                result = TryConvertDbValue(result, Convert.ToString);

                logger.Information($"Return value: {result}");

                return (string)result;
            }
            catch (SqlException e)
            {
                logger.Information(e,"Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Return an enumerable of data rows
        /// </summary>
        /// <param name="tableName">Name of the table to read from</param>
        /// <param name="adapter">Instance of adapter with an opened connection</param>
        /// <returns>An enumerable of data rows</returns>
        public static IEnumerable<DataRow> GetRows(string tableName, SqlDataAdapter adapter)
        {
            LogContext.PushProperty("Method","GetRows");
            LogContext.PushProperty("SourceContext", "SqlHelpers");
            logger.Information($"Function called with parameters tableName={tableName}");

            var dataSet = new DataSet();
            adapter.Fill(dataSet, tableName);

            var result = dataSet.Tables[tableName].Rows.Cast<DataRow>();

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Maps to a list of target type instances
        /// </summary>
        /// <typeparam name="T">The type to map to</typeparam>
        /// <param name="mapper">Mapper function for the target type</param>
        /// <param name="adapter">Instance of adapter with an opened connection</param>
        /// <returns></returns>
        public static IList<T> MapToList<T> (Func<DataRow, T> mapper, SqlDataAdapter adapter)
        {
            LogContext.PushProperty("Method","MapToList");
            LogContext.PushProperty("SourceContext", "SqlHelpers");
            logger.Information($"Function called with parameters mapper={mapper.Method.Name}");

            string tableName = nameof(T) + 's';

            logger.Information($"tableName has been determined as {tableName}");

            var dataSet = new DataSet();
            adapter.Fill(dataSet, tableName);

            var result = dataSet.Tables[tableName].Rows
                         .Cast<DataRow>()
                         .Select(mapper)
                         .ToList();

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Convert a value that can be DBNull using a specified converter
        /// </summary>
        /// <typeparam name="T">A type to convert value to</typeparam>
        /// <param name="value">A value to convert to T>
        /// <param name="converter">A converter function to use for converting value>
        /// <returns>null or the wanted type T</returns>
        public static dynamic TryConvertDbValue<T>(object value, Func<object, T> converter) where T: IConvertible
        {
            if (value is DBNull)
            {
                return null;
            }

            return converter(value);
        }
    }
}
