using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Commands;
using Messenger.Commands.TeamManage;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.ViewModels.Controls;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Messenger.ViewModels.Pages
{
    public class TeamManageViewModel : Observable
    {
        #region Privates

        private ObservableCollection<MemberViewModel> _membersView;

        private TeamViewModel _teamView;

        private List<MemberViewModel> _membersStore;

        private KeyEventHandler _removeSearchBoxInput;

        private KeyEventHandler _addSearchBoxInput;

        private User _selectedSearchedUser;

        private MemberViewModel _selectedMember;

        #endregion

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

        public MemberViewModel SelectedMember
        {
            get { return _selectedMember; }
            set { Set(ref _selectedMember, value); }
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

        public ICommand CreateChannelCommand => new CreateChannelCommand();

        public ICommand InviteUserCommand => new InviteUserCommand(this);

        public ICommand RemoveChannelClick => new RemoveChannelCommand();

        public ICommand NavigateBackCommand => new RelayCommand(() => NavigationService.Open<ChatPage>());

        public KeyEventHandler RemoveSearchInput => _removeSearchBoxInput ?? (_removeSearchBoxInput = new KeyEventHandler(RemoveSearchTextBoxKeyUp));

        public KeyEventHandler AddSearchInput => _addSearchBoxInput ?? (_addSearchBoxInput = new KeyEventHandler(AddSearchTextBoxKeyUp));

        public UserViewModel CurrentUser => App.StateProvider.CurrentUser;

        public MembersListControlViewModel MembersListControlViewModel { get; set; }

        public TeamManageViewModel()
        {
            MembersListControlViewModel = new MembersListControlViewModel(this);
        }

        private async Task<IReadOnlyList<string>> OnSearch(string keyword)
        {
            /** FILTER OUT MEMBERS **/
            return (await UserService.SearchUser(keyword))
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
        }

        private async Task<User> GetSelectedUser(string name, uint nameId)
        {
            return await UserService.GetUser(name, nameId);
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

            var usernames = await UserService.SearchUser(searchBox.Text);

            Members.Clear();
            foreach (var username in usernames)
            {
                Members.Add(new MemberViewModel() { Name = username });
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
