using Messenger.Commands;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.ViewModels
{
    public class ChatRoomViewModel : Observable
    {
        private UserDataService _userDataService => Singleton<UserDataService>.Instance;

        private readonly SignalRChatService _chatService;

        private Message _message;

        public Message Message
        {
            get { return _message; }
            set
            {
                _message = value;
                Set(ref _message, value);
            }
        }

        private bool _isConnected;

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                Set(ref _isConnected, value);
            }
        }

        private string _errorMessage;

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                Set(ref _errorMessage, value);
            }
        }

        private UserViewModel _user;

        public UserViewModel User
        {
            get { return _user; }
            set { _user = value; }
        }

        public ICommand SendChatRoomMessageCommand { get => new SendChatRoomMessageCommand(this, _chatService); }

        public ObservableCollection<Message> Messages { get; }

        public ChatRoomViewModel(SignalRChatService chatService)
        {
            _chatService = chatService;
            _chatService.MessageReceived += ChatService_MessageReceived;

            Messages = new ObservableCollection<Message>();

            GetUser();
        }

        public static ChatRoomViewModel CreateConnectedViewModel(SignalRChatService chatService)
        {
            ChatRoomViewModel viewModel = new ChatRoomViewModel(chatService);

            chatService.Connect().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    viewModel.ErrorMessage = "Unable to connect to chat room";
                }
            });

            return viewModel;
        }

        private async void GetUser()
        {
            User = await _userDataService.GetUserAsync();
        }

        private void ChatService_MessageReceived(Message message)
        {
            Debug.WriteLine($"Message Received::{message.Content} From {message.SenderId}::{message.CreationTime}");
            Messages.Add(message);
        }
    }
}
