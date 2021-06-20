﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Views.DialogBoxes;
using Windows.UI.Xaml.Controls;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ChatNavViewModel : Observable
    {
        private ObservableCollection<PrivateChat> _chats;
        private bool _isBusy;

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        public ObservableCollection<PrivateChat> Chats
        {
            get
            {
                return _chats;
            }
            set
            {
                Set(ref _chats, value);
            }
        }

        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                Set(ref _isBusy, value);
            }
        }

        public ICommand SwitchChatCommand => new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(SwitchChat);

        public ICommand StartChatCommand => new RelayCommand(StartChatAsync);

        public ChatNavViewModel()
        {
            IsBusy = true;

            Chats = new ObservableCollection<PrivateChat>();
            ChatHubService.TeamsUpdated += OnTeamsUpdated;
            LoadAsync();
        }

        private async void LoadAsync()
        {
            var user = await UserDataService.GetUserAsync();

            if (user.Teams != null && user.Teams.Count > 0)
            {
                FilterAndUpdateChats(user.Teams);
            }

            IsBusy = false;
        }

        public async void StartChatAsync()
        {
            var dialog = new CreateChatDialog();
            dialog.OnSearch = SearchUsers;
            ContentDialogResult result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var selectedUser = dialog.SelectedUser;

                await ChatHubService.StartChat(selectedUser.DisplayName, selectedUser.NameId);
            }
        }

        private async Task<IList<string>> SearchUsers(string username)
        {
            var userStrings = await ChatHubService.SearchUser(username);

            return userStrings;
        }

        /// <summary>
        /// Fires on TeamsUpdated in ChatHubService and refreshes the view
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="teams">Enumerable of teams</param>
        private void OnTeamsUpdated(object sender, IEnumerable<Team> teams)
        {
            if (teams != null)
            {
                FilterAndUpdateChats(teams);
            }

            IsBusy = false;
        }

        /// <summary>
        /// Fires on click and invokes ChatHubService to load messages of the selected team
        /// </summary>
        /// <param name="args">Event argument from the event, contains the data of the invoked item</param>
        private async void SwitchChat(WinUI.TreeViewItemInvokedEventArgs args)
        {
            uint teamId = (args.InvokedItem as Team).Id;

            // Invokes TeamSwitched event
            await ChatHubService.SwitchTeam(teamId);
        }

        private void FilterAndUpdateChats(IEnumerable<Team> teams)
        {
            if (teams != null)
            {
                var chatsList = teams
                    .Where(team => string.IsNullOrEmpty(team.Name))
                    .Select(data => PrivateChat.CreatePrivateChatFromTeamData(data));

                Chats.Clear();
                foreach (var chat in chatsList)
                {
                    Chats.Add(chat);
                }
            }
        }
    }
}
