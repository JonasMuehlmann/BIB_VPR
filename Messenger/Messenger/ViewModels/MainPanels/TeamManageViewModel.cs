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
        private KeyEventHandler _searchBoxInput;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;
        private TeamService TeamService => Singleton<TeamService>.Instance;
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
                Set(ref _membersView, value);
            }
        }

        public ICommand RemoveTeamMembers => _removeTeamMembers ?? (_removeTeamMembers = new RelayCommand(LoadTeamMembersAsync));
        public KeyEventHandler SearchInput => _searchBoxInput ?? (_searchBoxInput = new KeyEventHandler(SearchTextBoxKeyUp));

        public UserViewModel CurrentUser => ChatHubService.CurrentUser;

        public TeamManageViewModel()
        {
            Members = new ObservableCollection<User>();
            _membersStore = new List<User>();
            UserDataService.UserDataUpdated += OnUserDataUpdated;
        }




        /// <summary>
        /// Fires on UserDataUpdated(mostly once on log-in) and refreshes the view
        /// This prevents the view model to read from default user, which is not wanted here
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="user">UserViewModel of the current user</param>
        private void OnUserDataUpdated(object sender, UserViewModel user)
        {
        }

        /// <summary>
        /// The method is responsible for loading all members of the team from the database
        /// </summary>
        private async void LoadTeamMembersAsync() {
            if (ChatHubService.CurrentTeamId != null)
            {
                IEnumerable<User> members = await TeamService.GetAllMembers((uint)ChatHubService.CurrentTeamId);

                Members.Clear();
                _membersStore.Clear();

                foreach (var user in members) {
                    _membersStore.Add(user);
                    Members.Add(user);
                }
            }
        }


        //TODO add UserNull handling

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
    }
}
