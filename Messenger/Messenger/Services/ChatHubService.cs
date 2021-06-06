using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog.Context;
using Serilog;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels;

namespace Messenger.Services
{
    /// <summary>
    /// Service aggregator and container class for the state management
    /// View models can subscribe to this service for corresponding ui events (MessageReceived, InvitationReceived, etc.)
    /// </summary>
    public class ChatHubService
    {
        #region Private

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private UserService UserService => Singleton<UserService>.Instance;

        #endregion

        #region Properties

        public ConcurrentDictionary<uint, List<Message>> MessagesByConnectedTeam { get; }

        public uint? CurrentTeamId { get; set; }

        public UserViewModel CurrentUser { get; private set; }

        public ILogger logger = GlobalLogger.Instance;
        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for "ReceiveMessage"(SignalR)
        /// </summary>
        public event EventHandler<Message> MessageReceived;

        /// <summary>
        /// Event handler for "ReceiveInvitation"(SignalR)
        /// </summary>
        public event EventHandler<uint> InvitationReceived;

        /// <summary>
        /// Event handler for switching teams
        /// </summary>
        public event EventHandler<IEnumerable<Message>> TeamSwitched;

        #endregion

        public ChatHubService()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<uint, List<Message>>();

            Initialize();
        }

        private async void Initialize()
        {
            CurrentUser = await UserDataService.GetUserAsync();

            MessengerService.RegisterListenerForMessages(OnMessageReceived);
            MessengerService.RegisterListenerForInvites(OnInvitationReceived);
        }

        /// <summary>
        /// Gets all messages of the current team
        /// </summary>
        /// <returns>List of messages</returns>
        public async Task<IEnumerable<Message>> GetMessages()
        {
            LogContext.PushProperty("Method","GetMessages");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");

            if (CurrentTeamId == null)
            {
                logger.Information("No current team set, exiting");
                logger.Information("Return value: null");

                return null;
            }

            uint teamId = (uint)CurrentTeamId;

            // Checks the cache if the messages has been loaded for the team
            if (MessagesByConnectedTeam.TryGetValue(teamId, out List<Message> fromCache))
            {
                // Loads from cache
                logger.Information($"Return value: {fromCache}");

                return fromCache;
            }
            else
            {
                // Loads from database
                var fromDb = await MessengerService.LoadMessages(teamId);
                CreateEntryForCurrentTeam(fromDb);

                logger.Information($"Return value: {fromDb}");

                return fromDb;
            }
        }

        /// <summary>
        /// Gets the list of teams of the current user
        /// Should only be used to 'reload', since the list should be already loaded in UserViewModel.Teams
        /// </summary>
        /// <returns>List of teams</returns>
        public async Task<IEnumerable<Team>> GetTeamsList()
        {
            LogContext.PushProperty("Method","GetTeamsList");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");


            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            var result = await MessengerService.LoadTeams(CurrentUser.Id);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Sends a message to the current team
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SendMessage(string content)
        {
            LogContext.PushProperty("Method","SendMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter content={content}");

            var message = new Message()
            {
                Content = content,
                CreationTime = DateTime.Now,
                SenderId = CurrentUser.Id,
                RecipientId = (uint)CurrentTeamId
            };

            var result = MessengerService.SendMessage(message);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Updates current team id and invokes registered ui events
        /// </summary>
        /// <param name="teamId">Id of the team to switch to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SwitchTeam(uint teamId)
        {
            LogContext.PushProperty("Method","SwitchTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter teamId={teamId}");

            CurrentTeamId = teamId;
            // Invokes ui events with the list of messages of the team
            TeamSwitched?.Invoke(this, await GetMessages());
        }

        public async Task InviteUser(Invitation invitation)
        {
            LogContext.PushProperty("Method","InviteUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter invitation={invitation}");

            await MessengerService.InviteUser(invitation.UserId, Convert.ToUInt32(invitation.TeamId));
        }

        #region Events

        /// <summary>
        /// Loads the sender information and saves the message to the cache
        /// Fires on "ReceiveMessage"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Received message object</param>
        private async void OnMessageReceived(object sender, Message message)
        {
            Debug.WriteLine($"{nameof(ChatHubService)}.{nameof(this.OnMessageReceived)} :: " +
                $"{message.Content} From {message.SenderId} To Team #{message.RecipientId} :: {message.CreationTime}");

            var teamId = message.RecipientId;

            // Loads user data of the sender
            message.Sender = await UserService.GetUser(message.SenderId);

            // Adds to message dictionary
            MessagesByConnectedTeam.AddOrUpdate(
                teamId,
                new List<Message>() { message },
                (key, list) =>
                {
                    list.Add(message);
                    return list;
                });

            // Invoke registered ui events
            MessageReceived?.Invoke(this, message);
        }

        /// <summary>
        /// Fires on "ReceiveInvitation"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="teamId">Id of the team that the user was invited to</param>
        private void OnInvitationReceived(object sender, uint teamId)
        {
            Debug.WriteLine($"{nameof(ChatHubService)}.{nameof(this.OnInvitationReceived)} :: " +
                $"Invited To Team #{teamId} :: Listening to Hub #{teamId}");

            // Add to the team list of the current user
            MessengerService
                .GetTeam(teamId)
                .ContinueWith(task =>
                {
                    if (task.IsCompletedSuccessfully)
                    {
                        var team = task.Result;
                        CurrentUser.Teams.Add(team);
                    }
                });

            // Invoke registered ui events
            InvitationReceived?.Invoke(this, teamId);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Safely creates a new entry in concurrent dictionary for a team
        /// </summary>
        /// <param name="messages">List of messages to initialize with</param>
        private void CreateEntryForCurrentTeam(IEnumerable<Message> messages)
        {
            LogContext.PushProperty("Method","CreateEntryForCurrentTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");

            MessagesByConnectedTeam.AddOrUpdate(
                (uint)CurrentTeamId,
                (key) =>
                {
                    List<Message> list = new List<Message>();
                    foreach (var message in messages)
                    {
                        list.Add(message);
                    }

                    return list;
                },
                (key, list) =>
                {
                    list.Clear();
                    foreach (var message in messages)
                    {
                        list.Add(message);
                    }

                    return list;
                });
        }

        #endregion
    }
}
