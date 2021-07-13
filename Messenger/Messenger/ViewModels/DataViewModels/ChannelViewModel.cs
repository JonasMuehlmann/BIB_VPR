using Messenger.Helpers;
using System;

namespace Messenger.ViewModels.DataViewModels
{
    public class ChannelViewModel : Observable
    {
        private MessageViewModel _pinnedMessage;
        private string _channelName;
        private uint _channelId;
        private string _description;
        private uint _teamId;

        public MessageViewModel PinnedMessage
        {
            get { return _pinnedMessage; }
            set { Set(ref _pinnedMessage, value); }
        }

        public string ChannelName
        {
            get { return _channelName; }
            set { Set(ref _channelName, value); }
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

        public uint TeamId
        {
            get { return _teamId; }
            set { Set(ref _teamId, value); }
        }

        public ChannelViewModel()
        {

        }
    }
}
