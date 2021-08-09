#Benutzte Pakete
System
System.Collections.Generic
Messenger.Core.Helpers
Messenger.Core.Models
System.Data.SqlClient
System.Linq
System.Threading.Tasks
Serilog.Context
#Exportschnittstellen
public static async Task<uint?> CreateChannel(string channelName, uint teamId)
public static async Task<Channel> GetChannel(uint channelId)
public static async Task<bool> PinMessage(uint messageId, uint channelId)
public static async Task<bool> RemoveAllChannels(uint teamId)
public static async Task<bool> RemoveChannel(uint channelId)
public static async Task<bool> RenameChannel(string channelName, uint channelId)
public static async Task<IEnumerable<Message>> RetrievePinnedMessages(uint channelId)
public static async Task<bool> UnPinMessage(uint messageId, uint channelId)
