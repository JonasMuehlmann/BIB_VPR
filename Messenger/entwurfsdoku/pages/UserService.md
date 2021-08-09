#Benutzte Pakete
System
System.Data.SqlClient
System.Threading.Tasks
Messenger.Core.Models
Messenger.Core.Helpers
System.Data
System.Linq
Serilog.Context
System.Collections.Generic
#Importschnittstellen
Messenger.Core.Helpers.Mapper.UserFromMSGraph(Messenger.Core.Models.User)
Messenger.Core.Helpers.SqlHelpers.ExecuteScalarAsync<T>(string, System.Func<object, T>)
Messenger.Core.Helpers.SqlHelpers.GetColumnType(string, string)
Messenger.Core.Helpers.SqlHelpers.GetRows(string, string)
Messenger.Core.Helpers.SqlHelpers.NonQueryAsync(string)
Messenger.Core.Services.AzureServiceBase.GetDefaultConnection()
Messenger.Core.Services.FileSharingService.UploadFromBase64(string, string)
Messenger.Core.Services.UserService.DetermineNewNameId(string, SqlConnection)
Messenger.Core.Services.UserService.UpdateUsername(string, string)
Serilog.Context.LogContext.PushProperty(string, object, bool)
Serilog.ILogger.Information(string)
string.Split(char, System.StringSplitOptions)
string.Trim()
System.Convert.ToString(object?)
System.Convert.ToUInt32(object?)
#Exportschnittstellen
public static async Task<bool> DeleteUser(string userId)
public static async Task<User> GetOrCreateApplicationUser(User userdata)
public static async Task<User> GetUser(string userId)
public static async Task<User> GetUser(string userName, uint nameId)
public static async Task<IList<string>> SearchUser(string userName)
public static async Task<bool> Update(string userId, string columnToChange, string newVal)
public static async Task<bool> UpdateUserBio(string userId, string newBio)
public static async Task<bool> UpdateUserMail(string userId, string newMail)
public static async Task<bool> UpdateUserPhoto(string userId, string newPhoto)
public static async Task<bool> UpdateUsername(string userId, string newUsername)
