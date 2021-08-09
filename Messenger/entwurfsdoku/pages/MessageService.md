#Benutzte Pakete
System
System.Data.SqlClient
System.Threading.Tasks
Messenger.Core.Models
Messenger.Core.Helpers
System.Collections.Generic
System.Linq
Serilog.Context
#Importschnittstellen
Messenger.Core.Helpers.SqlHelpers.ExecuteScalarAsync<T>(string, System.Func<object, T>)
Messenger.Core.Helpers.SqlHelpers.GetRows(string, string)
Messenger.Core.Helpers.SqlHelpers.MapToList<T>(System.Func<System.Data.DataRow, T>, string, string)
Messenger.Core.Helpers.SqlHelpers.NonQueryAsync(string)
Messenger.Core.Services.MessageService.DeleteMessage(uint)
Messenger.Core.Services.MessageService.RetrieveReplies(uint)
Serilog.Context.LogContext.PushProperty(string, object, bool)
Serilog.ILogger.Information(string)
string.IsNullOrEmpty(string?)
string.Join(string?, System.Collections.Generic.IEnumerable<string?>)
#Exportschnittstellen
public static async Task<uint> AddReaction(uint messageId, string userId, string reaction)
public static async Task<bool> CanMakeReaction(uint messageId, string userId, string reaction)
public static async Task<uint?> CreateMessage(uint recipientsId,
public static async Task<bool> DeleteMessage(uint messageId)
public static async Task<bool> EditMessage(uint messageId, string newContent)
public static async Task<IEnumerable<string>> GetBlobFileNamesOfAttachments(uint messageId)
public static async Task<Message> GetMessage(uint messageId)
public static async Task<Reaction> GetReaction(uint reactionId)
public static async Task<bool> RemoveReaction(uint messageId, string userId, string reaction)
public static async Task<IList<Message>> RetrieveMessages(uint channelId)
public static async Task<IEnumerable<Reaction>> RetrieveReactions(uint messageId)
public static async Task<IList<Message>> RetrieveReplies(uint messageId)
