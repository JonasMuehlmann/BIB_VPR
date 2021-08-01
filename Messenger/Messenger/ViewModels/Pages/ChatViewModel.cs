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
        /// Message that the user is replying to,
        /// update is triggered by MessageView, data is injected to SendMessageControl
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
        /// Message object to be sent,
        /// update is triggered by SendMessageControl, data is injected to SendMessageCommand
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

        /// <summary>
        /// Defines data to be bound to 'Chat Page',
        /// also serves as a shared data pool between MessagesListControl and SendMessageControl
        /// </summary>
        public ChatViewModel()
        {
            MessagesListViewModel = new MessagesListControlViewModel(this);
            SendMessageControlViewModel = new SendMessageControlViewModel(this);
            MessageToSend = new Message();

            App.EventProvider.MessagesSwitched += MessagesListViewModel.OnMessagesSwitched;
            App.EventProvider.MessageUpdated += MessagesListViewModel.OnMessageUpdated;
        }
    }
}
