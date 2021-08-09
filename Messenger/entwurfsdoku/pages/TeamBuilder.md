#Benutzte Pakete
Messenger.Core.Services
Messenger.ViewModels.DataViewModels
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
Windows.UI
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
