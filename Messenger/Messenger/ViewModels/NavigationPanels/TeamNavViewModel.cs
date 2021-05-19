using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class TeamNavViewModel : Observable
    {
        private ShellViewModel _shellViewModel;
        private ICommand _itemInvokedCommand;
        private object _selectedItem;
        private User _user;
        public ShellViewModel ShellViewModel { get {
                return _shellViewModel;
            }
            set {
                _shellViewModel = value;
                _shellViewModel.ChatName = "Maoin";
            }
        }

        public object SelectedItem
        {
            get => _selectedItem;
            set => Set(ref _selectedItem, value);
        }

        public User User
        {
            get => _user;
            set => Set(ref _user, value);
        }

        public ObservableCollection<SampleCompany> SampleItems { get; } = new ObservableCollection<SampleCompany>();

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public TeamNavViewModel()
        {
            User = new User();
            User.Teams = new List<Team>();
            User.Teams.Add(new Team() { Name = "T1", Channels = new List<TeamChannel>()});
            User.Teams[0].Channels.Add(new TeamChannel() { ChannelName = "C1" });
            User.Teams[0].Channels.Add(new TeamChannel() { ChannelName = "C2" });
        }


        public async Task LoadDataAsync()
        {
            var data = await SampleDataService.GetTreeViewDataAsync();
            foreach (var item in data)
            {
                SampleItems.Add(item);
            }
        }

        private void OnItemInvoked(WinUI.TreeViewItemInvokedEventArgs args)
            => SelectedItem = args.InvokedItem;
    }
}
