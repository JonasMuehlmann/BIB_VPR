#Benutzte Pakete
Messenger.SignalR.Models
Microsoft.AspNetCore.SignalR
System
System.Collections.Generic
System.Threading.Tasks
#Importschnittstellen
Messenger.SignalR.ChatHub.AddMember(Messenger.Core.Models.User, Messenger.Core.Models.Team)
uint.ToString()
#Exportschnittstellen
public async Task AddMember(User user, Team team)
public async Task AddOrUpdateTeamRole(TeamRole role)
public async Task CreateChannel(Channel channel)
public async Task CreateTeam(Team team)
public async Task DeleteChannel(Channel channel)
public async Task DeleteMessage(Message message, string teamId)
public async Task DeleteTeam(Team team)
public async Task DeleteTeamRole(TeamRole role)
public async Task JoinTeam(string userId, string teamId)
public async Task LeaveTeam(string userId, string teamId)
public Task Register(string userId)
public async Task RemoveMember(User user, Team team)
public async Task SendInvitation(User user, Team team)
public async Task SendMessage(Message message, string teamId)
public async Task UpdateChannel(Channel channel)
public async Task UpdateMember(User user, Team team)
public async Task UpdateMessage(Message message, string teamId)
public async Task UpdateMessageReactions(Message message, string teamId)
public async Task UpdateTeam(Team team)
public async Task UpdateUser(User user)
