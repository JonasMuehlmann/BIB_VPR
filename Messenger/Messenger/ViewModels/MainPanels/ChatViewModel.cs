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

        public ICommand SendMessageCommand => new RelayCommand<string>(SendMessage);
        public ICommand OpenFilesCommand => new RelayCommand(SelectFiles);

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
            await Hub.SendMessage(content, SelectedFiles);
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
