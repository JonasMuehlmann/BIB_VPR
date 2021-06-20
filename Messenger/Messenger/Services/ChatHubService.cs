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

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            CurrentUser = await UserDataService.GetUserAsync();

            var teams = await MessengerService.LoadTeams(CurrentUser.Id);

            teams = teams.Select((team) =>
            {
                Task.Run(async () =>
                {
                    var members = await MessengerService.LoadTeamMembers(team.Id);
                    SetMembers(ref team, members);
                }).GetAwaiter().GetResult();

                return team;
            });

            CurrentUser.Teams = teams.ToList();

            foreach (Team team in teams)
            {
                var messages = await MessengerService.LoadMessages(team.Id);
                CreateEntryForCurrentTeam(team.Id, messages);
            }

            // Sets the first team as the selected team
            CurrentTeamId = CurrentUser.Teams.FirstOrDefault().Id;

            // Broadcast Teams
            TeamsUpdated?.Invoke(this, CurrentUser.Teams);

            MessengerService.RegisterListenerForMessages(OnMessageReceived);
            MessengerService.RegisterListenerForInvites(OnInvitationReceived);
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
        /// <param name="content">Content to be written in the message</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SendMessage(Message message)
        {
            if (CurrentUser == null)
            {
                logger.Information($"Error while fetching user data");
                return;
            }

            // Set sender and recipient ids
            message.SenderId = CurrentUser.Id;
            message.RecipientId = (uint)CurrentTeamId;

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

            teams = teams.Select((team) =>
            {
                Task.Run(async () =>
                {
                    var members = await MessengerService.LoadTeamMembers(team.Id);
                    SetMembers(ref team, members);
                }).GetAwaiter().GetResult();

                return team;
            });

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
        public async Task SwitchTeam(uint? teamId)
        {
            LogContext.PushProperty("Method","SwitchTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter teamId={teamId}");

            CurrentTeamId = teamId;
            // Invokes ui events with the list of messages of the team
            TeamSwitched?.Invoke(this, await GetMessages());
        }

        /// <summary>
        /// Returns the current team model from the loaded list
        /// </summary>
        /// <returns>A complete team object</returns>
        public Team GetCurrentTeam()
        {
            if (CurrentTeamId == null)
            {
                return null;
            }

            var currentTeam = CurrentUser.Teams
                .Where(t => t.Id == CurrentTeamId)
                .FirstOrDefault();

            return currentTeam;
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

            await MessengerService.InviteUser(invitation.UserId, invitation.TeamId);
        }

        /// <summary>
        /// Removes a user from a specific Team
        /// </summary>
        /// <param name="userId">Id of the user to be removed</param>
        /// <param name="teamId">Id of the team</param>
        /// <returns>Task to be awaited</returns>
        public async Task RemoveUser(string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "RemoveUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            await MessengerService.RemoveUser(userId, teamId);
        }

        /// <summary>
        /// Returns a user by username and nameId
        /// </summary>
        /// <param name="username">DisplayName of the user</param>
        /// <param name="nameId">NameId of the user</param>
        /// <returns>List of User objects</returns>
        public async Task<IList<User>> GetUser(string username, uint nameId)
        {
            LogContext.PushProperty("Method", "RemoveUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters username={username}, nameId={nameId}");

            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            return await UserService.GetUser(username, nameId);
        }

        /// <summary>
        /// Get all team Members of a team
        /// </summary>
        /// <param name="teamId">Id of the team</param>
        /// <returns>List of User objects</returns>
        public async Task<IEnumerable<User>> GetTeamMembers(uint teamId)
        {
            LogContext.PushProperty("Method", "GetTeamMembers");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function with parameters teamId={teamId}");

            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            return await MessengerService.LoadTeamMembers(teamId);
        }

        /// <summary>
        /// Search for users matching the given user name
        /// </summary>
        /// <param name="username">User name to search for</param>
        /// <returns>String of user name with name id</returns>
        public async Task<IList<string>> SearchUser(string username)
        {
            LogContext.PushProperty("Method", "SearchUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters username={username}");

            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            return await UserService.SearchUser(username);
        }

        /// <summary>
        /// Get user with a valid user name and name id;
        /// </summary>
        /// <param name="username">DisplayName of the user</param>
        /// <param name="nameId">NameId of the user</param>
        /// <returns>A complete user object</returns>
        public async Task<User> GetUserWithNameId(string username, uint nameId)
        {
            var user = await MessengerService.GetUserWithNameId(username, nameId);

            return user;
        }

        #endregion

        #region PrivateChat

        /// <summary>
        /// Start a new chat and invokes registered events(TeamsUpdated)
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="teamDescription"></param>
        /// <returns></returns>
        public async Task StartChat(string userName, uint nameId)
        {
            LogContext.PushProperty("Method", "StartChat");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userName={userName}, nameId={nameId}");

            uint? chatId = await MessengerService.StartChat(CurrentUser.Id, userName, nameId);

            if (chatId != null)
            {
                await SwitchTeam((uint)chatId);
            }

            var chat = await MessengerService.GetTeam((uint)chatId);
            var members = await MessengerService.LoadTeamMembers(chat.Id);

            SetMembers(ref chat, members);

            CurrentUser.Teams.Add(chat);

            TeamsUpdated?.Invoke(this, CurrentUser.Teams);
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

        /// <summary>
        /// Determines the type of team and sets the corresponding member models
        /// </summary>
        /// <param name="team">Team object to reference to</param>
        /// <param name="members">List of members to set</param>
        private void SetMembers(ref Team team, IEnumerable<User> members)
        {
            // If it is a private chat, exclude current user id
            if (string.IsNullOrEmpty(team.Name))
            {
                team.Members = members
                    .Where(m => m.Id != CurrentUser.Id)
                    .ToList();
            }
            else
            {
                team.Members = members.ToList();
            }
        }

        #endregion
    }
}
