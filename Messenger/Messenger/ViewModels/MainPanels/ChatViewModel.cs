using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
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

        private Message _replyMessage;

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

        public ICommand SendMessageCommand => new RelayCommand<string>(SendMessage);
        public ICommand OpenFilesCommand => new RelayCommand(SelectFiles);
        public ICommand ReplyToCommand => new RelayCommand<Message>(ReplyTo);

        public ChatViewModel()
        {
            Messages = new ObservableCollection<Message>();
            Hub.MessageReceived += OnMessageReceived;
            Hub.TeamSwitched += OnTeamSwitched;
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

        private async void SendMessage(string content)
        {
            uint? parentMessageId = null;

            if (ReplyMessage != null)
            {
                parentMessageId = ReplyMessage.Id;
            }

            // Records input, created timestamp and parent message id
            var message = new Message()
            {
                Content = content,
                CreationTime = DateTime.Now,
                ParentMessageId = parentMessageId
            };

            // User/Team data will be handled in ChatHubService
            await Hub.SendMessage(message, SelectedFiles);
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

        private async void SelectFiles() 
        {
            FileOpenPicker openPicker = new FileOpenPicker();
            openPicker.FileTypeFilter.Add("*");
            IReadOnlyList<StorageFile> files = await openPicker.PickMultipleFilesAsync();

            if (files.Count > 0)
            {
                SelectedFiles = files;
            }
        }
        
        private void OnTeamSwitched(object sender, IEnumerable<Message> messages)
        {
            UpdateView(messages);
        }
    }
}
