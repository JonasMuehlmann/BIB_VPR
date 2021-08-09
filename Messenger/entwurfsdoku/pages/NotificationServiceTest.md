#Benutzte Pakete
System.Linq
System.Threading.Tasks
Messenger.Core.Models
Messenger.Core.Helpers
Messenger.Core.Services
System.Data.SqlClient
System.Collections.Generic
Microsoft.VisualStudio.TestTools.UnitTesting
Newtonsoft.Json.Linq
#Importschnittstellen
Messenger.Core.Helpers.NotificationMessageBuilder.MakeMessageInSubscribedChannelNotificationMessage(uint)
Messenger.Core.Services.ChannelService.CreateChannel(string, uint)
Messenger.Core.Services.MessageService.CreateMessage(uint, string, string, System.Nullable<uint>, System.Collections.Generic.IEnumerable<string>)
Messenger.Core.Services.NotificationService.AddMute(Messenger.Core.Models.NotificationType, Messenger.Core.Models.NotificationSource, string, string, string)
Messenger.Core.Services.NotificationService.CanSendNotification(Newtonsoft.Json.Linq.JObject, string)
Messenger.Core.Services.NotificationService.RemoveMute(uint)
Messenger.Core.Services.NotificationService.RemoveNotification(uint)
Messenger.Core.Services.NotificationService.RetrieveNotifications(string)
Messenger.Core.Services.NotificationService.SendNotification(string, Newtonsoft.Json.Linq.JObject)
Messenger.Core.Services.TeamService.CreateTeam(string, string)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
object.ToString()
System.Reflection.MethodBase.GetCurrentMethod()
System.Threading.Tasks.Task.Run(System.Action)
uint.ToString()
#Exportschnittstellen
public void AddMute_Test()
public void RemoveMute_Test()
public void RemoveNotification_Test()
public void SendNotification_Test()
