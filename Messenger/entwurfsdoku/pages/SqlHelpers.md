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
#Exportschnittstellen
public static async Task<string> GetColumnType(string tableName, string columnName)
public static async Task<IEnumerable<DataRow>> GetRows(string tableName, string query)
public static async Task<IList<T>> MapToList<T> (Func<DataRow, T> mapper, string query, string tableName="")
public static async Task<IList<T>> MapToList<T> (Func<DataRow, string, T> mapper, string query, string tableName, string columnName)
public static async Task<bool> NonQueryAsync(string query)
public static dynamic TryConvertDbValue<T>(object value, Func<object, T> converter)
