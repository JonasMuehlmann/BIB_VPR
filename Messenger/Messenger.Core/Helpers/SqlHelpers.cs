using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;
using Messenger.Core.Services;

namespace Messenger.Core.Helpers {
  public class SqlHelpers : AzureServiceBase {
    /// <summary>
    /// Run the specified query on the specified connection.
    /// </summary>
    /// <param name="query">A query to run</param>
    /// <param name="connection">An sql connection to run the query on</param>
    /// <returns>True if no exceptions occured while executing the query and it
    /// affected at least one entry, false otherwise</returns>
    public static async Task<bool> NonQueryAsync(string query) {
      LogContext.PushProperty("Method", "NonQueryAsync");
      LogContext.PushProperty("SourceContext", "SqlHelpers");
      logger.Information($"Function called with parameters query={query}");

      using (SqlConnection connection = GetDefaultConnection()) {
        try {
          await connection.OpenAsync();

          SqlCommand command = new SqlCommand(query, connection);

          var result = Convert.ToBoolean(await command.ExecuteNonQueryAsync());

          logger.Information($"Return value: {result}");

          return result;
        } catch (SqlException e) {
          logger.Information(e, "Return value: false");

          return false;
        }
      }
    }

    /// <summary>
    /// Run the specified query on the specified connection and retrieve a
    /// converterd scalar value.
    /// </summary>
    /// <typeparam name="T">The type to map to</typeparam>
    /// <param name="query">A query to run</param>
    /// <param name="connection">An sql connection to run the query on</param>
    /// <returns>
    /// The converted scalar result on success, The default value of T on
    /// Failure
    /// </returns>
    public static async Task<T> ExecuteScalarAsync<T>(string query,
                                                      Func<object, T> converter)
        where T : IConvertible {
      LogContext.PushProperty("Method", "ExecuteScalarAsync");
      LogContext.PushProperty("SourceContext", "SqlHelpers");
      logger.Information($"Function called with parameters query={query}");

      using (SqlConnection connection = GetDefaultConnection()) {
        try {
          await connection.OpenAsync();

          SqlCommand command = new SqlCommand(query, connection);

          var result = TryConvertDbValue(await command.ExecuteScalarAsync(),
                                         converter) ?? default(T);

          LogContext.PushProperty("Method", "ExecuteScalarAsync");
          LogContext.PushProperty("SourceContext", "SqlHelpers");
          logger.Information($"Return value: {result}");

          return result;
        } catch (SqlException e) {
          logger.Information(e, $"Return value: {default(T)}");

          return default(T);
        }
      }
    }

    /// <summary>
    /// Return a string representing the SQL datatype of the specified column in
    /// the specified table.
    /// </summary>
    /// <param name="tableName">A table to check a columns type of</param>
    /// <param name="columnName">A column to check the type of</param>
    /// <param name="connection">An sql connection to run the query on</param>
    /// <returns>Null if the specifid column does not exist in the table, it's
    /// type name otherwise</returns>
    public static async Task<string> GetColumnType(string tableName,
                                                   string columnName) {
      LogContext.PushProperty("Method", "GetColumnType");
      LogContext.PushProperty("SourceContext", "SqlHelpers");
      logger.Information(
          $"Function called with parameters tableName={tableName}, columnName={columnName}");

      string query =
          $"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '{tableName}' AND COLUMN_NAME = '{columnName}';";

      using (SqlConnection connection = GetDefaultConnection()) {
        try {
          await connection.OpenAsync();

          logger.Information($"Running the following query: {query}");

          var result =
              await SqlHelpers.ExecuteScalarAsync(query, Convert.ToString);

          logger.Information($"Return value: {result}");

          return (string)result;
        } catch (SqlException e) {
          logger.Information(e, "Return value: null");

          return null;
        }
      }
    }

    /// <summary>
    /// Return an enumerable of data rows
    /// </summary>
    /// <param name="tableName">Name of the table to read from</param>
    /// <param name="query">The query that returns the data rows</param>
    /// <returns>An enumerable of data rows</returns>
    public static async Task<IEnumerable<DataRow>> GetRows(string tableName,
                                                           string query) {
      LogContext.PushProperty("Method", "GetRows");
      LogContext.PushProperty("SourceContext", "SqlHelpers");
      logger.Information(
          $"Function called with parameters tableName={tableName}");

      using (SqlConnection connection = GetDefaultConnection()) {
        try {
          await connection.OpenAsync();

          var adapter = new SqlDataAdapter(query, connection);
          var dataSet = new DataSet();
          adapter.Fill(dataSet, tableName);

          return dataSet.Tables[tableName].Rows.Cast<DataRow>();
        } catch (SqlException e) {
          logger.Information($"{e}");
          return null;
        }
      }
    }

