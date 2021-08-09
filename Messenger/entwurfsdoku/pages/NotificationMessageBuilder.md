#Benutzte Pakete
System
System.Threading.Tasks
Newtonsoft.Json.Linq
Messenger.Core.Models
Messenger.Core.Services
#Exportschnittstellen
public static async Task<JObject> MakeInvitedToTeamNotificationMessage(uint teamId)
public static async Task<JObject> MakeMessageInPrivateChatNotificationMessage(uint messageId)
public static async Task<JObject> MakeMessageInSubscribedChannelNotificationMessage(uint messageId)
public static async Task<JObject> MakeMessageInSubscribedTeamNotificationMessage(uint messageId)
public static async Task<JObject> MakeReactionToMessageNotificationMessage(uint reactionId)
public static async Task<JObject> MakeRemovedFromTeamNotificationMessage(uint teamId)
public static async Task<JObject> MakeUserMentionedNotificationMessage(uint mentionId)
