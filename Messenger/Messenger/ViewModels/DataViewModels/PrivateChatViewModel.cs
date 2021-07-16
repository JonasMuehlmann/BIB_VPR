﻿using Messenger.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.ViewModels.DataViewModels
{
    public class PrivateChatViewModel : TeamViewModel
    {
        private Member _partner;
        private MessageViewModel _lastMessage;
        private ChannelViewModel _mainChannel;

        public Member Partner
        {
            get { return _partner; }
            set { Set(ref _partner, value); }
        }

        public MessageViewModel LastMessage
        {
            get { return _lastMessage; }
            set { Set(ref _lastMessage, value); }
        }

        public ChannelViewModel MainChannel
        {
            get { return _mainChannel; }
            set { Set(ref _mainChannel, value); }
        }

        public PrivateChatViewModel(TeamViewModel viewModel)
        {
            Member partner = viewModel.Members.FirstOrDefault();
            ChannelViewModel mainChannel = viewModel.Channels.FirstOrDefault();

            Id = viewModel.Id;
            TeamName = viewModel.TeamName;
            Description = viewModel.Description;
            CreationDate = viewModel.CreationDate;
            Partner = partner;
            MainChannel = mainChannel;
        }
    }
}
