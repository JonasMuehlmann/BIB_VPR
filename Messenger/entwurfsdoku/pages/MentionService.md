#Benutzte Pakete
System
System.Collections.Generic
System.Linq
System.Text.RegularExpressions
System.Threading.Tasks
Messenger.Core.Helpers
Messenger.Core.Models
Serilog.Context
#Importschnittstellen
char.IsLetter(char)
Messenger.Core.Helpers.SqlHelpers.ExecuteScalarAsync<T>(string, System.Func<object, T>)
Messenger.Core.Helpers.SqlHelpers.GetRows(string, string)
Messenger.Core.Helpers.SqlHelpers.MapToList<T>(System.Func<System.Data.DataRow, T>, string, string)
Messenger.Core.Helpers.SqlHelpers.NonQueryAsync(string)
Messenger.Core.Services.ChannelService.GetChannel(uint)
Messenger.Core.Services.MentionService.RetrieveMention(uint)
Messenger.Core.Services.MentionService.SearchChannels(string, uint)
Messenger.Core.Services.MentionService.SearchMentionable(string, uint)
Messenger.Core.Services.MentionService.SearchMessages(string, uint)
Messenger.Core.Services.MentionService.SearchRoles(string, uint)
Messenger.Core.Services.MentionService.SearchUsers(string, uint)
Messenger.Core.Services.TeamService.GetRole(uint)
Messenger.Core.Services.UserService.GetUser(string)
object.ToString()
Serilog.Context.LogContext.PushProperty(string, object, bool)
Serilog.ILogger.Information(string)
string.Substring(int)
System.Collections.Generic.List<Messenger.Core.Models.Mentionable>.AddRange(System.Collections.Generic.IEnumerable<Messenger.Core.Models.Mentionable>)
System.Convert.ToUInt32(object?)
#Exportschnittstellen
public static async Task<uint?> CreateMention<T>(MentionTarget target, T id, string mentionerId)
public static async Task<bool> RemoveMention(uint mentionId)
public static async Task<string> ResolveMentions(string message)
public static async Task<Mention> RetrieveMention(uint mentionId)
public static async Task<IList<Mentionable>> SearchMentionable(string searchString, uint teamId)
