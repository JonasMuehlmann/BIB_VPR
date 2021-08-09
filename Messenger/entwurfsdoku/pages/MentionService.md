#Benutzte Pakete
System
System.Collections.Generic
System.Linq
System.Text.RegularExpressions
System.Threading.Tasks
Messenger.Core.Helpers
Messenger.Core.Models
Serilog.Context
#Exportschnittstellen
public static async Task<uint?> CreateMention<T>(MentionTarget target, T id, string mentionerId)
public static async Task<bool> RemoveMention(uint mentionId)
public static async Task<string> ResolveMentions(string message)
public static async Task<Mention> RetrieveMention(uint mentionId)
public static async Task<IList<Mentionable>> SearchMentionable(string searchString, uint teamId)
