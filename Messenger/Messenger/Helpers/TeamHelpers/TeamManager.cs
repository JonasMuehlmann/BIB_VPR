using Messenger.Core.Models;
using Messenger.Models;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers.TeamHelpers
{
    public class TeamManager : Observable
    {
        #region Private

        private readonly StateProvider _provider;

        private ObservableCollection<TeamViewModel> _myTeams = new ObservableCollection<TeamViewModel>();

        private ObservableCollection<PrivateChatViewModel> _myChats = new ObservableCollection<PrivateChatViewModel>();

        #endregion

        public ObservableCollection<TeamViewModel> MyTeams
        {
            get
            {
                return _myTeams;
            }
        }

        public ObservableCollection<PrivateChatViewModel> MyChats
        {
            get
            {
                return _myChats;
            }
        }

        public TeamManager(StateProvider provider)
        {
            _provider = provider;
        }

        public async Task Initialize(UserViewModel user)
        {
            await LoadTeamsFromDatabase(user);

            _provider.MessageManager.MessagesLoadedForChannel += OnMessagesLoadedForChannel;
        }

        /// <summary>
        /// Fires only when initializing, to set the last message of the channel to be shown on the navigation panel
        /// </summary>
        /// <param name="sender">Message Manager</param>
        /// <param name="args">Argument containing the messages, team and channel</param>
        private void OnMessagesLoadedForChannel(object sender, ManagerEventArgs args)
        {
            if (args.Team is PrivateChatViewModel)
            {
                foreach (PrivateChatViewModel chat in _myChats)
                {
                    if (chat.MainChannel == args.Channel)
                    {
                        chat.LastMessage = args.Messages.Last();
                    }
                }
            }
            else
            {
                foreach (TeamViewModel team in _myTeams)
                {
                    if (team.Channels.Contains(args.Channel))
                    {
                        ChannelViewModel channel = team.Channels.Single(c => c.ChannelId == args.Channel.ChannelId);

                        channel.LastMessage = args.Messages.Last();
                    }
                }
            }
        }

        #region Load

        public async Task<IEnumerable<TeamViewModel>> LoadTeamsFromDatabase(UserViewModel user)
        {
            IEnumerable<Team> data = await TeamBuilder.GetTeamsFromDatabase(user);

            /* EXIT IF NO DATA */
            if (data == null || data.Count() <= 0)
            {
                return null;
            }

            IEnumerable<TeamViewModel> viewModels = await AddOrUpdateTeam(data);

            return viewModels;
        }

        #endregion

        #region Add or Update

        public async Task<TeamViewModel> AddOrUpdateTeam(Team teamData)
        {
            dynamic viewModel = await TeamBuilder.Build(teamData, _provider.CurrentUser.Id);

            if (viewModel is PrivateChatViewModel)
            {
                PrivateChatViewModel chatViewModel = (PrivateChatViewModel)viewModel;

                if (!_myChats.Any(chat => chat.Id == chatViewModel.Id))
                {
                    _myChats.Add(chatViewModel);
                }
                else
                {
                    PrivateChatViewModel oldValue = _myChats.SingleOrDefault(e => e.Id == chatViewModel.Id);

                    int index = _myChats.IndexOf(oldValue);

                    _myChats[index] = chatViewModel;
                }

                return chatViewModel;
            }
            else
            {
                TeamViewModel teamViewModel = viewModel as TeamViewModel;

                if (!_myTeams.Any(team => team.Id == teamViewModel.Id))
                {
                    _myTeams.Add(teamViewModel);
                }
                else
                {
                    TeamViewModel oldValue = _myTeams.SingleOrDefault(e => e.Id == teamViewModel.Id);

                    int index = _myTeams.IndexOf(oldValue);

                    _myTeams[index] = teamViewModel;
                }

                return teamViewModel;
            }
        }

        public async Task<IList<TeamViewModel>> AddOrUpdateTeam(IEnumerable<Team> teamData)
        {
            List<TeamViewModel> result = new List<TeamViewModel>();

            foreach (Team data in teamData)
            {
                TeamViewModel viewModel = await AddOrUpdateTeam(data);
                result.Add(viewModel);
            }

            return result;
        }

        public ChannelViewModel AddOrUpdateChannel(Channel channelData)
        {
            foreach (TeamViewModel teamViewModel in _myTeams)
            {
                if (teamViewModel.Id == channelData.TeamId)
                {
                    ChannelViewModel viewModel = TeamBuilder.Map(channelData);

                    if (!teamViewModel.Channels.Any(channel => channel.ChannelId == viewModel.ChannelId))
                    {
                        teamViewModel.Channels.Add(viewModel);
                    }
                    else
                    {
                        int index = teamViewModel.Channels.IndexOf(viewModel);

                        teamViewModel.Channels[index] = viewModel;
                    }

                    return viewModel;
                }
            }

            return null;
        }

        public IList<ChannelViewModel> AddOrUpdateChannel(IEnumerable<Channel> channelData)
        {
            IList<ChannelViewModel> result = new List<ChannelViewModel>();

            foreach (Channel channel in channelData)
            {
                result.Add(AddOrUpdateChannel(channel));
            }

            return result;
        }

        public async Task<MemberViewModel> AddOrUpdateMember(uint teamId, User userData)
        {
            foreach (TeamViewModel teamViewModel in _myTeams)
            {
                if (teamViewModel.Id == teamId)
                {
                    MemberViewModel member = await TeamBuilder
                        .Map(userData)
                        .WithMemberRoles(teamId);

                    if (!teamViewModel.Members.Any(m => m.Id == member.Id))
                    {
                        teamViewModel.Members.Add(member);
                    }
                    else
                    {
                        MemberViewModel oldValue = teamViewModel.Members.SingleOrDefault(m => m.Id == member.Id);

                        int index = teamViewModel.Members.IndexOf(oldValue);

                        teamViewModel.Members[index] = member;
                    }

                    return member;
                }
            }

            return null;
        }

        public async Task<IList<MemberViewModel>> AddOrUpdateMember(uint teamId, IEnumerable<User> userData)
        {
            List<MemberViewModel> members = new List<MemberViewModel>();

            foreach (User data in userData)
            {
                MemberViewModel member = await AddOrUpdateMember(teamId, data);

                members.Add(member);
            }

            return members;
        }

        public async Task<TeamRoleViewModel> AddOrUpdateTeamRole(TeamRole teamRole)
        {
            foreach (TeamViewModel teamViewModel in _myTeams)
            {
                if (teamViewModel.Id == teamRole.TeamId)
                {
                    TeamRoleViewModel roleViewModel = await TeamBuilder.Map(teamRole).WithPermissions();

                    if (!teamViewModel.TeamRoles.Any(r => r.Id == roleViewModel.Id))
                    {
                        teamViewModel.TeamRoles.Add(roleViewModel);
                    }
                    else
                    {
                        TeamRoleViewModel oldValue = teamViewModel.TeamRoles.SingleOrDefault(r => r.Id == roleViewModel.Id);

                        int index = teamViewModel.TeamRoles.IndexOf(oldValue);

                        teamViewModel.TeamRoles[index] = roleViewModel;
                    }

                    return roleViewModel;
                }
            }

            return null;
        }

        #endregion

        #region Remove

        public TeamViewModel RemoveTeam(uint teamId)
        {
            TeamViewModel viewModel = _myTeams.SingleOrDefault(team => team.Id == teamId);
            
            if (viewModel == null)
            {
                return null;
            }

            _myTeams.Remove(viewModel);

            return viewModel;
        }

        public TeamRoleViewModel RemoveTeamRole(uint roleId)
        {
            TeamRoleViewModel teamRole = _myTeams.SelectMany(t => t.TeamRoles).SingleOrDefault(r => r.Id == roleId);

            if (teamRole == null)
            {
                return null;
            }

            TeamViewModel team = _myTeams.SingleOrDefault(t => t.Id == teamRole.TeamId);

            team.TeamRoles.Remove(teamRole);

            return teamRole;
        }

        public ChannelViewModel RemoveChannel(uint channelId)
        {
            ChannelViewModel channelViewModel = _myTeams.SelectMany(team => team.Channels).SingleOrDefault(channel => channel.ChannelId == channelId);

            if (channelViewModel == null)
            {
                return null;
            }

            TeamViewModel teamViewModel = _myTeams.SingleOrDefault(team => team.Id == channelViewModel.TeamId);

            teamViewModel.Channels.Remove(channelViewModel);

            return channelViewModel;
        }

        public MemberViewModel RemoveMember(uint teamId, string userId)
        {
            TeamViewModel teamViewModel = _myTeams.Single(team => team.Id == teamId);

            if (teamViewModel == null)
            {
                return null;
            }

            MemberViewModel memberViewModel = teamViewModel.Members.Single(member => member.Id == userId);

            if (memberViewModel == null)
            {
                return null;
            }

            teamViewModel.Members.Remove(memberViewModel);

            return memberViewModel;
        }

        #endregion
    }
}
