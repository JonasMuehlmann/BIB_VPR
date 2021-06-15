using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Core;

namespace Messenger.ViewModels
{
    public class ChatHubViewModel : Observable
    {
        #region Private

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private UserService UserService => Singleton<UserService>.Instance;

        private string _errorMessage;
        private uint _currentTeamId = 1;
        private UserViewModel _user;
        private ObservableCollection<Message> _messages;

        #endregion

        #region Properties

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
                LoadMessages(value);
            }
        }
        
        #endregion

        #region Commands

        /// <summary>
        /// Command: sends a message with the current team id
        /// </summary>
        public ICommand SendMessageCommand => new SendMessageCommand(this, MessengerService);

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
            //MessengerService.RegisterListenerForMessages(OnMessageReceived);
            // Bind to "ReceiveInvite" event
            //MessengerService.RegisterListenerForInvites(OnInviteReceived);

            LoadAsync();
        }

        /// <summary>
        /// Safely calls asynchronous methods on UI-Thread
        /// </summary>
        public async void LoadAsync()
        {
            await CoreWindow
                .GetForCurrentThread()
                .Dispatcher
                .RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    User = await UserDataService.GetUserAsync();
                    LoadMessages(CurrentTeamId);
                });
        }

        #region Signal-R Events

        /// <summary>
        /// Fires on "ReceiveMessage" Hub-method
        /// </summary>
        /// <param name="message">Message received from the hub</param>
        private void OnMessageReceived(Message message)
        {
            Debug.WriteLine($"Message Received::{message.Content} From {message.SenderId} To Team #{message.RecipientId}::{message.CreationTime}");

            AddMessageToCollection(message);
        }

        /// <summary>
        /// Fires on "ReceiveInvitation" Hub-method
        /// </summary>
        /// <param name="teamId"></param>
        private void OnInviteReceived(uint teamId)
        {
            Debug.WriteLine($"Joined the chat:: Team #{teamId}");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Asynchronously loads messages either from cache or database
        /// </summary>
        /// <param name="teamId">Current team id</param>
        private async void LoadMessages(uint teamId)
        {
            ObservableCollection<Message> fromCache = new ObservableCollection<Message>();

            // Checks the cache if the messages has not yet been loaded for the team
            if (!MessagesByConnectedTeam.TryGetValue(teamId, out fromCache))
            {
                // Loads from database
                var fromDB = await MessengerService.LoadMessages(teamId);
                UpdateMessagesView(fromDB);
            }
            else
            {
                // Loads from cache
                UpdateMessagesView(fromCache);
            }
        }

        /// <summary>
        /// Adds the message to the cache
        /// </summary>
        /// <param name="message">A complete message object to be added</param>
        private async void AddMessageToCollection(Message message)
        {
            var teamId = message.RecipientId;

            // Loads user data of the sender
            message.Sender = await UserService.GetUser(message.SenderId);

            // Adds to message dictionary
            MessagesByConnectedTeam.AddOrUpdate(
                teamId,
                new ObservableCollection<Message>() { message },
                (key, collection) => {
                    collection.Add(message);
                    return collection;
                });

            // Adds to the messages list if the message is for the current team 
            if (teamId == CurrentTeamId)
            {
                Messages.Add(message);
            }
        }

        /// <summary>
        /// Updates UI with the given messages
        /// </summary>
        /// <param name="messages">List of messages to be updated on the view</param>
        private void UpdateMessagesView(IEnumerable<Message> messages)
        {
            Messages.Clear();
            foreach (var message in messages)
            {
                Messages.Add(message);
            }
        }

        #endregion
    }
}
