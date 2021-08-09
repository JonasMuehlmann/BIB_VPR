#Benutzte Pakete
Messenger.Core.Helpers
Messenger.Core.Models
System
System.Collections.Generic
System.Data.SqlClient
System.Linq
System.Threading.Tasks
Serilog.Context
#Importschnittstellen
Messenger.Core.Helpers.SqlHelpers.ExecuteScalarAsync<T>(string, System.Func<object, T>)
Messenger.Core.Helpers.SqlHelpers.NonQueryAsync(string)
Messenger.Core.Helpers.SqlHelpers.TryConvertDbValue<T>(object, System.Func<object, T>)
Messenger.Core.Services.TeamService.AddMember(string, uint)
Messenger.Core.Services.TeamService.GetAllTeamsByUserId(string)
Serilog.Context.LogContext.PushProperty(string, object, bool)
Serilog.ILogger.Information(string)
#Exportschnittstellen
public static async Task<uint?> CreatePrivateChat(string myUserId, string otherUserId)
public static async Task<IEnumerable<Team>> GetAllPrivateChatsFromUser(string userId)
public static async Task<string> GetPartner(uint teamId)
