using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.Controls;
using Messenger.ViewModels.DataViewModels;
using System.Linq;

namespace Messenger.ViewModels.Pages
{
    public class ChatViewModel : Observable
    {
        #region Private

        private MessageViewModel _replyMessage;

        private Message _messageToSend;

        private TeamViewModel _selectedTeam;

        private ChannelViewModel _selectedChannel;

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

        public TeamViewModel SelectedTeam
        {
            get { return _selectedTeam; }
            set { Set(ref _selectedTeam, value); }
        }

        public ChannelViewModel SelectedChannel
        {
            get { return _selectedChannel; }
            set { Set(ref _selectedChannel, value); }
        }

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

            /** REFERENCE TO GLOBAL STATE **/
            SelectedTeam = App.StateProvider.SelectedTeam;
            SelectedChannel = App.StateProvider.SelectedChannel;

            /** EVENTS REGISLATION **/
            App.EventProvider.MessagesSwitched += MessagesListViewModel.OnMessagesSwitched;
            App.EventProvider.MessageUpdated += MessagesListViewModel.OnMessageUpdated;
            App.EventProvider.TeamUpdated += OnTeamUpdated;
            App.EventProvider.ChannelUpdated += OnChannelUpdated;
        }

        #region Events

        private void OnTeamUpdated(object sender, BroadcastArgs e)
        {
            if (e.Reason == BroadcastReasons.Updated)
            {
                TeamViewModel team = e.Payload as TeamViewModel;

                if (team.Id == SelectedTeam.Id)
                {
                    ChannelViewModel channel = team.Channels.SingleOrDefault(c => c.ChannelId == SelectedChannel.ChannelId);

                    SelectedTeam = team;
                    SelectedChannel = channel;
                }
            }
        }

        private void OnChannelUpdated(object sender, BroadcastArgs e)
        {
            if (e.Reason == BroadcastReasons.Updated)
            {
                ChannelViewModel channel = e.Payload as ChannelViewModel;

                if (channel.ChannelId == SelectedChannel.ChannelId)
                {
                    SelectedChannel = channel;
                }
            }
        }

        #endregion
    }
}
