using Messenger.Core.Models;
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
        private UserViewModel _currentUser;

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

        public UserViewModel CurrentUser
        {
            get { return _currentUser; }
            set { Set(ref _currentUser, value); }
        }

        public TeamManager(TeamBuilder builder)
        {
            _builder = builder;
        }

        public static TeamManager CreateTeamManager()
        {
            TeamFactory factory = new TeamFactory();
            TeamBuilder builder = new TeamBuilder(factory);

            return new TeamManager(builder);
        }

        public void Clear()
        {
            _myTeams = new List<TeamViewModel>();
        }

        public TeamViewModel GetTeam(uint teamId)
        {
            return _myTeams
                .Where(team => team.Id == teamId)
                .FirstOrDefault();
        }

        public PrivateChatViewModel GetChat(uint chatId)
        {
            return _myChats
                .Where(chat => chat.Id == chatId)
                .FirstOrDefault();
        }

        public ChannelViewModel GetChannel(uint channelId)
        {
            foreach (TeamViewModel team in _myTeams)
            {
                foreach (ChannelViewModel channel in team.Channels)
                {
                    if (channel.ChannelId == channelId)
                    {
                        return channel;
                    }
                }
            }

            return null;
        }

        public async Task<TeamViewModel> AddTeam(Team teamData)
        {
            if (!Validate(teamData))
            {
                return null;
            }

            TeamViewModel viewModel = await _builder.Build(teamData, CurrentUser.Id);

            if (viewModel is PrivateChatViewModel)
            {
                PrivateChatViewModel chat = viewModel as PrivateChatViewModel;
                _myChats.Add(chat);

                return chat;
            }
            else
            {
                _myTeams.Add(viewModel);

                return viewModel;
            }
        }

        public async Task AddTeam(IEnumerable<Team> teamData)
        {
            foreach (Team data in teamData)
            {
                await AddTeam(data);
            }
        }

        public ChannelViewModel AddChannel(Channel channelData)
        {
            foreach (TeamViewModel teamViewModel in _myTeams)
            {
                if (teamViewModel.Id == channelData.TeamId)
                {
                    ChannelViewModel viewModel = _builder.Map(channelData);

                    teamViewModel.Channels.Add(viewModel);

                    return viewModel;
                }
            }

            return null;
        }

        public void RemoveTeam(uint id)
        {
            IEnumerable<TeamViewModel> removed =
                _myTeams
                .TakeWhile((team) => team.Id != id);

            _myTeams = removed.ToList();
        }

        private bool Validate(Team teamData)
        {
            bool isValid = teamData != null
                && teamData.Id > 0;

            return isValid;
        }
    }
}
