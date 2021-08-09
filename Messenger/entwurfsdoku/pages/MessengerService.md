#Benutzte Pakete
Messenger.Core.Models
System
System.Collections.Generic
System.Linq
System.Threading.Tasks
Serilog
Serilog.Context
#Exportschnittstellen
public static async Task<bool> AssignUserRole(string role, string userId, uint teamId)
public static async Task<uint?> CreateChannel(string channelName, uint teamId)
public static async Task<Reaction> CreateMessageReaction(uint messageId, string userId, uint teamId, string reaction)
public static async Task<uint?> CreateTeam(string creatorId, string teamName, string teamDescription = "")
public static async Task<bool> CreateTeamRole(string role, uint teamId, string colorCode)
public static async Task<Channel> DeleteChannel(uint channelId)
public static async Task<bool> DeleteMessage(uint messageId, uint teamId)
public static async Task<Reaction> DeleteMessageReaction(uint messageId, string userId, uint teamId, string reaction)
public static async Task<bool> DeleteTeam(uint teamId)
public static async Task<bool> DeleteTeamRole(string role, uint teamId)
public static async Task<IEnumerable<Channel>> GetChannelsForTeam(uint teamId)
public static async Task<Message> GetMessage(uint messageId)
public static async Task<IEnumerable<Message>> GetMessages(uint channelId)
public static async Task<IEnumerable<Reaction>> GetReactions(uint messageId)
public static async Task<IEnumerable<TeamRole>> GetRolesList(uint teamId, string userId)
public static async Task<Team> GetTeam(uint teamId)
public static async Task<IEnumerable<User>> GetTeamMembers(uint teamId)
public static async Task<IEnumerable<Team>> GetTeams(string userId)
public static async Task<User> GetUserWithNameId(string username, uint nameId)
public static async Task<bool> GrantPermission(uint teamId, string role, Permissions permission)
public static async Task<IList<Team>> Initialize(string userId, string connectionString = null)
public static async Task<bool> RemoveMember(string userId, uint teamId)
public static async Task<bool> RenameChannel(string channelName, uint channelId)
public static async Task<bool> RevokePermission(uint teamId, string role, Permissions permission)
public static async Task<bool> SendInvitation(string userId, uint teamId)
public static async Task<bool> SendMessage(Message message, uint teamId)
public static async Task<uint?> StartChat(string userId, string targetUserId)
public static async Task<bool> UnassignUserRole(string role, string userId, uint teamId)
public static async Task<bool> UpdateMessage(uint messageId, string newContent, uint teamId)
public static async Task<bool> UpdateTeamDescription(string teamDescription, uint teamId)
public static async Task<bool> UpdateTeamName(string teamName, uint teamId)
public static async Task<bool> UpdateTeamRole(uint roleId, string role, string colorCode)
public static async Task<bool> UpdateUserBio(string userId, string newBio)
public static async Task<bool> UpdateUserEmail(string userId, string newEmail)
public static async Task<bool> UpdateUserPhoto(string userId, string newPhotoURL)
