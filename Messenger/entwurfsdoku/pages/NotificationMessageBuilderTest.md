#Benutzte Pakete
System.Linq
System.Threading.Tasks
Messenger.Core.Models
Messenger.Core.Helpers
Messenger.Core.Services
Microsoft.VisualStudio.TestTools.UnitTesting
Newtonsoft.Json.Linq
#Importschnittstellen
Messenger.Core.Helpers.NotificationMessageBuilder.MakeInvitedToTeamNotificationMessage(uint)
Messenger.Core.Helpers.NotificationMessageBuilder.MakeMessageInPrivateChatNotificationMessage(uint)
Messenger.Core.Helpers.NotificationMessageBuilder.MakeMessageInSubscribedChannelNotificationMessage(uint)
Messenger.Core.Helpers.NotificationMessageBuilder.MakeMessageInSubscribedTeamNotificationMessage(uint)
Messenger.Core.Services.ChannelService.CreateChannel(string, uint)
Messenger.Core.Services.MessageService.CreateMessage(uint, string, string, System.Nullable<uint>, System.Collections.Generic.IEnumerable<string>)
Messenger.Core.Services.PrivateChatService.CreatePrivateChat(string, string)
Messenger.Core.Services.TeamService.CreateTeam(string, string)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
object.ToString()
System.Reflection.MethodBase.GetCurrentMethod()
System.Threading.Tasks.Task.Run(System.Action)
uint.ToString()
#Exportschnittstellen
public void MakeInvitedToTeamNotificationMessage_Test()
public void MakeMessageInPrivateChatNotificationMessage_Test()
public void MakeMessageInSubscribedChannelNotificationMessage_Test()
public void MakeMessageInSubscribedTeamNotificationMessage_Test()
