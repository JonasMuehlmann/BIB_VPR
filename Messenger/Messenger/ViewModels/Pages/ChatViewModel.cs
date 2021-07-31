using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.ViewModels.Controls;
using Messenger.ViewModels.DataViewModels;

namespace Messenger.ViewModels.Pages
{
    public class ChatViewModel : Observable
    {
        #region Private

        private MessageViewModel _replyMessage;

        private Message _messageToSend;

        #endregion

        #region Properties
        /// <summary>
        /// Message that the user is replying to
        /// </summary>
        public MessageViewModel ReplyMessage
        {
            get
            {
                return _replyMessage;
            }
            set
            {
                Set(ref _replyMessage, value);
            }
        }

        /// <summary>
        /// Message object to be sent
        /// </summary>
        public Message MessageToSend
        {
            get
            {
                return _messageToSend;
            }
            set
            {
                Set(ref _messageToSend, value);
            }
        }

        public TeamViewModel SelectedTeam => App.StateProvider.SelectedTeam;

        public ChannelViewModel SelectedChannel => App.StateProvider.SelectedChannel;

        public UserViewModel CurrentUser => App.StateProvider.CurrentUser;

        public MessagesListControlViewModel MessagesListViewModel { get; set; }

        public SendMessageControlViewModel SendMessageControlViewModel { get; set; }

        #endregion

        public ChatViewModel()
        {
            MessagesListViewModel = new MessagesListControlViewModel(this);
            SendMessageControlViewModel = new SendMessageControlViewModel(this);
        }
    }
}
