using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.Messenger;
using Messenger.Commands.PrivateChat;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using Messenger.Views.DialogBoxes;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        #region Private

        private ChatHubService Hub => Singleton<ChatHubService>.Instance;

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

        /// <summary>
        /// Sends the current MessageToSend model
        /// </summary>
        public ICommand SendMessageCommand => new SendMessageCommand(this, Hub);

        /// <summary>
        /// Open file open picker for attachments on the message
        /// </summary>
        public ICommand OpenFilesCommand => new OpenFilesCommand(this);

        /// <summary>
        /// Marks the current MessageToSend model as a reply
        /// </summary>
        public ICommand ReplyToCommand => new ReplyToCommand(this);

        public ICommand EditMessageCommand => new EditMessageCommand(Hub);

        public ICommand DeleteMessageCommand => new DeleteMessageCommand(Hub);

        public ICommand ToggleReactionCommand => new ToggleReactionCommand(Hub);

        public ICommand OpenTeamManagerCommand => new RelayCommand(() => NavigationService.Open<TeamManagePage>());

        public ICommand OpenSettingsCommand => new RelayCommand(() => NavigationService.Open<SettingsPage>());

        public ICommand EditTeamDetailsCommand => new UpdateTeamDetailsCommand(Hub);

        #endregion

        public ChatViewModel()
        {
            // Initialize models
            Messages = new ObservableCollection<MessageViewModel>();
            MessageToSend = new Message();

            // Register events
            App.EventProvider.MessagesSwitched += OnMessagesSwitched;
            App.EventProvider.MessageUpdated += OnMessageUpdated;

            LoadAsync();
        }

        /// <summary>
        /// Loads messages from the hub
        /// </summary>
        private async void LoadAsync()
        {
            IEnumerable<MessageViewModel> messages = await Hub.GetMessagesForSelectedChannel();

            UpdateView(messages);
        }

        #region Helper

        /// <summary>
        /// Updates the view with the given messages
        /// </summary>
        /// <param name="messages">Messages from the hub</param>
        private void UpdateView(IEnumerable<MessageViewModel> messages)
        {
            Messages.Clear();

            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }

        #endregion

        #region Events

        private void OnMessageUpdated(object sender, BroadcastArgs e)
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

        private void OnMessagesSwitched(object sender, BroadcastArgs e)
        {
            IEnumerable<MessageViewModel> messages = e.Payload as IEnumerable<MessageViewModel>;

            if (messages != null && messages.Count() > 0)
            {
                UpdateView(messages);
            }
        }

        #endregion
    }
}
