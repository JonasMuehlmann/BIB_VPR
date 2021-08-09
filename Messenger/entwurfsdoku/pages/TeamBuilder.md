#Benutzte Pakete
Messenger.Core.Services
Messenger.ViewModels.DataViewModels
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
Windows.UI
#Importschnittstellen
MemberViewModel.WithMemberRoles()
Messenger.Core.Services.MessengerService.GetChannelsForTeam(uint)
Messenger.Core.Services.MessengerService.GetTeamMembers(uint)
Messenger.Core.Services.MessengerService.GetTeams(string)
Messenger.Core.Services.TeamService.GetPermissionsOfRole(uint, string)
Messenger.Core.Services.TeamService.GetUsersRoles(uint, string)
Messenger.Core.Services.TeamService.ListRoles(uint)
Messenger.Helpers.TeamHelpers.TeamBuilder.Build(Messenger.Core.Models.Team, string)
Messenger.Helpers.TeamHelpers.TeamBuilder.DetermineTeamType(TeamViewModel)
Messenger.Helpers.TeamHelpers.TeamBuilder.Map(Messenger.Core.Models.Channel)
Messenger.Helpers.TeamHelpers.TeamBuilder.Map(Messenger.Core.Models.Team)
Messenger.Helpers.TeamHelpers.TeamBuilder.Map(Messenger.Core.Models.TeamRole)
Messenger.Helpers.TeamHelpers.TeamBuilder.Map(System.Collections.Generic.IEnumerable<Messenger.Core.Models.TeamRole>)
Messenger.Helpers.TeamHelpers.TeamBuilder.Map(System.Collections.Generic.IEnumerable<Messenger.Core.Models.User>)
string.Concat(string?, string?)
string.IsNullOrEmpty(string?)
string.Substring(int)
string.Substring(int, int)
string.ToUpper()
System.Collections.Generic.IEnumerable<TeamRoleViewModel>.WithPermissions()
System.Collections.Generic.List<ChannelViewModel>.Add(ChannelViewModel)
System.Collections.Generic.List<TeamViewModel>.Add(TeamViewModel)
TeamRoleViewModel.WithPermissions()
TeamViewModel.WithChannels()
TeamViewModel.WithMembers(string)
TeamViewModel.WithTeamRoles()
#Exportschnittstellen
public static async Task<TeamViewModel> Build(this Team team, string userId)
public static async Task<IEnumerable<TeamViewModel>> Build(this IEnumerable<Team> teams, string userId)
public static TeamViewModel DetermineTeamType(TeamViewModel viewModel)
public static async Task<IEnumerable<Team>> GetTeamsFromDatabase(UserViewModel currentUser)
public static TeamViewModel Map(Team team)
public static TeamRoleViewModel Map(TeamRole teamRole)
public static IEnumerable<TeamRoleViewModel> Map(IEnumerable<TeamRole> teamRoles)
public static ChannelViewModel Map(Channel channel)
public static IEnumerable<ChannelViewModel> Map(IEnumerable<Channel> channels)
public static MemberViewModel Map(User user)
public static IEnumerable<MemberViewModel> Map(IEnumerable<User> users)
public static async Task<TeamViewModel> WithChannels(this TeamViewModel viewModel)
public static async Task<MemberViewModel> WithMemberRoles(this MemberViewModel viewModel)
public static async Task<TeamViewModel> WithMembers(this TeamViewModel viewModel, string currentUserId)
public static async Task<TeamRoleViewModel> WithPermissions(this TeamRoleViewModel viewModel)
public static async Task<IEnumerable<TeamRoleViewModel>> WithPermissions(this IEnumerable<TeamRoleViewModel> viewModels)
public static async Task<TeamViewModel> WithTeamRoles(this TeamViewModel viewModel)
