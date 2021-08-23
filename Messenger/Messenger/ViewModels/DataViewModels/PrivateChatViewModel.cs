using Messenger.Commands.PrivateChat;
using System.Linq;
using System.Windows.Input;

namespace Messenger.ViewModels.DataViewModels
{
    public class PrivateChatViewModel : TeamViewModel
    {
        private MemberViewModel _partner;
        private MessageViewModel _lastMessage;
        private ChannelViewModel _mainChannel;

        public MemberViewModel Partner
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
        
        public ICommand RemoveChatCommand => new RemoveChatCommand();

        public PrivateChatViewModel(TeamViewModel viewModel)
        {
            MemberViewModel partner = viewModel.Members.FirstOrDefault();
            ChannelViewModel mainChannel = viewModel.Channels.FirstOrDefault();

            Id = viewModel.Id;
            TeamName = viewModel.TeamName;
            Description = viewModel.Description;
            CreationDate = viewModel.CreationDate;
            Members = viewModel.Members;
            Partner = partner;
            MainChannel = mainChannel;
        }
    }
}
