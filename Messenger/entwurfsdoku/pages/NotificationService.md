#Benutzte Pakete
System
System.Collections.Generic
System.Threading.Tasks
Serilog.Context
Messenger.Core.Models
Messenger.Core.Helpers
Newtonsoft.Json
Newtonsoft.Json.Linq
#Exportschnittstellen
public static async Task<uint?> AddMute(NotificationType notificationType, NotificationSource notificationSourceType, string notificationSourceValue, string userId, string senderId = null)
public static async Task<bool> CanSendNotification(JObject message, string userId)
public static async Task<IList<NotificationMute>> GetUsersMutes(string userId)
public static async Task<bool> RemoveMute(uint muteId)
public static async Task<bool> RemoveNotification(uint notificationId)
public static async Task<IEnumerable<Notification>> RetrieveNotifications(string userId)
public static async Task<uint?> SendNotification(string recipientId, JObject message)
