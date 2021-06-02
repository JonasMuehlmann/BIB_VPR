﻿using System;
using Messenger.Core.Helpers;
using Messenger.Helpers;
using Messenger.Services;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        private ShellViewModel _shellViewModel;

        public ShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
            set
            {
                _shellViewModel = value;
                _shellViewModel.ChatName = "ChatName";
            }
        }

        public ChatHubViewModel Hub => Singleton<ChatHubViewModel>.Instance;

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        private UserViewModel _user;

        public UserViewModel User
        {
            get { return _user; }
            set { _user = value; }
        }

        public ChatViewModel()
        {
            LoadAsync();
        }

        public async void LoadAsync()
        {
            User = await UserDataService.GetUserAsync();
        }
    }
}
