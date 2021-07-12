using Messenger.Helpers;
using System;

namespace Messenger.ViewModels.DataViewModels
{
    public class ChannelViewModel : Observable
    {
        private MessageViewModel _pinnedMessage;
        private string _name;
        private DateTime _creationDate;
        private uint _channelId;
        private string _description;

        public MessageViewModel PinnedMessage
        {
            get { return _pinnedMessage; }
            set { Set(ref _pinnedMessage, value); }
        }

        public string Name
        {
            get { return _name; }
            set { Set(ref _name, value); }
        }

        public DateTime CreationDate
        {
            get { return _creationDate; }
            set { Set(ref _creationDate, value); }
        }

        public uint ChannelId
        {
            get { return _channelId; }
            set { Set(ref _channelId, value); }
        }

        public string Description
        {
            get { return _description; }
            set { Set(ref _description, value); }
        }

        public ChannelViewModel()
        {

        }
    }
}
