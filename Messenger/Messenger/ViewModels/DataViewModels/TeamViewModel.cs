using Messenger.Core.Models;
using Messenger.Helpers;
using System;
using System.Collections.ObjectModel;

namespace Messenger.ViewModels.DataViewModels
{
    public class TeamViewModel : Observable
    {
        private uint? _id;
        private string _teamName;
        private string _description;
        private DateTime _creationDate;
        private ObservableCollection<User> _members;
        private ObservableCollection<ChannelViewModel> _channels;
        private bool _isPrivateChat;

        public uint? Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        public string TeamName
        {
            get { return _teamName; }
            set { Set(ref _teamName, value); }
        }

        public string Description
        {
            get { return _description; }
            set { Set(ref _description, value); }
        }

        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { Set(ref _creationDate, value); }
        }

        public ObservableCollection<User> Members
        {
            get { return _members; }
            set { Set(ref _members, value); }
        }

        public ObservableCollection<ChannelViewModel> Channels
        {
            get { return _channels; }
            set { Set(ref _channels, value); }
        }

        public bool IsPrivateChat
        {
            get { return _isPrivateChat; }
            set { Set(ref _isPrivateChat, value); }
        }

        public TeamViewModel()
        {

        }
    }
}
