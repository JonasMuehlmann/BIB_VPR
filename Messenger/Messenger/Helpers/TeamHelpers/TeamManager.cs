using Messenger.Core.Models;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Helpers.TeamHelpers
{
    public class TeamManager : Observable
    {
        private readonly TeamBuilder _builder;
        private List<TeamViewModel> _myTeams = new List<TeamViewModel>();
        private List<PrivateChatViewModel> _myChats = new List<PrivateChatViewModel>();

        public ReadOnlyCollection<TeamViewModel> MyTeams
        {
            get
            {
                return _myTeams.AsReadOnly();
            }
        }

        public ReadOnlyCollection<PrivateChatViewModel> MyChats
        {
            get
            {
                return _myChats.AsReadOnly();
            }
        }

        public TeamManager()
        {
            _builder = new TeamBuilder();
            Clear();
        }

        public void Clear()
        {
            _myTeams = new List<TeamViewModel>();
            _myChats = new List<PrivateChatViewModel>();
        }

        public async Task<TeamViewModel> AddOrUpdateTeam(Team teamData)
        {
            dynamic viewModel = await _builder.Build(teamData, App.StateProvider.CurrentUser.Id);

            if (viewModel is PrivateChatViewModel)
            {
                PrivateChatViewModel chatViewModel = viewModel as PrivateChatViewModel;

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
                    ChannelViewModel viewModel = _builder.Map(channelData);

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
                    MemberViewModel member = _builder.Map(userData);
                    IList<MemberRole> memberRoles = await _builder.WithMemberRoles(teamId, member);

                    if (memberRoles != null && memberRoles.Count > 0)
                    {
                        foreach (MemberRole role in memberRoles)
                        {
                            member.MemberRoles.Add(role);
                        }
                    }

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

        public TeamViewModel RemoveTeam(uint teamId)
        {
            TeamViewModel viewModel = _myTeams.Single(team => team.Id == teamId);

            if (viewModel == null)
            {
                return null;
            }

            _myTeams.Remove(viewModel);

            return viewModel;
        }

        public ChannelViewModel RemoveChannel(uint channelId)
        {
            ChannelViewModel channelViewModel = _myTeams.SelectMany(team => team.Channels).SingleOrDefault(channel => channel.ChannelId == channelId);

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
    }
}
