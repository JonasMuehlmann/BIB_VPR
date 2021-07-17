using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
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
        private ObservableCollection<Member> _membersView;
        private TeamViewModel _teamView;
        private List<Member> _membersStore;
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

        public ObservableCollection<Member> Members
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
                    value.Add(new Member() { Name = "Keine Nutzer gefunden" });
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

        public ICommand InviteUserCommand => new InviteUserCommand(ChatHubService);
        public ICommand RemoveUserCommand => new RemoveUserCommand(ChatHubService);
        public ICommand CreateChannelCommand => _createChannel ?? (_createChannel = new RelayCommand(CreateChannel));
        public ICommand RemoveTeamMemberClick => _removeTeamMemberClick ?? (_removeTeamMemberClick = new RelayCommand<string>(RemoveUserAsync));
        public ICommand RemoveChannelClick => _removeChannelClick ?? (_removeChannelClick = new RelayCommand<uint>(RemoveChannelAsync));
        public ICommand AddTeamMemberClick => _addTeamMemberClick ?? (_addTeamMemberClick = new RelayCommand<string>(AddUserAsync));
        public KeyEventHandler RemoveSearchInput => _removeSearchBoxInput ?? (_removeSearchBoxInput = new KeyEventHandler(RemoveSearchTextBoxKeyUp));
        public KeyEventHandler AddSearchInput => _addSearchBoxInput ?? (_addSearchBoxInput = new KeyEventHandler(AddSearchTextBoxKeyUp));

        public UserViewModel CurrentUser => ChatHubService.CurrentUser;

        public TeamManageViewModel()
        {
            Members = new ObservableCollection<Member>();
            _membersStore = new List<Member>();

            ChatHubService.TeamsUpdated += OnTeamsUpdated;
            CurrentTeam = ChatHubService.SelectedTeam;

            LoadTeamMember();
        }

        /// <summary>
        /// The method is responsible for loading all members of the team from the database
        /// </summary>
        private void LoadTeamMember() {
            IEnumerable<Member> members = ChatHubService.SelectedTeam.Members;

            _membersStore.Clear();

            foreach (var user in members) {
                _membersStore.Add(user);
            }

            Members = new ObservableCollection<Member>(CopyList(_membersStore));
        }

        private async Task<IReadOnlyList<string>> OnSearch(string keyword)
        {
            IEnumerable<string> result = await ChatHubService.SearchUser(keyword);

            return result
                .TakeWhile(user =>
                {
                    TeamViewModel selectedTeam = ChatHubService.SelectedTeam;
                    var data = user.Split('#');
                    string name = data[0];
                    uint nameId = Convert.ToUInt32(data[1]);

                    bool isMember = selectedTeam.Members
                        .Any(member => member.Name == name && member.NameId == nameId);

                    return !isMember;
                }).ToList();
        }

        private async Task<User> GetSelectedUser(string name, uint nameId)
        {
            User result = await ChatHubService.GetUserWithNameId(name, nameId);

            return result;
        }

        /// <summary>
        /// Clears the Member list if there was something in from removeUser tab
        /// </summary>
        private void InitAddTeamMembers() {
            if (ChatHubService.SelectedTeam.Id != null)
            {
                _membersStore.Clear();
                Members = CopyList(_membersStore);
            }
        }

        /// <summary>
        /// inits the channels view
        /// </summary>
        private void InitChannels()
        {
            if (ChatHubService.SelectedTeam.Id != null)
            {
                _membersStore.Clear();
                CurrentTeam = ChatHubService.SelectedTeam;
            }
        }

        /// <summary>
        /// Removes some user from Team
        /// </summary>
        /// <param name="userId"></param>
        private async void RemoveUserAsync(string userId)
        {
            if (ChatHubService.SelectedTeam.Id != null)
            {
                await ChatHubService.RemoveUser(userId, (uint)ChatHubService.SelectedTeam.Id);
                LoadTeamMember();
            }
        }


        /// <summary>
        /// Removes channels
        /// </summary>
        /// <param name="channelId"></param>
        private async void RemoveChannelAsync(uint channelId)
        {
            if (ChatHubService.SelectedTeam.Id != null)
            {
                await ChatHubService.RemoveChannel(channelId);
            }
        }

        /// <summary>
        /// Invited an selected User to the Team
        /// </summary>
        /// <param name="username"></param>
        private async void AddUserAsync(string username)
        {
            var usernameSp = username.Split("#");
            if (ChatHubService.SelectedTeam.Id != null)
            {
                User user = await ChatHubService.GetUser(usernameSp[0], Convert.ToUInt32(usernameSp[1]));
                if (user != null)
                {
                    await ChatHubService.InviteUser(new Invitation()
                    {
                        UserId = user.Id,
                        TeamId = (uint)ChatHubService.SelectedTeam.Id
                    });
                    InitAddTeamMembers();
                }
            }
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
                Members.Add(new Member() { Name = username });
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

        /// <summary>
        /// refactors the channel list when channels are updated
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="channels"></param>
        private void OnTeamsUpdated(object sender, IEnumerable<TeamViewModel> teams)
        {
            if (teams != null)
            {
                CurrentTeam = ChatHubService.SelectedTeam;
            }
        }

        #region Helpers

        /// <summary>
        /// The method copies the stored MemberList to the ViewMemberlist to safe data traffic, when searching the users
        /// </summary>
        /// <param name="memberList"></param>
        /// <returns>a copy of the given list on input</returns>
        private ObservableCollection<Member> CopyList(List<Member> memberList) {
            ObservableCollection<Member> memb = new ObservableCollection<Member>();
            foreach (var user in memberList)
            {
                memb.Add(user);
            }
            return memb;
        }
        #endregion
    }
}
