﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
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
        private ObservableCollection<User> _membersView;
        private ObservableCollection<Channel> _channelsView;
        private List<User> _membersStore;
        private ICommand _removeTeamMembers;
        private ICommand _addTeamMembers;
        private ICommand _channelManagement;
        private ICommand _createChannel;
        private ICommand _removeTeamMemberClick;
        private ICommand _addTeamMemberClick; 
        private KeyEventHandler _removeSearchBoxInput;
        private KeyEventHandler _addSearchBoxInput;

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

        public ObservableCollection<Channel> Channels
        {
            get
            {
                return _channelsView;
            }
            set
            {
                //TODO add UserNull handling
                if (value.Count == 0 || value == null)
                {
                    value.Add(new Channel() { ChannelName = "Keine Channels gefunden" });
                }
                Set(ref _channelsView, value);
            }
        }

        public ICommand RemoveTeamMembers => _removeTeamMembers ?? (_removeTeamMembers = new RelayCommand(LoadTeamMembersAsync));
        public ICommand AddTeamMembers => _addTeamMembers ?? (_addTeamMembers = new RelayCommand(InitAddTeamMembers));
        public ICommand ChannelManagement => _channelManagement ?? (_channelManagement = new RelayCommand(InitChannels));
        public ICommand CreateChannelCommand => _createChannel ?? (_createChannel = new RelayCommand(CreateChannel));
        public ICommand RemoveTeamMemberClick => _removeTeamMemberClick ?? (_removeTeamMemberClick = new RelayCommand<string>(RemoveUserAsync));
        public ICommand AddTeamMemberClick => _addTeamMemberClick ?? (_addTeamMemberClick = new RelayCommand<string>(AddUserAsync));
        public KeyEventHandler RemoveSearchInput => _removeSearchBoxInput ?? (_removeSearchBoxInput = new KeyEventHandler(RemoveSearchTextBoxKeyUp));
        public KeyEventHandler AddSearchInput => _addSearchBoxInput ?? (_addSearchBoxInput = new KeyEventHandler(AddSearchTextBoxKeyUp));

        public UserViewModel CurrentUser => ChatHubService.CurrentUser;

        public TeamManageViewModel()
        {
            Members = new ObservableCollection<User>();
            _membersStore = new List<User>();
            Channels = new ObservableCollection<Channel>();
            ChatHubService.ChannelsUpdated += OnChannelsUpdated;
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
        /// Clears the Member list if there was something in from removeUser tab
        /// </summary>
        private void InitAddTeamMembers() {
            if (ChatHubService.CurrentTeamId != null)
            {
                _membersStore.Clear();
                Members = CopyList(_membersStore);
            }
        }

        /// <summary>
        /// inits the channels view
        /// </summary>
        private async void InitChannels()
        {
            if (ChatHubService.CurrentTeamId != null)
            {
                _membersStore.Clear();
                FilterAndUpdateChannels(await ChatHubService.GetChannelsList());
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
        /// Invited an selected User to the Team
        /// </summary>
        /// <param name="username"></param>
        private async void AddUserAsync(string username)
        {
            var usernameSp = username.Split("#");
            if (ChatHubService.CurrentTeamId != null)
            {
                User user = await ChatHubService.GetUser(usernameSp[0], Convert.ToUInt32(usernameSp[1]));
                if (user != null)
                {
                    await ChatHubService.InviteUser(new Models.Invitation(user.Id, (uint)ChatHubService.CurrentTeamId));
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
                Members = CopyList(_membersStore.FindAll(x => x.DisplayName.Contains(searchBox.Text)));
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
                Members.Add(new User() {DisplayName = username});
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
        private void OnChannelsUpdated(object sender, IEnumerable<Channel> channels)
        {
            if (ChatHubService.CurrentUser.Teams != null)
            {
                FilterAndUpdateChannels(channels);
            }
        }

        #region Helpers

        /// <summary>
        /// The method copies the stored MemberList to the ViewMemberlist to safe data traffic, when searching the users
        /// </summary>
        /// <param name="memberList"></param>
        /// <returns>a copy of the given list on input</returns>
        private ObservableCollection<User> CopyList(List<User> memberList) {
            ObservableCollection<User> memb = new ObservableCollection<User>();
            foreach (var user in memberList)
            {
                memb.Add(user);
            }
            return memb;
        }

        private void FilterAndUpdateChannels(IEnumerable<Channel> channels)
        {
            if (channels != null)
            {
                Channels.Clear();

                foreach (var channel in channels)
                {
                    Channels.Add(channel);
                }
            }
        }

        #endregion
    }
}
