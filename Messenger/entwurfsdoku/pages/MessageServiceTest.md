#Benutzte Pakete
System.Threading.Tasks
System.Linq
Messenger.Core.Models
Messenger.Core.Services
Microsoft.VisualStudio.TestTools.UnitTesting
#Importschnittstellen
Messenger.Core.Services.ChannelService.CreateChannel(string, uint)
Messenger.Core.Services.MessageService.AddReaction(uint, string, string)
Messenger.Core.Services.MessageService.CreateMessage(uint, string, string, System.Nullable<uint>, System.Collections.Generic.IEnumerable<string>)
Messenger.Core.Services.MessageService.DeleteMessage(uint)
Messenger.Core.Services.MessageService.EditMessage(uint, string)
Messenger.Core.Services.MessageService.RemoveReaction(uint, string, string)
Messenger.Core.Services.MessageService.RetrieveMessages(uint)
Messenger.Core.Services.MessageService.RetrieveReactions(uint)
Messenger.Core.Services.MessageService.RetrieveReplies(uint)
Messenger.Core.Services.TeamService.CreateTeam(string, string)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
System.Reflection.MethodBase.GetCurrentMethod()
System.Threading.Tasks.Task.Run(System.Action)
#Exportschnittstellen
public void AddReaction_Test()
public void Cleanup()
public void CreateMessage_Test()
public void RemoveMessageWithReplies_Test()
public void RemoveMessage_Test()
public void RemoveReaction_Test()
public void RenameMessage_Test()
public void RetrieveMessagesNoneExist_Test()
public void RetrieveMessages_Test()
public void RetrieveReplies_Test()
