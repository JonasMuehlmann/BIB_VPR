﻿using Messenger.Commands.TeamManage;
using Messenger.Helpers;
using System;
using System.Windows.Input;

namespace Messenger.ViewModels.DataViewModels
{
    public class ChannelViewModel : DataViewModel
    {
        private MessageViewModel _pinnedMessage;
        private string _channelName;
        private uint _channelId;
        private string _description;
        private uint _teamId;
        private MessageViewModel _lastMessage;

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

        public ICommand RemoveChannelCommand
        {
            get => new RemoveChannelCommand();
        }

        public MessageViewModel LastMessage
        {
            get { return _lastMessage; }
            set { Set(ref _lastMessage, value); }
        }

        public ChannelViewModel()
        {

        }

        public ChannelViewModel Clone(ChannelViewModel c) {
            PinnedMessage = c.PinnedMessage;
            ChannelName = c.ChannelName;
            ChannelId = c.ChannelId;
            Description = c.Description;
            TeamId = c.TeamId;
            LastMessage = c.LastMessage;
            return this;
        }
    }
}
