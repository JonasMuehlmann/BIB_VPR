#Benutzte Pakete
Messenger.Models
Messenger.Services.Providers
Messenger.ViewModels.DataViewModels
System.Collections.Generic
System.Collections.ObjectModel
System.Linq
System.Threading.Tasks
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
