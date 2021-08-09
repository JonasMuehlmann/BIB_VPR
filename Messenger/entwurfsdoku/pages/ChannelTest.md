#Benutzte Pakete
System.Threading.Tasks
System.Linq
System.Collections.Generic
Messenger.Core.Services
Messenger.Core.Models
Microsoft.VisualStudio.TestTools.UnitTesting
#Importschnittstellen
Messenger.Core.Services.ChannelService.CreateChannel(string, uint)
Messenger.Core.Services.ChannelService.PinMessage(uint, uint)
Messenger.Core.Services.ChannelService.RemoveChannel(uint)
Messenger.Core.Services.ChannelService.RenameChannel(string, uint)
Messenger.Core.Services.ChannelService.RetrievePinnedMessages(uint)
Messenger.Core.Services.ChannelService.UnPinMessage(uint, uint)
Messenger.Core.Services.MessageService.CreateMessage(uint, string, string, System.Nullable<uint>, System.Collections.Generic.IEnumerable<string>)
Messenger.Core.Services.TeamService.CreateTeam(string, string)
Messenger.Core.Services.TeamService.GetAllChannelsByTeamId(uint)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
System.Reflection.MethodBase.GetCurrentMethod()
System.Threading.Tasks.Task.Run(System.Action)
#Exportschnittstellen
public void Cleanup()
public void CreateChannel_Test()
public void PinMessage_Test()
public void RemoveChannel_Test()
public void RenameChannel_Test()
public void UnPinMessage_Test()
