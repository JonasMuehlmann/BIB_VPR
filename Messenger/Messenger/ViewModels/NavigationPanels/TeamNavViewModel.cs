using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class TeamNavViewModel : Observable
    {
        private ShellViewModel _shellViewModel;
        private ICommand _itemInvokedCommand;
        private object _selectedItem;
        private UserViewModel _user;

        private ObservableCollection<Team> _teams;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;

        public ShellViewModel ShellViewModel { get {
                return _shellViewModel;
            }
            set {
                _shellViewModel = value;
            }
        }

        public object SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        public UserViewModel User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public ObservableCollection<Team> Teams
        {
            get => _teams;
            set => Set(ref _teams, value);
        }

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public TeamNavViewModel()
        {
            InitAsync();
        }

        private void OnItemInvoked(WinUI.TreeViewItemInvokedEventArgs args)
            => SelectedItem = args.InvokedItem;

        private async void InitAsync() {
            Teams = new ObservableCollection<Team>();
            User = await UserDataService.GetUserAsync();
            LoadTeams();
        }

        private async void LoadTeams() {
            // Load all teams with the current user id
            var teams = await MessengerService.LoadTeams(User.Id);

            // Add to the view
            Teams.Clear();
            foreach (var item in teams)
            {
                Teams.Add(item);
            }
        }

        public async void NewTeam(string name) {
            await MessengerService.CreateTeam(User.Id, name);
            LoadTeams();
        }
    }
}
