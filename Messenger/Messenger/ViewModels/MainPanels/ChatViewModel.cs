using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        private ShellViewModel _shellViewModel;
        private ObservableCollection<Message> _messages;
        private IReadOnlyList<StorageFile> _selectedFiles;
        private ChatHubService Hub => Singleton<ChatHubService>.Instance;
        private Message _replyMessage;
        private Message _messageToSend;

        public ShellViewModel ShellViewModel
        {
            get
            {
                return _shellViewModel;
            }
            set
            {
                _shellViewModel = value;
            }
        }

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

        public ICommand SendMessageCommand => new SendMessageCommand(this, Hub);
        public ICommand OpenFilesCommand => new OpenFilesCommand(this);
        public ICommand ReplyToCommand => new RelayCommand<Message>(ReplyTo);

        public ChatViewModel()
        {
            // Initialize models
            Messages = new ObservableCollection<Message>();
            ReplyMessage = new Message();
            MessageToSend = new Message();

            // Register events
            Hub.MessageReceived += OnMessageReceived;
            Hub.TeamSwitched += OnTeamSwitched;

            // Load teams
            LoadAsync();
        }

        private async void LoadAsync()
        {
            if (Hub.CurrentTeamId == null) Hub.CurrentTeamId = 1;
            UpdateView(await Hub.GetMessages());
        }

        private void UpdateView(IEnumerable<Message> messages)
        {
            Messages.Clear();
            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }

        private void ReplyTo(Message message)
        {
            ReplyMessage = message;
        }

        private void OnMessageReceived(object sender, Message message)
        {
            if (message.RecipientId == Hub.CurrentTeamId)
            {
                Messages.Add(message);
            }
        }
        
        private void OnTeamSwitched(object sender, IEnumerable<Message> messages)
        {
            UpdateView(messages);
        }
    }
}
