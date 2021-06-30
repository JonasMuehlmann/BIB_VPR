using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Services;
using Windows.Storage;
using Windows.UI.Xaml;
using Prism.Commands;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        #region Private

        private ObservableCollection<Message> _messages;
        private IReadOnlyList<StorageFile> _selectedFiles;
        private ChatHubService Hub => Singleton<ChatHubService>.Instance;
        private Message _replyMessage;
        private Visibility _replyVisible;
        private Message _messageToSend;

        #endregion

        #region Properties

        /// <summary>
        /// Loaded messages of the current team/chat
        /// </summary>
        public ObservableCollection<Message> Messages
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
        public Message ReplyMessage
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
        /// Reply Box in SendMessageControl is visible or not
        /// </summary>
        public Visibility ReplyVisible
        {
            get =>  _replyVisible;
            set
            {
                OnPropertyChanged(nameof(ReplyVisible));
                _replyVisible = value;
            }
        }

        public DelegateCommand BtnToggleReplyVisibility { get; set; }


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

        #endregion

        public ChatViewModel()
        {
            // Initialize models
            Messages = new ObservableCollection<Message>();
            ReplyMessage = new Message();
            MessageToSend = new Message();
            ReplyVisible = Visibility.Visible;
            BtnToggleReplyVisibility = new DelegateCommand(ToggleVisibility);

            // Register events
            Hub.MessageReceived += OnMessageReceived;
            Hub.TeamSwitched += OnTeamSwitched;

            LoadAsync();
        }
         
        /// <summary>
        /// Loads messages from the hub
        /// </summary>
        private async void LoadAsync()
        {
            var messages = await Hub.GetMessages();

            UpdateView(messages);
        }

        #region Helper

        /// <summary>
        /// Updates the view with the given messages
        /// </summary>
        /// <param name="messages">Messages from the hub</param>
        private void UpdateView(IEnumerable<Message> messages)
        {
            Messages.Clear();

            if (messages == null)
            {
                return;
            }

            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }

        public void ToggleVisibility()
        {
            if (ReplyVisible == Visibility.Visible)
            {
                ReplyVisible = Visibility.Collapsed;
            }
            else
            {
                ReplyVisible = Visibility.Visible;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Fires on MessageReceived of ChatHubService
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="message">Received Message object</param>
        private void OnMessageReceived(object sender, Message message)
        {
            if (message.RecipientId == Hub.CurrentTeamId)
            {
                Messages.Add(message);
            }
        }

        /// <summary>
        /// Fires on TeamSwitched of ChatHubService
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="messages">List of message of the current team</param>
        private void OnTeamSwitched(object sender, IEnumerable<Message> messages)
        {
            UpdateView(messages);
        }

        #endregion
    }
}
