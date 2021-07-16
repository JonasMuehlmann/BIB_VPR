using Messenger.Core.Models;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System.Collections.ObjectModel;

namespace Messenger.Helpers.TeamHelpers
{
    public class TeamFactory
    {
        private readonly Team _data;

        private TeamViewModel _baseViewModel;

        public TeamFactory(Team data)
        {
            _data = data;
        }

        public TeamViewModel CreateBaseViewModel()
        {
            _baseViewModel = new TeamViewModel()
            {
                Id = _data.Id,
                TeamName = _data.Name,
                Description = _data.Description,
                CreationDate = _data.CreationDate,
                Members = new ObservableCollection<Member>(),
                Channels = new ObservableCollection<ChannelViewModel>()
            };

            return _baseViewModel;
        }

        public TeamViewModel GetViewModel(TeamViewModel viewModel)
        {
            if (string.IsNullOrEmpty(viewModel.TeamName))
            {
                return new PrivateChatViewModel(viewModel);
            }
            else
            {
                return viewModel;
            }
        }
    }
}
