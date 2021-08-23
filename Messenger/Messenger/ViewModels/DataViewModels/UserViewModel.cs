using System.Collections.Generic;
using Messenger.Core.Models;
using Windows.UI.Xaml.Media.Imaging;

namespace Messenger.ViewModels.DataViewModels
{
    public class UserViewModel : DataViewModel
    {
        private string _name;
        private string _bio;
        private string _mail;
        private BitmapImage _photo;
        private string _id;
        private uint _nameId;

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

        public uint NameId
        {
            get => _nameId;
            set => Set(ref _nameId, value);
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

        public UserViewModel()
        {
        }

        public MemberViewModel ToMemberViewModel(uint teamId)
        {
            return new MemberViewModel()
            {
                Id = Id,
                Name = Name,
                NameId = NameId,
                Bio = Bio,
                Mail = Mail,
                Photo = Photo,
                MemberRoles = new List<TeamRoleViewModel>(),
                AssignableMemberRoles = new List<TeamRoleViewModel>(),
                TeamId = teamId
            };
        }

        public User ToUserObject()
        {
            return new User()
            {
                Id = Id,
                DisplayName = Name,
                NameId = NameId,
                Photo = Photo.UriSource.ToString(),
                Mail = Mail,
                Bio = Bio
            };
        }
    }
}
