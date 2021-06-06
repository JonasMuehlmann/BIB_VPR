using System;
using System.Collections.Generic;
using Messenger.Core.Models;
using Messenger.Helpers;

using Windows.UI.Xaml.Media.Imaging;

namespace Messenger.ViewModels
{
    public class UserViewModel : Observable
    {
        private string _name;
        private string _userPrincipalName;
        private string _bio;
        private string _mail;
        private BitmapImage _photo;
        private string _id;
        private List<Team> _teams = new List<Team>();

        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Name
        {
            get => _name;
            set => Set(ref _name, value);
        }

        public string UserPrincipalName
        {
            get => _userPrincipalName;
            set => Set(ref _userPrincipalName, value);
        }

        public string Bio
        {
            get { return _bio; }
            set { _bio = value; }
        }

        public string Mail
        {
            get { return _mail; }
            set { _mail = value; }
        }

        public BitmapImage Photo
        {
            get => _photo;
            set => Set(ref _photo, value);
        }

        public List<Team> Teams
        {
            get { return _teams; }
            set
            {
                _teams = value;
                Set(ref _teams, value);
            }
        }

        public UserViewModel()
        {
        }

        public User ToUserObject() {
            return new User() { Id = Id, DisplayName = UserPrincipalName, NameId = 0, Photo = "", Mail = Mail, Bio = Bio};
        }
    }
}
