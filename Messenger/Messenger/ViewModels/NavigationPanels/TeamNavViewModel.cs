using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public IEnumerable<Team> _teams;


        private TeamService TeamService => Singleton<TeamService>.Instance;
        private UserService UserService => Singleton<UserService>.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;



        public ShellViewModel ShellViewModel { get {
                return _shellViewModel;
            }
            set {
                _shellViewModel = value;
                _shellViewModel.ChatName = "Chatname";
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

        public IEnumerable<Team> Teams
        {
            get => _teams;
            set => Set(ref _teams, value);
        }

        public ICommand ItemInvokedCommand => _itemInvokedCommand ?? (_itemInvokedCommand = new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(OnItemInvoked));

        public TeamNavViewModel()
        {
            
            InitAsync();
            
            //User = new User();
            //User.Teams = new List<Team>();
            //User.Teams.Add(new Team() { Name = "T1", Channels = new List<TeamChannel>()});
            //User.Teams[0].Channels.Add(new TeamChannel() { ChannelName = "C1" });
            //User.Teams[0].Channels.Add(new TeamChannel() { ChannelName = "C2" });
        }


        private void OnItemInvoked(WinUI.TreeViewItemInvokedEventArgs args)
            => SelectedItem = args.InvokedItem;

        private async void InitAsync() {
            User = await UserDataService.GetUserAsync();
            await UserService.GetOrCreateApplicationUser(User.ToUserObject());

            //uint? tid = await TeamService.CreateTeam("test 01");
            //if(tid != null)
            //{
            //    await TeamService.AddMember(User.Id, tid.Value);
            //}

            //tid = await TeamService.CreateTeam("test 02");
            //if(tid != null)
            //{
            //    await TeamService.AddMember(User.Id, tid.Value);
            //}

            //tid = await TeamService.CreateTeam("test 02");
            //if(tid != null)
            //{
            //    await TeamService.AddMember(User.Id, tid.Value);
            //}
            

            LoadTeams();
        }

        //TODO user and userviewmodel impl fix
        private async void LoadTeams() {
            Teams = await TeamService.GetAllTeamsByUserId(User.Id);
            Console.WriteLine(string.Join(",", Teams));
        }

        public async void NewTeam(string name) {
            uint? tid = await TeamService.CreateTeam(name);
            if(tid != null)
            {
                await TeamService.AddMember(User.Id, tid.Value);
                LoadTeams();
            }
        }

    }
}
