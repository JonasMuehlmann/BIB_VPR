using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog.Context;
using Serilog;
using System.Linq;
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

        /// <summary>
        /// Event handler for updates in teams list
        /// </summary>
        public event EventHandler<IEnumerable<Team>> TeamsUpdated;

        #endregion

        public ChatHubService()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<uint, List<Message>>();
            UserDataService.UserDataUpdated += OnLoggedIn;

            Initialize();
        }

        private async void Initialize()
        {
            CurrentUser = await UserDataService.GetUserAsync();

            // Loads messages for teams the current user has joined
            if (CurrentUser.Teams.Count > 0)
            {
                foreach (Team team in CurrentUser.Teams)
                {
                    var messages = await MessengerService.LoadMessages(team.Id);
                    CreateEntryForCurrentTeam(team.Id, messages);
                }

                // Sets the first team as the selected team
                CurrentTeamId = CurrentUser.Teams.FirstOrDefault().Id;
            }

            MessengerService.RegisterListenerForMessages(OnMessageReceived);
            MessengerService.RegisterListenerForInvites(OnInvitationReceived);
        }

        private void OnLoggedIn(object sender, UserViewModel user)
        {
            CurrentUser = user;
        }

        #region Message

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
                CreateEntryForCurrentTeam((uint)CurrentTeamId, fromDb);

                logger.Information($"Return value: {fromDb}");

                return fromDb;
            }
        }

        /// <summary>
        /// Sends a message to the current team
        /// </summary>
        /// <param name="content"></param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SendMessage(string content)
        {
            var message = new Message()
            {
                Content = content,
                CreationTime = DateTime.Now,
                SenderId = CurrentUser.Id,
                RecipientId = (uint)CurrentTeamId
            };

            await MessengerService.SendMessage(message);
        }

        #endregion

        #region Team

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
          
            var teams = await MessengerService.LoadTeams(CurrentUser.Id);

            // Updates the teams list under the current user
            CurrentUser.Teams.Clear();
            foreach (var team in teams)
            {
                CurrentUser.Teams.Add(team);
            }

            return teams;
        }


        /// <summary>
        /// Creates a new team and invokes registered events(TeamsUpdated)
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="teamDescription"></param>
        /// <returns></returns>
        public async Task CreateTeam(string teamName, string teamDescription)
        {
            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamName={teamName}, teamDescription={teamDescription}");

            uint? teamId = await MessengerService.CreateTeam(CurrentUser.Id, teamName, teamDescription);

            if (teamId != null)
            {
                await SwitchTeam((uint)teamId);
            }
            TeamsUpdated?.Invoke(this, await GetTeamsList());
        }

        /// <summary>
        /// Updates current team id and invokes registered events(TeamSwitched)
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

        #endregion

        #region Member

        /// <summary>
        /// Adds the user to the target team
        /// </summary>
        /// <param name="invitation">Model to build required fields, used only under UI-logic</param>
        /// <returns>Task to be awaited</returns>
        public async Task InviteUser(Invitation invitation)
        {
            LogContext.PushProperty("Method","InviteUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter invitation={invitation}");

            await MessengerService.InviteUser(invitation.UserId, Convert.ToUInt32(invitation.TeamId));
        }

        /// <summary>
        /// Removes a user from a specific Team
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="teamId"></param>
        /// <returns>Task to be awaited</returns>
        public async Task RemoveUser(string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "RemoveUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");


            await MessengerService.RemoveUser(userId, teamId);
        }

        /// <summary>
        /// Get all team Members of a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public async Task<IEnumerable<User>> GetTeamMembers(uint teamId)
        {
            LogContext.PushProperty("Method", "GetTeamMembers");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");

            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            return await MessengerService.LoadTeamMembers(teamId);
        }


        #endregion

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

            // Invoke registered events
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

            // Invoke registered events
            InvitationReceived?.Invoke(this, teamId);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Safely creates a new entry in concurrent dictionary for a team
        /// </summary>
        /// <param name="teamId">Id of the team for the entry</param>
        /// <param name="messages">List of messages to initialize with</param>
        private void CreateEntryForCurrentTeam(uint teamId, IEnumerable<Message> messages)
        {
            LogContext.PushProperty("Method","CreateEntryForCurrentTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");

            MessagesByConnectedTeam.AddOrUpdate(
                teamId,
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
