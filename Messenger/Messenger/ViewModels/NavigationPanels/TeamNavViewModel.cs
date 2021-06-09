using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Microsoft.UI.Xaml.Controls;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class TeamNavViewModel : Observable
    {
        #region Privates

        private ShellViewModel _shellViewModel;
        private ICommand _itemInvokedCommand;
        private Team _selectedTeam;
        private ObservableCollection<Team> _teams;

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        #endregion

        public ShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
            set
            {
                _shellViewModel = value;
                Set(ref _shellViewModel, value);
            }
        }

        public Team SelectedTeam
        {
            get
            {
                return _selectedTeam;
            }
            set
            {
                SelectedTeam = value;
                Set(ref _selectedTeam, value);
            }
        }

        public ObservableCollection<Team> Teams
        {
            get
            {
                return _teams;
            }
            set
            {
                _teams = value;
                Set(ref _teams, value);
            }
        }

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public TeamNavViewModel()
        {
            Initialize();

            
        }

        private async void Initialize() {
            Teams = new ObservableCollection<Team>();
            foreach (var team in ChatHubService.CurrentUser.Teams)
            {
                Teams.Add(team);
            }
        }

        private async Task LoadTeamsAsync() {
            var teams = await ChatHubService.GetTeamsList();

            // Add to the view
            Teams.Clear();
            foreach (var item in teams)
            {
                Teams.Add(item);
            }
        }

        public async void NewTeam(string name, string description) {
            await ChatHubService.CreateTeam(name, description);
            await LoadTeamsAsync();
        }

        private async void OnItemInvoked(TreeViewItemInvokedEventArgs args)
        {
            uint teamId = (args.InvokedItem as Team).Id;
            await ChatHubService.SwitchTeam(teamId);
        }
    }
}
