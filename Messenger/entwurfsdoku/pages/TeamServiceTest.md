#Benutzte Pakete
System.Threading.Tasks
System.Linq
System
System.Data.SqlClient
Messenger.Core.Models
Messenger.Core.Services
Messenger.Core.Helpers
Microsoft.VisualStudio.TestTools.UnitTesting
#Importschnittstellen
Messenger.Core.Services.TeamService.AddMember(string, uint)
Messenger.Core.Services.TeamService.AddRole(string, uint, string)
Messenger.Core.Services.TeamService.AssignRole(string, string, uint)
Messenger.Core.Services.TeamService.ChangeTeamDescription(uint, string)
Messenger.Core.Services.TeamService.ChangeTeamName(uint, string)
Messenger.Core.Services.TeamService.CreateTeam(string, string)
Messenger.Core.Services.TeamService.DeleteTeam(uint)
Messenger.Core.Services.TeamService.GetAllMembers(uint)
Messenger.Core.Services.TeamService.GetAllTeams()
Messenger.Core.Services.TeamService.GetPermissionsOfRole(uint, string)
Messenger.Core.Services.TeamService.GetTeam(uint)
Messenger.Core.Services.TeamService.GetUsersRoles(uint, string)
Messenger.Core.Services.TeamService.GetUsersWithRole(uint, string)
Messenger.Core.Services.TeamService.GrantPermission(uint, string, Messenger.Core.Models.Permissions)
Messenger.Core.Services.TeamService.HasPermission(uint, string, Messenger.Core.Models.Permissions)
Messenger.Core.Services.TeamService.ListRoles(uint)
Messenger.Core.Services.TeamService.RemoveMember(string, uint)
Messenger.Core.Services.TeamService.RemoveRole(string, uint)
Messenger.Core.Services.TeamService.RevokePermission(uint, string, Messenger.Core.Models.Permissions)
Messenger.Core.Services.TeamService.UnAssignRole(string, string, uint)
Messenger.Core.Services.UserService.GetOrCreateApplicationUser(Messenger.Core.Models.User)
string.Join(char, params string?[])
System.Reflection.MethodBase.GetCurrentMethod()
System.Threading.Tasks.Task.Run(System.Action)
#Exportschnittstellen
public void AddMemberExisting_Test()
public void AddMember_Test()
public void AddRole_Test()
public void AssignRole_Test()
public void ChangeTeamDescription_Test()
public void ChangeTeamName_Test()
public void Cleanup()
public void CreateTeamEmptyName_Test()
public void CreateTeam_Test()
public void DeleteTeamNonexistent_Test()
public void DeleteTeam_Test()
public void GetAllTeamsNoneExist_Test()
public void GetAllTeams_Test()
public void GetUsersRoles_Test()
public void GrantPermission_Test()
public void ListPermissionsOfRole_Test()
public void RemoveMemberNonExistent_Test()
public void RemoveMember_Test()
public void RemoveRole_Test()
public void RevokePermission_Test()
public void UnassignRole_Test()
