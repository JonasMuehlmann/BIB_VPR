using Messenger.Commands;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
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
        private SignalRService SignalRService => Singleton<SignalRService>.Instance;
        private TeamService TeamService => Singleton<TeamService>.Instance;

        private Message _message;
        private bool _isConnected;
        private string _errorMessage;
        private UserViewModel _user;

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
        public ConcurrentDictionary<int, ObservableCollection<Message>> MessagesByConnectedTeam { get; }

        /// <summary>
        /// Collection of messages from the dictionary to be shown on UI
        /// </summary>
        public ObservableCollection<Message> Messages
        {
            get
            {
                return MessagesByConnectedTeam.GetOrAdd(CurrentTeamId, new ObservableCollection<Message>());
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
        public int CurrentTeamId = 1;

        #endregion

        #region Commands

        /// <summary>
        /// Command: Sends a message with the current team id
        /// </summary>
        public ICommand SendMessageCommand => new SendMessageCommand(this, SignalRService);

        #endregion

        public ChatHubViewModel()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<int, ObservableCollection<Message>>();

            // Loads current user data
            UserDataService.GetUserAsync().ContinueWith(async (task) =>
            {
                if (task.Exception == null)
                {
                    User = task.Result;

                    // Subscribes to hub groups
                    await ConnectToTeams(User.Id);
                    //await SignalRService.JoinTeam(CurrentTeamId.ToString());

                    // Subscribes to "ReceiveMessage" event
                    SignalRService.MessageReceived += ChatService_MessageReceived;
                }
                else
                {
                    ErrorMessage = "Unable to fetch user data";
                    User = null;
                }
            });
        }

        /// <summary>
        /// Returns SignalRHubViewModel with the pre-configured connection
        /// </summary>
        /// <returns>ChatHubViewModel with the connection to SignalR-service</returns>
        public static ChatHubViewModel CreateConnectedViewModel()
        {
            ChatHubViewModel viewModel = new ChatHubViewModel();

            // Connects the ViewModel to the hub with the preconfigured setting
            viewModel.SignalRService.ConnectToHub().ContinueWith(task =>
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

        #region Events

        /// <summary>
        /// Fires on "ReceiveMessage" Hub-method
        /// </summary>
        /// <param name="message">Message received from the hub</param>
        private void ChatService_MessageReceived(Message message)
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

        #region Helpers

        /// <summary>
        /// Connects the current user to teams of which user has a membership
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        private async Task ConnectToTeams(string userId)
        {
            var memberships = await TeamService.GetAllMembershipByUserId(User.Id);

            if (memberships == null || memberships.Count <= 0)
            {
                return;
            }

            // Subscribes to hub groups
            foreach (var teamId in memberships.Select(m => m.TeamId.ToString()))
            {
                await SignalRService.JoinTeam(teamId);
            }
        }

        #endregion
    }
}
