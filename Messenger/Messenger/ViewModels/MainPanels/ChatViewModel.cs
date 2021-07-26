using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.Messenger;
using Messenger.Commands.PrivateChat;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using Windows.Storage;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        #region Private

        private ObservableCollection<MessageViewModel> _messages;

        private IReadOnlyList<StorageFile> _selectedFiles;

        private MessageViewModel _replyMessage;

        private Message _messageToSend;

        #endregion

        #region Properties

        /// <summary>
        /// Loaded messages of the current team/chat
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
                Set(ref _messages, value);
            }
        }

        /// <summary>
        /// Attachments list to upload with the message
        /// </summary>
        public IReadOnlyList<StorageFile> SelectedFiles {
            get
            {
                return _selectedFiles;
            }
            set
            {
                Set(ref _selectedFiles, value);
            }
        }

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

        #endregion

        #region Commands

        public ICommand SendMessageCommand => new SendMessageCommand(this);

        public ICommand OpenFilesCommand => new AttachFileCommand(this);

        public ICommand ReplyToCommand => new ReplyMessageCommand(this);

        public ICommand UpdateMessageCommand => new UpdateMessageCommand();

        public ICommand UpdateTeamDetailsCommand => new UpdateTeamDetailsCommand();

        public ICommand DeleteMessageCommand => new DeleteMessageCommand();

        public ICommand ToggleReactionCommand => new ToggleReactionCommand();

        public ICommand OpenTeamManagerCommand => new RelayCommand(() => NavigationService.Open<TeamManagePage>());

        public ICommand OpenSettingsCommand => new RelayCommand(() => NavigationService.Open<SettingsPage>());

        #endregion

        public ChatViewModel()
        {
            Initialize();
        }

        private void Initialize()
        {
            Messages = new ObservableCollection<MessageViewModel>();
            MessageToSend = new Message();

            /** LOAD FROM CACHE **/
            if (App.StateProvider != null)
            {
                if (CacheQuery.TryGetMessages(SelectedChannel.ChannelId, out ObservableCollection<MessageViewModel> messages))
                {
                    Messages.Clear();

                    foreach (MessageViewModel message in messages)
                    {
                        Messages.Add(message);
                    }
                }
            }
        }

        #region Events

        public void OnMessageUpdated(object sender, BroadcastArgs e)
        {
            MessageViewModel message = e.Payload as MessageViewModel;

            if (message == null) return;

            if (e.Reason == BroadcastReasons.Created)
            {
                Messages.Add(message);
            }
            else if (e.Reason == BroadcastReasons.Updated)
            {
                MessageViewModel target = Messages.Single(m => m.Id == message.Id);

                if (target != null)
                {
                    int index = Messages.IndexOf(target);

                    Messages[index] = target;
                }
            }
            else if (e.Reason == BroadcastReasons.Deleted)
            {
                MessageViewModel target = Messages.Single(m => m.Id == message.Id);

                if (target != null)
                {
                    Messages.Remove(target);
                }
            }
        }

        public void OnMessagesSwitched(object sender, BroadcastArgs e)
        {
            IEnumerable<MessageViewModel> messages = e.Payload as IEnumerable<MessageViewModel>;

            if (messages != null && messages.Count() > 0)
            {
                Messages.Clear();

                foreach (MessageViewModel message in messages)
                {
                    Messages.Add(message);
                }
            }
        }

        #endregion
    }
}
