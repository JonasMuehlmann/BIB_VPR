using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.ViewModels
{
    public class ChatHubViewModel : Observable
    {
        #region Private

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
        /// Command: switch currently selected team
        /// </summary>
        public ICommand SwitchTeamCommand => new SwitchTeamCommand(this);

        #endregion

        /// <summary>
        /// ChatHubViewModel should only be created through the factory method below
        /// </summary>
        private ChatHubViewModel()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<uint, ObservableCollection<Message>>();

            // Bind to "ReceiveMessage" event
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
        /// <returns>Task to be awaited</returns>
        public Task Initialize()
        {
            return Singleton<UserDataService>.Instance
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

        #region Events

        /// <summary>
        /// Fires on "ReceiveMessage" Hub-method
        /// </summary>
        /// <param name="message">Message received from the hub</param>
        private void OnMessageReceived(Message message)
        {
            Debug.WriteLine($"Message Received::{message.Content} From {message.SenderId} To Team #{message.RecipientId}::{message.CreationTime}");

            // Adds to message dictionary
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
