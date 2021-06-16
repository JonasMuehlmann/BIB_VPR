using System;
using System.Collections.Generic;
using Messenger.Core.Models;
using Messenger.Helpers;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class ChatNavViewModel : Observable
    {
        private ShellViewModel _shellViewModel;
        public ShellViewModel ShellViewModel { get {
                return _shellViewModel;
            }
            set {
                _shellViewModel = value;
            } }

        private User _user;
        public User User
        {
            get
            {
                return _user;
            }
            set { Set(ref _user, value); }
        }

        public ChatNavViewModel()
        {
            User = new User();
            User.Userlist = new List<string>();
            User.Userlist.Add("Hello");
            User.Userlist.Add("HI");
        }

    }
}
