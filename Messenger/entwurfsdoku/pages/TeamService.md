#Benutzte Pakete
Messenger.Core.Helpers
Messenger.Core.Models
System.Collections.Generic
System.Data.SqlClient
System.Linq
System.Threading.Tasks
Serilog.Context
#Exportschnittstellen
public static async Task<bool> AddMember(string userId, uint teamId)
public static async Task<uint?> AddRole(string role, uint teamId, string colorCode)
public static async Task<bool> AssignRole(string role, string userId, uint teamId)
public static async Task<bool> ChangeTeamDescription(uint teamId, string description)
public static async Task<bool> ChangeTeamName(uint teamId, string teamName)
public static async Task<uint?> CreateTeam(string teamName, string teamDescription = "")
public static async Task<bool> DeleteTeam(uint teamId)
public static async Task<IList<Channel>> GetAllChannelsByTeamId(uint teamId)
public static async Task<IEnumerable<User>> GetAllMembers(uint teamId)
public static async Task<IList<Membership>> GetAllMembershipByUserId(string userId)
public static async Task<IEnumerable<Team>> GetAllTeams()
public static async Task<IEnumerable<Team>> GetAllTeamsByUserId(string userId)
public static async Task<IList<Permissions>> GetPermissionsOfRole(uint teamId, string role)
public static async Task<TeamRole> GetRole(uint roleId)
public static async Task<Team> GetTeam(uint teamId)
public static async Task<IList<TeamRole>> GetUsersRoles(uint teamId, string userId)
public static async Task<IList<User>> GetUsersWithRole(uint teamId, string role)
public static async Task<uint?> GrantPermission(uint teamId, string role, Permissions permission)
public static async Task<bool> HasPermission(uint teamId, string role, Permissions permission)
public static async Task<IList<TeamRole>> ListRoles(uint teamId)
public static async Task<bool> RemoveMember(string userId, uint teamId)
public static async Task<uint?> RemoveRole(string role, uint teamId)
public static async Task<uint?> RevokePermission(uint teamId, string role, Permissions permission)
public static async Task<bool> UnAssignRole(string role, string userId, uint teamId)
public static async Task<bool> UpdateRole(uint roleId, string role, string colorCode)
public static async Task<bool> UpdateTeam(string teamName, string teamDescription, uint teamId)
