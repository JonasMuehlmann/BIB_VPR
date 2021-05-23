﻿using Messenger.Commands;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.ViewModels
{
    public class SignalRHubViewModel : Observable
    {
        #region Private

        private readonly SignalRService _signalRService;
        private Message _message;
        private bool _isConnected;
        private string _errorMessage;
        private UserViewModel _user;

        #endregion

        public Message Message
        {
            get { return _message; }
            set
            {
                _message = value;
                Set(ref _message, value);
            }
        }

        public bool IsConnected
        {
            get { return _isConnected; }
            set
            {
                _isConnected = value;
                Set(ref _isConnected, value);
            }
        }

        /// <summary>
        /// Error messages from SignalR operations, returns string.Empty if there is no Exception
        /// </summary>
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                Set(ref _errorMessage, value);
            }
        }

        /// <summary>
        /// Collection of messages to be shown on UI
        /// </summary>
        public ObservableCollection<Message> Messages { get; }

        /// <summary>
        /// Currently logged-in user
        /// </summary>
        public UserViewModel User
        {
            get { return _user; }
            set { _user = value; }
        }

        /// <summary>
        /// Sends a message with the current team id
        /// </summary>
        public ICommand SendMessageCommand => new SendMessageCommand(this, _signalRService);

        /// <summary>
        /// Current target team id to send messages(Message.RecipientsId)
        /// </summary>
        public int CurrentTeamId = 1;

        /// <summary>
        /// Constructor with services only
        /// </summary>
        /// <param name="signalRService">SignalRService from the view model (Singleton)</param>
        /// <param name="userDataService">UserDataService from the view model (Singleton)</param>
        public SignalRHubViewModel(SignalRService signalRService, UserDataService userDataService)
        {
            _signalRService = signalRService;
            Messages = new ObservableCollection<Message>();

            // Loads current user data
            userDataService.GetUserAsync().ContinueWith(async (task) =>
            {
                if (task.Exception == null)
                {
                    User = task.Result;
                }
                else
                {
                    ErrorMessage = "Unable to fetch user data";
                    User = null;
                }

                // Subscribes to hub groups
                await _signalRService.JoinTeam(CurrentTeamId.ToString());
            });

            // Subscribes to "ReceiveMessage" event
            _signalRService.MessageReceived += ChatService_MessageReceived;
        }

        /// <summary>
        /// Returns SignalRHubViewModel with the pre-configured connection
        /// </summary>
        /// <param name="signalRService"></param>
        /// <param name="userDataService"></param>
        /// <returns></returns>
        public static SignalRHubViewModel CreateHubConnection(SignalRService signalRService, UserDataService userDataService)
        {
            SignalRHubViewModel viewModel = new SignalRHubViewModel(signalRService, userDataService);

            // Connects to the hub with the pre-configured setting
            signalRService.ConnectToHub().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    viewModel.ErrorMessage = "Unable to connect to chat room";
                    viewModel.IsConnected = false;
                }
                else
                {
                    viewModel.IsConnected = true;
                }
            });

            return viewModel;
        }

        private void ChatService_MessageReceived(Message message)
        {
            Debug.WriteLine($"Message Received::{message.Content} From {message.SenderId} To Team #{message.RecipientId}::{message.CreationTime}");

            // TODO::Save to Database::Messenger.Core.Services.MessageService

            // Updates to UI
            Messages.Add(message);
        }
    }
}
