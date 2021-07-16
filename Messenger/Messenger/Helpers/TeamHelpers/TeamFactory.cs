using Messenger.Core.Models;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System.Collections.ObjectModel;

namespace Messenger.Helpers.TeamHelpers
{
    public class TeamFactory
    {
        public TeamViewModel CreateBaseViewModel(Team data)
        {
            return new TeamViewModel()
            {
                Id = data.Id,
                TeamName = data.Name,
                Description = data.Description,
                CreationDate = data.CreationDate,
                Members = new ObservableCollection<Member>(),
                Channels = new ObservableCollection<ChannelViewModel>()
            };
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
