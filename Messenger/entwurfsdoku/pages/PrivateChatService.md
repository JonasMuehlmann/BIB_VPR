#Benutzte Pakete
Messenger.Core.Helpers
Messenger.Core.Models
System
System.Collections.Generic
System.Data.SqlClient
System.Linq
System.Threading.Tasks
Serilog.Context
#Exportschnittstellen
public static async Task<uint?> CreatePrivateChat(string myUserId, string otherUserId)
public static async Task<IEnumerable<Team>> GetAllPrivateChatsFromUser(string userId)
public static async Task<string> GetPartner(uint teamId)
