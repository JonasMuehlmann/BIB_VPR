#Benutzte Pakete
System
System.Collections.Generic
System.Data
System.Data.SqlClient
System.Linq
System.Threading.Tasks
Serilog
Serilog.Context
Messenger.Core.Services
#Importschnittstellen
Messenger.Core.Helpers.SqlHelpers.ExecuteScalarAsync<string>(string, System.Func<object, string>)
Messenger.Core.Helpers.SqlHelpers.TryConvertDbValue<T>(object, System.Func<object, T>)
Messenger.Core.Services.AzureServiceBase.GetDefaultConnection()
object.GetType()
Serilog.Context.LogContext.PushProperty(string, object, bool)
Serilog.ILogger.Information(string)
System.Convert.ToBoolean(object?)
System.Convert.ToString(object?)
System.Func<DataRow, string, T>.Invoke(DataRow, string)
System.Func<object, T>.Invoke(object)
#Exportschnittstellen
public static async Task<string> GetColumnType(string tableName, string columnName)
public static async Task<IEnumerable<DataRow>> GetRows(string tableName, string query)
public static async Task<IList<T>> MapToList<T> (Func<DataRow, T> mapper, string query, string tableName="")
public static async Task<IList<T>> MapToList<T> (Func<DataRow, string, T> mapper, string query, string tableName, string columnName)
public static async Task<bool> NonQueryAsync(string query)
public static dynamic TryConvertDbValue<T>(object value, Func<object, T> converter)
