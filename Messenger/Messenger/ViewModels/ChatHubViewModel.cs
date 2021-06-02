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
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private UserService UserService => Singleton<UserService>.Instance;

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
        /// ChatHubViewModel connects to signal-r hub and listens for messages
        /// </summary>
        public ChatHubViewModel()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<uint, ObservableCollection<Message>>();
            Messages = new ObservableCollection<Message>();

            CurrentTeamId = 1;

            // Bind to "ReceiveMessage" event
            MessengerService.RegisterListener(OnMessageReceived);

            LoadAsync();
        }

        public async void LoadAsync()
        {
            User = await UserDataService.GetUserAsync();

            var messages = await MessengerService.LoadMessages(CurrentTeamId);
            Messages.Clear();
            foreach (var message in messages)
            {
                message.Sender = await UserService.GetUser(message.SenderId);
                Messages.Add(message);
            }
        }

        #region Events

        /// <summary>
        /// Fires on "ReceiveMessage" Hub-method
        /// </summary>
        /// <param name="message">Message received from the hub</param>
        private void OnMessageReceived(Message message)
        {
            Debug.WriteLine($"Message Received::{message.Content} From {message.SenderId} To Team #{message.RecipientId}::{message.CreationTime}");

            AddMessageToCollection(message);
        }

        #endregion

        #region Helpers

        private async void AddMessageToCollection(Message message)
        {
            var team = message.RecipientId;
            message.Sender = await UserService.GetUser(message.SenderId);

            // Adds to message dictionary
            MessagesByConnectedTeam.AddOrUpdate(
                team,
                new ObservableCollection<Message>() { message },
                (key, collection) => {
                    collection.Add(message);
                    return collection;
                });

            if (team == CurrentTeamId)
            {
                Messages.Add(message);
            }
        }

        #endregion
    }
}
