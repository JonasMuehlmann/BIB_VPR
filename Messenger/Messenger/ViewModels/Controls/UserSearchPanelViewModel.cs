using Messenger.Commands;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Messenger.ViewModels.Controls
{
    public class UserSearchPanelViewModel : Observable
    {
        private Func<string, bool> _searchFilter;

        public Func<string, bool> SearchFilter
        {
            get { return _searchFilter; }
            set { Set(ref _searchFilter, value); }
        }

        private User _selectedUser;

        public User SelectedUser
        {
            get { return _selectedUser; }
            set { Set(ref _selectedUser, value); }
        }

        private ObservableCollection<string> _searchResults;

        public ObservableCollection<string> SearchResults
        {
            get { return _searchResults; }
            set { Set(ref _searchResults, value); }
        }

        public ICommand SearchCommand { get => new RelayCommand<string>((term) => Search(term)); }

        public ICommand GetSelectedUserCommand { get => new RelayCommand<string>((searchResult) => GetSelectedUser(searchResult
            )); }

        public UserSearchPanelViewModel(Func<string, bool> searchFilter)
        {
            SearchResults = new ObservableCollection<string>();

            SearchFilter = searchFilter;
        }

        private async void Search(string keyword)
        {
            /** FILTER OUT MEMBERS **/
            IList<string> results = await UserService.SearchUser(keyword);

            if (results == null || results.Count < 0) return;

            if (SearchFilter == null)
            {
                SearchResults.Clear();

                foreach (string result in results)
                {
                    SearchResults.Add(result);
                }
            }
            else
            {
                SearchResults.Clear();

                foreach (string result in results.TakeWhile(SearchFilter))
                {
                    SearchResults.Add(result);
                }
            }
        }

        private async void GetSelectedUser(string searchResult)
        {
            string[] data = searchResult.Split('#');

            string name = data[0];
            uint nameId = Convert.ToUInt32(data[1]);

            User user = await UserService.GetUser(name, nameId);

            if (user == null) return;

            SelectedUser = user;
        }
    }
}
