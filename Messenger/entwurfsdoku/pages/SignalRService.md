#Benutzte Pakete
Messenger.Core.Helpers
Microsoft.AspNetCore.SignalR.Client
Microsoft.Extensions.Logging
System
System.Threading.Tasks
Serilog.Context
System.Collections.Generic
#Exportschnittstellen
public async Task AddMember(User user, Team team)
public async Task AddOrUpdateTeamRole(TeamRole role)
public async Task CloseConnection()
public async Task CreateChannel(Channel channel)
public async Task CreateTeam(Team team)
public async Task DeleteChannel(Channel channel)
public async Task DeleteMessage(Message message, uint teamId)
public async Task DeleteTeam(Team team)
public async Task DeleteTeamRole(TeamRole role)
public void Initialize()
public async Task JoinTeam(string userId, string teamId)
public async Task LeaveTeam(string userId, string teamId)
public async Task OpenConnection(string userId)
public async Task RemoveMember(User user, Team team)
public async Task SendInvitation(User user, Team team)
public async Task SendMessage(Message message, uint teamId)
public async Task UpdateChannel(Channel channel)
public async Task UpdateMember(User user, Team team)
public async Task UpdateMessage(Message message, uint teamId)
public async Task UpdateMessageReactions(Message message, uint teamId)
public async Task UpdateTeam(Team team)
public async Task UpdateUser(User user)
