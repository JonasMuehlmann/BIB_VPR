using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class TeamManageViewModel : Observable
    {

        #region Privates
        private ShellViewModel _shellViewModel;
        private ObservableCollection<User> _membersView;
        private List<User> _membersStore;
        private ICommand _removeTeamMembers;
        private ICommand _removeTeamMemberClick;
        private KeyEventHandler _searchBoxInput;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
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
            }
        }

        public ObservableCollection<User> Members
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
                    value.Add(new User() { DisplayName = "Keine Nutzer gefunden" });
                }
                Set(ref _membersView, value);
            }
        }

        public ICommand RemoveTeamMembers => _removeTeamMembers ?? (_removeTeamMembers = new RelayCommand(LoadTeamMembersAsync));
        public ICommand RemoveTeamMemberClick => _removeTeamMemberClick ?? (_removeTeamMemberClick = new RelayCommand<string>(RemoveUserAsync));
        public KeyEventHandler SearchInput => _searchBoxInput ?? (_searchBoxInput = new KeyEventHandler(SearchTextBoxKeyUp));

        public UserViewModel CurrentUser => ChatHubService.CurrentUser;

        public TeamManageViewModel()
        {
            Members = new ObservableCollection<User>();
            _membersStore = new List<User>();
        }

        /// <summary>
        /// The method is responsible for loading all members of the team from the database
        /// </summary>
        private async void LoadTeamMembersAsync() {
            if (ChatHubService.CurrentTeamId != null)
            {
                IEnumerable<User> members = await ChatHubService.GetTeamMembers((uint)ChatHubService.CurrentTeamId);

                _membersStore.Clear();

                foreach (var user in members) {
                    _membersStore.Add(user);
                }
                Members = CopyList(_membersStore);
            }
        }

        /// <summary>
        /// Removes some user from Team
        /// </summary>
        /// <param name="userId"></param>
        private async void RemoveUserAsync(string userId)
        {
            if (ChatHubService.CurrentTeamId != null)
            {
                await ChatHubService.RemoveUser(userId, (uint)ChatHubService.CurrentTeamId);
                LoadTeamMembersAsync();
            }
        }


        /// <summary>
        /// The method ist Triggered when a user enters something in the user search box. It searches for the Members than
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchTextBoxKeyUp(object sender, KeyRoutedEventArgs e) {
            TextBox searchBox = sender as TextBox;

            if (searchBox.Text == "")
            {
                Members = CopyList(_membersStore);
            }
            else {
                Members = CopyList(_membersStore.FindAll(x => x.DisplayName.Contains(searchBox.Text)));
            }
        }

        #region Helpers

        /// <summary>
        /// The method copies the stored MemberList to the ViewMemberlist to safe data traffic, when searching the users
        /// </summary>
        /// <param name="memberList"></param>
        /// <returns></returns>
        private ObservableCollection<User> CopyList(List<User> memberList) {
            ObservableCollection<User> memb = new ObservableCollection<User>();
            foreach (var user in memberList)
            {
                memb.Add(user);
            }
            return memb;
        }
        #endregion
    }
}