    /// <summary>
    /// Maps to a list of target type instances
    /// </summary>
    /// Infers the table name from the mapping type if tableName is not
    /// specified <typeparam name="T">The type to map to</typeparam> <param
    /// name="mapper">Mapper function for the target type</param> <param
    /// name="query">The query that returns the list</param> <returns>A list of
    /// converted table values</returns>
    public static async Task<IList<T>> MapToList<T>(Func<DataRow, T> mapper,
                                                    string query) {
      LogContext.PushProperty("Method", "MapToList");
      LogContext.PushProperty("SourceContext", "SqlHelpers");
      logger.Information(
          $"Function called with parameters mapper={mapper.Method.Name}");

      using (SqlConnection connection = GetDefaultConnection()) {
        try {
          await connection.OpenAsync();

          var tableName = typeof(T).Name + 's';

          logger.Information($"tableName has been determined as {tableName}");

          logger.Information($"Running the following query: {query}");

          var adapter = new SqlDataAdapter(query, connection);
          var dataSet = new DataSet();
          adapter.Fill(dataSet, tableName);

          logger.Information(
              $"The query produced {dataSet.Tables.Count} row(s)");

          return dataSet.Tables[tableName]
              .Rows.Cast<DataRow>()
              .Select(mapper)
              .ToList();
        } catch (SqlException e) {
          logger.Information($"{e}");
          return null;
        }
      }
    }

    /// <summary>
    /// Maps to a list of target type instances
    /// </summary>
    /// Infers the table name from the mapping type if tableName is not
    /// specified <typeparam name="T">The type to map to</typeparam> <param
    /// name="mapper">Mapper function for the target type</param> <param
    /// name="query">The query that returns the list</param> <param
    /// name="tableName">The name of the table to retrieve data from, defaults
    /// to null</param> <param name="columnName">The name of the column to
    /// retrieve data from, defaults to null</param> <returns>A list of
    /// converted table values</returns>
    public static async Task<IList<T>>
    MapToList<T>(Func<DataRow, string, T> mapper, string query,
                 string tableName, string columnName) {
      LogContext.PushProperty("Method", "MapToList");
      LogContext.PushProperty("SourceContext", "SqlHelpers");
      logger.Information(
          $"Function called with parameters mapper={mapper.Method.Name}, tableName={tableName}, columnName={columnName}");

      using (SqlConnection connection = GetDefaultConnection()) {
        try {
          await connection.OpenAsync();

          logger.Information($"tableName has been determined as {tableName}");

          var adapter = new SqlDataAdapter(query, connection);
          var dataSet = new DataSet();
          adapter.Fill(dataSet, tableName);

          logger.Information(
              $"The query produced {dataSet.Tables.Count} row(s)");

          Func<DataRow, T> _mapper = (row) => mapper(row, columnName);

          return dataSet.Tables[tableName]
              .Rows.Cast<DataRow>()
              .Select(_mapper)
              .ToList();

        } catch (SqlException e) {
          logger.Information($"{e}");
          return null;
        }
      }
    }

    /// <summary>
    /// Convert a value that can be DBNull using a specified converter
    /// </summary>
    /// <typeparam name="T">A type to convert value to</typeparam>
    /// <param name="value">A value to convert to T</param>
    /// <param name="converter">A converter function to use for converting
    /// value</param> <returns>null or the wanted type T</returns>
    public static dynamic TryConvertDbValue<T>(object value,
                                               Func<object, T> converter)
        where T : IConvertible {
      LogContext.PushProperty("Method", "TryConvertDbValue");
      LogContext.PushProperty("SourceContext", "SqlHelpers");
      logger.Information(
          $"Function called with parameters value={(value is DBNull ? "DBNull" : value is null ? "null" : value)}, converter={converter.Method.Name}");

      if (value is DBNull || value is null) {
        logger.Information("Return value: null");

        return null;
      }

      var result = converter(value);

      logger.Information(
          $"Return value: {(result.GetType() == null ? "null" : Convert.ToString(result))}");

      return result;
    }
  }
}
