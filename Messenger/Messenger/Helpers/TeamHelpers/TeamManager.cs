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
        private TeamViewModel _selectedTeam;
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

        public TeamViewModel SelectedTeam
        {
            get { return _selectedTeam; }
            set { Set(ref _selectedTeam, value); }
        }

        public void Clear()
        {
            _myTeams = new List<TeamViewModel>();
        }

        public void AddTeam(TeamViewModel viewModel)
        {
            if (!Validate(viewModel))
            {
                return;
            }

            _myTeams.Add(viewModel);
        }

        public void AddChat(PrivateChatViewModel viewModel)
        {
            if (!Validate(viewModel))
            {
                return;
            }

            _myChats.Add(viewModel);
        }

        public void AddTeam(IEnumerable<TeamViewModel> viewModels)
        {
            foreach (TeamViewModel viewModel in viewModels)
            {
                AddTeam(viewModel);
            }
        }

        public void AddChannel(ChannelViewModel channel)
        {
            foreach (TeamViewModel teamViewModel in _myTeams)
            {
                if (teamViewModel.Id == channel.TeamId)
                {
                    teamViewModel.Channels.Add(channel);
                }
            }
        }

        public void RemoveTeam(uint id)
        {
            IEnumerable<TeamViewModel> removed =
                _myTeams
                .TakeWhile((team) => team.Id != id);

            _myTeams = removed.ToList();
        }

        private bool Validate(TeamViewModel viewModel)
        {
            bool isValid = viewModel != null
                && viewModel.Id != null
                && viewModel.Members != null;

            return isValid;
        }
    }
}
