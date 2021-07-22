using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Commands.Messenger;
using Messenger.Commands.TeamManage;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using Messenger.Views.DialogBoxes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class TeamManageViewModel : Observable
    {

        #region Privates

        private ShellViewModel _shellViewModel;
        private ObservableCollection<MemberViewModel> _membersView;
        private TeamViewModel _teamView;
        private List<MemberViewModel> _membersStore;
        private ICommand _removeTeamMembers;
        private ICommand _addTeamMembers;
        private ICommand _channelManagement;
        private ICommand _createChannel;
        private ICommand _removeTeamMemberClick;
        private ICommand _removeChannelClick;
        private ICommand _addTeamMemberClick; 
        private KeyEventHandler _removeSearchBoxInput;
        private KeyEventHandler _addSearchBoxInput;

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        #endregion
        private User _selectedSearchedUser;

        public ShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
            set
            {
                _shellViewModel = value;
            }
        }

        public ObservableCollection<MemberViewModel> Members
        {
            get
            {
                return _membersView;
            }
            set
            {
                //TODO add UserNull handling
                if (value.Count == 0 || value == null)
                {
                    value.Add(new MemberViewModel() { Name = "Keine Nutzer gefunden" });
                }
                Set(ref _membersView, value);
            }
        }

        public TeamViewModel CurrentTeam
        {
            get
            {
                return _teamView;
            }
            set
            {
                Set(ref _teamView, value);
            }
        }

        #region Search Users

        public User SelectedSearchedUser
        {
            get
            {
                return _selectedSearchedUser;
            }
            set
            {
                Set(ref _selectedSearchedUser, value);
            }
        }

        public Func<string, Task<IReadOnlyList<string>>> OnSearchFunction
        {
            get
            {
                return OnSearch;
            }
        }

        public Func<string, uint, Task<User>> GetSelectedUserFunction
        {
            get
            {
                return GetSelectedUser;
            }
        }

        #endregion

        public ICommand OpenManageRolesCommand => new OpenManageRolesCommand();

        public ICommand InviteUserCommand => new InviteUserCommand(this, ChatHubService);
        public ICommand RemoveUserCommand => new RemoveUserCommand(this, ChatHubService);
        public ICommand NavigateBackCommand => new RelayCommand(() => NavigationService.Open<ChatPage>());
        public ICommand CreateChannelCommand => _createChannel ?? (_createChannel = new RelayCommand(CreateChannel));
        public ICommand RemoveTeamMemberClick => _removeTeamMemberClick ?? (_removeTeamMemberClick = new RelayCommand<string>(RemoveUserAsync));
        public ICommand RemoveChannelClick => _removeChannelClick ?? (_removeChannelClick = new RelayCommand<uint>(RemoveChannelAsync));
        public KeyEventHandler RemoveSearchInput => _removeSearchBoxInput ?? (_removeSearchBoxInput = new KeyEventHandler(RemoveSearchTextBoxKeyUp));
        public KeyEventHandler AddSearchInput => _addSearchBoxInput ?? (_addSearchBoxInput = new KeyEventHandler(AddSearchTextBoxKeyUp));

        public UserViewModel CurrentUser => App.StateProvider.CurrentUser;

        public TeamManageViewModel()
        {
            Members = new ObservableCollection<MemberViewModel>();
            _membersStore = new List<MemberViewModel>();

            LoadTeamMember();
        }

        /// <summary>
        /// The method is responsible for loading all members of the team from the database
        /// </summary>
        private void LoadTeamMember() {
            IEnumerable<MemberViewModel> members = App.StateProvider.SelectedTeam.Members;

            _membersStore.Clear();

            foreach (var user in members) {
                _membersStore.Add(user);
            }

            Members = new ObservableCollection<MemberViewModel>(CopyList(_membersStore));
        }

        private async Task<IReadOnlyList<string>> OnSearch(string keyword)
        {
            IEnumerable<string> result = await ChatHubService.SearchUser(keyword);

            var filtered = result
                .TakeWhile(user =>
                {
                    TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

                    var data = user.Split('#');
                    string name = data[0];
                    uint nameId = Convert.ToUInt32(data[1]);

                    bool isMember = selectedTeam.Members
                        .Any(member => member.Name == name && member.NameId == nameId);

                    return !isMember;
                }).ToList();

            return filtered;
        }

        private async Task<User> GetSelectedUser(string name, uint nameId)
        {
            User result = await ChatHubService.GetUserWithNameId(name, nameId);

            return result;
        }

        /// <summary>
        /// Removes some user from Team
        /// </summary>
        /// <param name="userId"></param>
        private async void RemoveUserAsync(string userId)
        {
            await ChatHubService.RemoveUser(userId, App.StateProvider.SelectedTeam.Id);
            LoadTeamMember();
        }


        /// <summary>
        /// Removes channels
        /// </summary>
        /// <param name="channelId"></param>
        private async void RemoveChannelAsync(uint channelId)
        {
            await ChatHubService.RemoveChannel(channelId);
        }

        /// <summary>
        /// Is Triggered when a user enters something in the user search box to get some member. It searches for the Members than
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSearchTextBoxKeyUp(object sender, KeyRoutedEventArgs e) {
            TextBox searchBox = sender as TextBox;

            if (searchBox.Text == "")
            {
                Members = CopyList(_membersStore);
            }
            else {
                Members = CopyList(_membersStore.FindAll(x => x.Name.Contains(searchBox.Text)));
            }
        }


        /// <summary>
        /// Is Triggered when a user enters something in the user search box to find some user. It searches for Users than
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddSearchTextBoxKeyUp(object sender, KeyRoutedEventArgs e) {
            TextBox searchBox = sender as TextBox;

            Members.Clear();
            var usernames = await ChatHubService.SearchUser(searchBox.Text);
            foreach (var username in usernames)
            {
                Members.Add(new MemberViewModel() { Name = username });
            }
        }


        private async void CreateChannel() {
            if (CurrentUser == null)
            {
                return;
            }

            // Opens the dialog box for the input
            var dialog = new CreateChannelDialog();
            ContentDialogResult result = await dialog.ShowAsync();

            // Create team on confirm
            if (result == ContentDialogResult.Primary)
            {
               await ChatHubService.CreateChannel(dialog.ChannelName);

                await ResultConfirmationDialog
                    .Set(true, $"You created a new channel {dialog.ChannelName}")
                    .ShowAsync();
            }
        }

        #region Helpers

        /// <summary>
        /// The method copies the stored MemberList to the ViewMemberlist to safe data traffic, when searching the users
        /// </summary>
        /// <param name="memberList"></param>
        /// <returns>a copy of the given list on input</returns>
        private ObservableCollection<MemberViewModel> CopyList(List<MemberViewModel> memberList) {
            ObservableCollection<MemberViewModel> memb = new ObservableCollection<MemberViewModel>();
            foreach (var user in memberList)
            {
                memb.Add(user);
            }
            return memb;
        }
        #endregion
    }
}
