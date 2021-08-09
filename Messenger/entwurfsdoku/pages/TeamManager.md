#Benutzte Pakete
Messenger.Models
Messenger.Services.Providers
Messenger.ViewModels.DataViewModels
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
#Importschnittstellen
Messenger.Helpers.TeamHelpers.TeamManager.AddOrUpdateChannel(Messenger.Core.Models.Channel)
Messenger.Helpers.TeamHelpers.TeamManager.AddOrUpdateMember(uint, Messenger.Core.Models.User)
Messenger.Helpers.TeamHelpers.TeamManager.AddOrUpdateTeam(Messenger.Core.Models.Team)
Messenger.Helpers.TeamHelpers.TeamManager.AddOrUpdateTeam(System.Collections.Generic.IEnumerable<Messenger.Core.Models.Team>)
Messenger.Helpers.TeamHelpers.TeamManager.LoadTeamsFromDatabase(UserViewModel)
System.Collections.Generic.ICollection<ChannelViewModel>.Add(ChannelViewModel)
System.Collections.Generic.List<MemberViewModel>.Add(MemberViewModel)
System.Collections.Generic.List<TeamViewModel>.Add(TeamViewModel)
#Exportschnittstellen
public ChannelViewModel AddOrUpdateChannel(Channel channelData)
public IList<ChannelViewModel> AddOrUpdateChannel(IEnumerable<Channel> channelData)
public async Task<MemberViewModel> AddOrUpdateMember(uint teamId, User userData)
public async Task<IList<MemberViewModel>> AddOrUpdateMember(uint teamId, IEnumerable<User> userData)
public async Task<TeamViewModel> AddOrUpdateTeam(Team teamData)
public async Task<IList<TeamViewModel>> AddOrUpdateTeam(IEnumerable<Team> teamData)
public async Task<TeamRoleViewModel> AddOrUpdateTeamRole(TeamRole teamRole)
public async Task Initialize(UserViewModel user)
public async Task<IEnumerable<TeamViewModel>> LoadTeamsFromDatabase(UserViewModel user)
public ChannelViewModel RemoveChannel(uint channelId)
public MemberViewModel RemoveMember(uint teamId, string userId)
public TeamViewModel RemoveTeam(uint teamId)
public TeamRoleViewModel RemoveTeamRole(uint roleId)
public TeamManager(StateProvider provider)
