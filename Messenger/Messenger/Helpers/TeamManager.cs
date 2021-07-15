using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Helpers
{
    public class TeamManager : Observable
    {
        private TeamViewModel _selectedTeam;
        private List<TeamViewModel> myTeams = new List<TeamViewModel>();

        public ReadOnlyCollection<TeamViewModel> MyTeams
        {
            get
            {
                return myTeams.AsReadOnly();
            }
        }

        public TeamViewModel SelectedTeam
        {
            get { return _selectedTeam; }
            set { Set(ref _selectedTeam, value); }
        }

        public void Clear()
        {
            myTeams = new List<TeamViewModel>();
        }

        public void AddTeam(TeamViewModel viewModel)
        {
            if (!Validate(viewModel))
            {
                return;
            }

            myTeams.Add(viewModel);
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
            foreach (TeamViewModel teamViewModel in myTeams)
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
                myTeams
                .TakeWhile((team) => team.Id != id);

            myTeams = removed.ToList();
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
