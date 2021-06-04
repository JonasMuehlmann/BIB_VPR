using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Services;
using Windows.UI.Xaml.Navigation;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        private ShellViewModel _shellViewModel;
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
                _shellViewModel.ChatName = "ChatName";
            }
        }

        private ObservableCollection<Message> _messages;

        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                Set(ref _messages, value);
            }
        }

        public ICommand SendMessageCommand => new RelayCommand<string>(SendMessage);

        public ChatViewModel()
        {
            Messages = new ObservableCollection<Message>();
            Hub.MessageReceived += OnMessageReceived;
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
            await Hub.SendMessage(content);
        }

        private void OnMessageReceived(object sender, Message message)
        {
            if (message.RecipientId == Hub.CurrentTeamId)
            {
                Messages.Add(message);
            }
        }
    }
}
