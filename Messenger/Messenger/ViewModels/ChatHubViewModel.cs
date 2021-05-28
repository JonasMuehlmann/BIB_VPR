using Messenger.Commands;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.ViewModels
{
    public class ChatHubViewModel : Observable
    {
        #region Private

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private MessengerService MessengerService => Singleton<MessengerService>.Instance;

        private Message _message;
        private bool _isConnected;
        private string _errorMessage;
        private uint _currentTeamId = 1;
        private UserViewModel _user;
        private ObservableCollection<Message> _messages;

        #endregion

        #region Properties

        /// <summary>
        /// Message to send out
        /// </summary>
        public Message Message
        {
            get { return _message; }
            set
            {
                _message = value;
                Set(ref _message, value);
            }
        }

        /// <summary>
        /// Status of connection to the hub
        /// </summary>
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
        /// Dictionary for connected teams and their messages
        /// </summary>
        public ConcurrentDictionary<uint, ObservableCollection<Message>> MessagesByConnectedTeam { get; }

        /// <summary>
        /// Collection of messages from the dictionary to be shown on UI
        /// </summary>
        public ObservableCollection<Message> Messages
        {
            get { return _messages; }
            set
            {
                _messages = value;
                Set(ref _messages, value);
            }
        }

        /// <summary>
        /// Currently logged-in user
        /// </summary>
        public UserViewModel User
        {
            get { return _user; }
            set { _user = value; }
        }

        /// <summary>
        /// Current target team id to send messages(Message.RecipientsId)
        /// </summary>
        public uint CurrentTeamId
        {
            get { return _currentTeamId; }
            set
            {
                _currentTeamId = value;
                Set(ref _currentTeamId, value);
            }
        }
        
        #endregion

        #region Commands

        /// <summary>
        /// Command: sends a message with the current team id
        /// </summary>
        public ICommand SendMessageCommand => new SendMessageCommand(this, MessengerService);

        /// <summary>
        /// Command: switch current team id
        /// </summary>
        public ICommand SwitchTeamCommand => new RelayCommand<string>(SwitchTeam);

        #endregion

        private ChatHubViewModel()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<uint, ObservableCollection<Message>>();

            // Start listening to "ReceiveMessage" event
            MessengerService.RegisterListener(OnMessageReceived);
        }

        /// <summary>
        /// Returns SignalRHubViewModel with the pre-configured connection
        /// </summary>
        /// <returns>ChatHubViewModel with the connection to SignalR-service</returns>
        public static ChatHubViewModel CreateAndConnect()
        {
            ChatHubViewModel viewModel = new ChatHubViewModel();

            viewModel
                .Initialize()
                .ContinueWith(task =>
                {
                    if (viewModel.IsConnected)
                    {
                        Debug.WriteLine("Connected to the hub.");
                    }
                    else
                    {
                        Debug.WriteLine("Failed to connect.");
                    }
                });

            return viewModel;
        }

        /// <summary>
        /// Loads the current user data and initialize messenger service with the user id
        /// </summary>
        public Task Initialize()
        {
            return UserDataService
                .GetUserAsync()
                .ContinueWith((task) =>
                {
                    if (task.Exception != null)
                    {
                        ErrorMessage = "Unable to fetch user data";
                        IsConnected = false;
                    }
                    else
                    {
                        User = task.Result;

                        // Initialize messenger service upon success
                        MessengerService.Initialize(User.Id).Wait();
                        IsConnected = true;
                    }
                });
        }

        #region UI-Commands

        /// <summary>
        /// Switch the team to be shown on the view
        /// </summary>
        /// <param name="teamId">Id of a team</param>
        private void SwitchTeam(string teamId)
        {
            CurrentTeamId = Convert.ToUInt32(teamId);
            Messages = MessagesByConnectedTeam.GetOrAdd(CurrentTeamId, new ObservableCollection<Message>());
        }

        #endregion

        #region Events

        /// <summary>
        /// Fires on "ReceiveMessage" Hub-method
        /// </summary>
        /// <param name="message">Message received from the hub</param>
        private void OnMessageReceived(Message message)
        {
            Debug.WriteLine($"Message Received::{message.Content} From {message.SenderId} To Team #{message.RecipientId}::{message.CreationTime}");

            // TODO::Save to Database::Messenger.Core.Services.MessageService

            // Updates to UI
            MessagesByConnectedTeam.AddOrUpdate(
                message.RecipientId,
                new ObservableCollection<Message>() { message },
                (key, collection) => {
                    collection.Add(message);
                    return collection;
                });
        }

        #endregion
    }
}
