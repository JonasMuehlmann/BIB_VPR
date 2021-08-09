#Benutzte Pakete
System
System.Threading.Tasks
System.Linq
System.Collections.Generic
Messenger.Core.Models
Messenger.Core.Services
Messenger.Core.Helpers
Microsoft.VisualStudio.TestTools.UnitTesting
#Importschnittstellen
Messenger.Core.Services.ChannelService.CreateChannel(string, uint)
Messenger.Core.Services.MentionService.CreateMention<T>(Messenger.Core.Models.MentionTarget, T, string)
Messenger.Core.Services.MentionService.RemoveMention(uint)
Messenger.Core.Services.MentionService.ResolveMentions(string)
Messenger.Core.Services.MentionService.SearchMentionable(string, uint)
Messenger.Core.Services.MessageService.CreateMessage(uint, string, string, System.Nullable<uint>, System.Collections.Generic.IEnumerable<string>)
Messenger.Core.Services.MessageService.GetMessage(uint)
Messenger.Core.Services.TeamService.AddMember(string, uint)
Messenger.Core.Services.TeamService.AddRole(string, uint, string)
Messenger.Core.Services.TeamService.CreateTeam(string, string)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
string.Join(char, params string?[])
System.Collections.Generic.List<uint>.Add(uint)
System.Convert.ToUInt32(object?)
System.Reflection.MethodBase.GetCurrentMethod()
System.Threading.Tasks.Task.Run(System.Action)
#Exportschnittstellen
public void CreateMention_Test()
public void RemoveMention_Test()
public void ResolveMentionMentionAtBeginning_Test()
public void ResolveMentionMentionAtEnd_Test()
public void ResolveMentionMentionInMiddle_Test()
public void ResolveMentionNoMention_Test()
public void SearchMentionableChannel_Test()
public void SearchMentionableMessage_Test()
public void SearchMentionableRole_Test()
public void SearchMentionableUser_Test()
