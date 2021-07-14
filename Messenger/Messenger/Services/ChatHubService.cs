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
using Messenger.Helpers;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using System.Collections.ObjectModel;

namespace Messenger.Services
{
    /// <summary>
    /// Service aggregator and container class for the state management
    /// View models can subscribe to this service for corresponding ui events (MessageReceived, InvitationReceived, etc.)
    /// </summary>
    public class ChatHubService
    {
        #region Private

        private ILogger logger = GlobalLogger.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private MessageBuilder MessageBuilder;
        private TeamBuilder TeamBuilder;
        private TeamViewModel _currentTeam;
        private UserViewModel _currentUser;
        private ChannelViewModel _currentChannel;

        /// <summary>
        /// Instance to hold/manage messages
        /// </summary>
        private MessageManager MessageManager { get; }

        /// <summary>
        /// Instance to hold/manage teams
        /// </summary>
        private TeamManager TeamManager { get; }

        #endregion

        #region Properties


        /// <summary>
        /// Currently selected team view model
        /// </summary>
        public TeamViewModel CurrentTeam
        {
            get { return _currentTeam; }
            set { _currentTeam = value; }
        }

        /// <summary>
        /// Currently selected channel view model
        /// </summary>
        public ChannelViewModel CurrentChannel
        {
            get { return _currentChannel; }
            set { _currentChannel = value; }
        }

        /// <summary>
        /// Currently logged in user
        /// </summary>
        public UserViewModel CurrentUser
        {
            get { return _currentUser; }
            set { _currentUser = value; }
        }

        /// <summary>
        /// Represents the current states of the service
        /// - Loading: user or teams have not been loaded yet
        /// - NoDataFound: user is loaded with empty teams list
        /// - LoadedWithData: user is loaded with teams list
        /// </summary>
        public ChatHubConnectionState ConnectionState
        {
            get
            {
                if (CurrentUser == null || TeamManager.MyTeams == null)
                {
                    return ChatHubConnectionState.Loading;
                }
                else if (TeamManager.MyTeams.Count == 0)
                {
                    return ChatHubConnectionState.NoDataFound;
                }
                else
                {
                    return ChatHubConnectionState.LoadedWithData;
                }
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for messages
        /// </summary>
        public event EventHandler<MessageViewModel> MessageReceived;

        public event EventHandler MessageUpdated;

        public event EventHandler MessageDeleted;

        /// <summary>
        /// Event handler for "ReceiveInvitation"(SignalR)
        /// </summary>
        public event EventHandler<uint> InvitationReceived;

        /// <summary>
        /// Event handler for switching teams
        /// </summary>
        public event EventHandler<IEnumerable<MessageViewModel>> TeamSwitched;

        /// <summary>
        /// Event handler for updates in teams list
        /// </summary>
        public event EventHandler<IEnumerable<TeamViewModel>> TeamsUpdated;

        /// <summary>
        /// Event handler for update at TeamDescription and TeamName
        /// </summary>
        public event EventHandler<TeamViewModel> TeamUpdated;

        #endregion

        /// <summary>
        /// Provides view models with teams/channels information and messages
        /// </summary>
        public ChatHubService()
        {
            /** MANAGERS: instances to hold messages/teams **/
            MessageManager = new MessageManager();
            TeamManager = new TeamManager();

            /** BUILDERS: converts DB data from MessengerService into corresponding view models **/
            MessageBuilder = new MessageBuilder();
            TeamBuilder = new TeamBuilder();

            InitializeAsync();
        }

        /// <summary>
        /// Initialize the service
        /// loads the following data:
        /// - current user
        /// - my teams (broadcasted/list managed in TeamManager instance)
        /// - messages (entries created in MessageManager instance)
        /// </summary>
        private async void InitializeAsync()
        {
            LogContext.PushProperty("Method", $"{nameof(InitializeAsync)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Initializing ChatHubService");

            /** REGISTER LISTENERS **/
            MessengerService.RegisterListenerForMessages(OnMessageReceived);
            MessengerService.RegisterListenerForInvites(OnInvitationReceived);
            MessengerService.RegisterListenerForMessageUpdate(OnMessageUpdated);
            MessengerService.RegisterListenerForMessageDelete(OnMessageDeleted);

            /** (REQUIRED) LOAD USER **/
            CurrentUser = await UserDataService.GetUserAsync();

            /** (REQUIRED) LOAD TEAMS **/
            /* TeamManager <= MessengerService */
            var teamViewModels = await GetMyTeams();

            if (teamViewModels == null || teamViewModels.Count() <= 0) 
            {
                logger.Information($"Event {nameof(TeamsUpdated)} invoked with no team");

                TeamManager.Clear();
                TeamsUpdated?.Invoke(this, null);

                return; // Exit if user has no team
            }

            /** (IF TEAMS) LOAD MESSAGES **/
            /* MessageManager <= MessengerService */
            foreach (TeamViewModel teamViewModel in teamViewModels)
            {
                foreach (ChannelViewModel channelViewModel in teamViewModel.Channels)
                {
                    var messages = await MessengerService.LoadMessages((uint)channelViewModel.ChannelId);

                    if (messages == null)
                    {
                        continue;
                    }

                    var vms = await MessageBuilder.Build(messages, CurrentUser); // Convert to ViewModel

                    var parents = MessageBuilder.AssignReplies(vms);

                    channelViewModel.LastMessage = parents.LastOrDefault(); // Set last message content for the team

                    MessageManager.CreateEntry(channelViewModel.ChannelId, parents); // Messages loaded in MessageManager
                }
            }

            TeamManager.AddTeam(teamViewModels); // Add teams list to cache
            CurrentTeam = TeamManager.MyTeams.FirstOrDefault(); // First team as current team
            CurrentChannel = CurrentTeam.Channels.FirstOrDefault(); // First channel as current channel

            logger.Information($"Event {nameof(TeamsUpdated)} invoked with {teamViewModels.Count()} teams");
            TeamsUpdated?.Invoke(this, TeamManager.MyTeams); // Broadcast readonly list of my teams
        }

        #region Message

        /// <summary>
        /// Gets all messages of the current team in two possible ways:
        /// • loads from the cache dwelling in MessageManager
        /// • loads from the server database
        /// </summary>
        /// <returns>List of messages</returns>
        public async Task<ObservableCollection<MessageViewModel>> GetMessages()
        {
            LogContext.PushProperty("Method",$"{nameof(GetMessages)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            /** NO TEAM SELECTED **/
            if (CurrentTeam == null
                || CurrentChannel == null)
            {
                logger.Information("No current team set, exiting");
                logger.Information("Return value: null");

                return null;
            }

            /** CHECK IN CACHE **/
            uint channelId = (uint)CurrentChannel.ChannelId;

            if (MessageManager.TryGetMessages(channelId, out ObservableCollection<MessageViewModel> fromCache))
            {
                /** LOAD FROM CACHE **/
                logger.Information($"Return value: {fromCache}");

                return fromCache;
            }
            else
            {
                /** LOAD FROM DB **/
                var fromDb = await MessengerService.LoadMessages(channelId);

                if (fromDb == null)
                {
                    return null;
                }

                var vms = await MessageBuilder.Build(fromDb, CurrentUser); // Must be converted to view models from database models

                var parents = MessageBuilder.AssignReplies(vms);

                MessageManager.CreateEntry(channelId, parents);

                logger.Information($"Return value: {parents}");

                return new ObservableCollection<MessageViewModel>(parents);
            }
        }

        /// <summary>
        /// Invokes MessengerService to send given Message object,
        /// marks following information before forwarded:
        /// • CurrentUser.Id as SenderId
        /// • CurrentTeam.Id as RecipientId
        /// </summary>
        /// <param name="message">New Message object to send</param>
        /// <returns>True on success, false on error</returns>
        public async Task<bool> SendMessage(Message message)
        {
            LogContext.PushProperty("Method", $"{nameof(SendMessage)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentUser == null)
            {
                logger.Information($"Error while fetching user data");
                logger.Information($"Return value: false");
                return false; // Exit if no user
            }

            message.SenderId = CurrentUser.Id;
            message.RecipientId = (uint)CurrentTeam.Id;

            bool isSuccess = await MessengerService.SendMessage(message);

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
        }

        /// <summary>
        /// Invokes MessengerService to edit the message content with the given string value
        /// </summary>
        /// <param name="messageId">Id of the message</param>
        /// <param name="newContent">New content string</param>
        /// <returns>True if successful, else false</returns>
        public async Task<bool> EditMessage(uint messageId, string newContent)
        {
            LogContext.PushProperty("Method", $"{nameof(SendMessage)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentUser == null)
            {
                logger.Information($"Return value: false");
                return false;
            }

            bool isSuccess = await MessengerService.EditMessage(messageId, newContent);

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
        }

        /// <summary>
        /// Invokes MessengerService to delete the message from the database
        /// </summary>
        /// <param name="messageId">Id of the message</param>
        /// <returns>True if successful, else false</returns>
        public async Task<bool> DeleteMessage(uint messageId)
        {
            LogContext.PushProperty("Method", $"{nameof(DeleteMessage)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentUser == null)
            {
                logger.Information($"Return value: false");
                return false;
            }

            bool isSuccess = await MessengerService.DeleteMessage(messageId);

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
        }

        /// <summary>
        /// Invokes MessengerService to add reaction to the message,
        /// following information will be forwarded from the service:
        /// • CurrentUser.Id as UserId
        /// </summary>
        /// <param name="messageId">Id of the message</param>
        /// <param name="type">Type of reaction to be added</param>
        /// <returns>True if succesful, else false</returns>
        public async Task<bool> MakeReaction(uint messageId, ReactionType type)
        {
            LogContext.PushProperty("Method", $"{nameof(MakeReaction)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentUser == null)
            {
                logger.Information($"Return value: false");
                return false;
            }

            uint? isSuccess = await MessengerService.AddReaction(messageId, CurrentUser.Id, type.ToString());

            logger.Information($"Return value: {isSuccess}");

            return isSuccess != null ? true : false;
        }

        /// <summary>
        /// Invokes MessengerService to remove reaction to the message,
        /// following information will be forwarded from the service:
        /// • CurrentUser.Id as UserId
        /// </summary>
        /// <param name="messageId">Id of the message</param>
        /// <param name="type">Type of reaction to be removed</param>
        /// <returns>True if succesful, else false</returns>
        public async Task<bool> RemoveReaction(uint messageId, ReactionType type)
        {
            LogContext.PushProperty("Method", $"{nameof(MakeReaction)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentUser == null)
            {
                logger.Information($"Return value: false");
                return false;
            }

            bool isSuccess = await MessengerService.RemoveReaction(messageId, CurrentUser.Id, type.ToString());

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
        }


        #endregion

        #region Team

        /// <summary>
        /// Loads teams from the database and converts to view models
        /// </summary>
        /// <returns>List of team view models</returns>
        public async Task<IReadOnlyCollection<TeamViewModel>> GetMyTeams()
        {
            LogContext.PushProperty("Method",$"{nameof(GetMyTeams)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            /** EXIT: NO USER **/
            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            /** FROM CACHE **/
            if (TeamManager.MyTeams != null
                && TeamManager.MyTeams.Count > 0)
            {
                return TeamManager.MyTeams;
            }

            /** FROM DB **/
            var fromDB = await MessengerService.LoadTeams(CurrentUser.Id);
            var viewModels = await TeamBuilder.Build(fromDB, CurrentUser.Id);

            logger.Information($"Return value: {string.Join(", ", viewModels)}");

            return viewModels;
        }

        /// <summary>
        /// Creates a new team and invokes registered events(TeamsUpdated)
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task CreateTeam(string teamName, string teamDescription)
        {
            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamName={teamName}, teamDescription={teamDescription}");

            uint? teamId = await MessengerService.CreateTeam(CurrentUser.Id, teamName, teamDescription);

            if (teamId != null)
            {
                var channels = await MessengerService.GetChannelsForTeam((uint)teamId);
                Channel mainChannel = channels.FirstOrDefault();

                await SwitchChannel((uint)teamId, mainChannel.ChannelId); // Switch to main channel of the newly created team
            }

            var teams = await GetMyTeams();

            logger.Information($"Event {nameof(TeamsUpdated)} invoked with {teams?.Count()} teams");

            TeamsUpdated?.Invoke(this, teams);
        }

        /// <summary>
        /// Updates the teamName and teamDescription of the current tem
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateTeam(string teamName, string teamDescription)
        {
            LogContext.PushProperty("Method", "UpdateTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamName={teamName}, teamDescription={teamDescription}");

            await MessengerService.ChangeTeamName(teamName, (uint)CurrentTeam.Id);
            await MessengerService.ChangeTeamDescription(teamDescription, (uint)CurrentTeam.Id);

            CurrentTeam.TeamName = teamName;
            CurrentTeam.Description = teamDescription;

            TeamUpdated?.Invoke(this, CurrentTeam);
        }

        /// <summary>
        /// Updates CurrentTeam and invokes registered events(TeamSwitched)
        /// </summary>
        /// <param name="channelId">Id of the team to switch to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SwitchChannel(uint teamId, uint channelId)
        {
            LogContext.PushProperty("Method","SwitchTeam");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameter teamId={channelId}");

            IReadOnlyCollection<TeamViewModel> myTeams = await GetMyTeams();
            TeamViewModel targetTeam = myTeams.Where(t => t.Id == teamId).FirstOrDefault();
            ChannelViewModel targetChannel = targetTeam.Channels.Where(c => c.ChannelId == channelId).FirstOrDefault();

            CurrentTeam = targetTeam; // Set current team
            CurrentChannel = targetChannel; // Set current channel

            IEnumerable<MessageViewModel> messages = await GetMessages(); // Get messages for current channel

            logger.Information($"Event {nameof(TeamSwitched)} invoked with {messages?.Count()} messages");
            TeamSwitched?.Invoke(this, messages);
        }

        /// <summary>
        /// Creates a new channel with the given channel name
        /// </summary>
        /// <param name="channelName">Name of the new channel</param>
        /// <returns>Task to await</returns>
        public async Task<bool> CreateChannel(string channelName)
        {
            LogContext.PushProperty("Method", "CreateChannel");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameter channelName={channelName}");

            bool isSuccess = await MessengerService.CreateChannel(channelName, (uint)CurrentTeam.Id); // Create entry in DB

            TeamsUpdated?.Invoke(this, await GetMyTeams()); // Reload

            return isSuccess;
        }

        /// <summary>
        /// (Destructive) Deletes a channel by its channel id,
        /// this also deletes all the messages in the channel
        /// </summary>
        /// <param name="channelId">Id of the channel to be deleted</param>
        /// <returns>Task to await</returns>
        public async Task<bool> RemoveChannel(uint channelId)
        {
            LogContext.PushProperty("Method", "RemoveChannel");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameter channelId={channelId}");

            bool isSuccess = await MessengerService.RemoveChannel(channelId); // Remove from DB

            MessageManager.RemoveEntry(channelId); // Remove from the cache

            TeamsUpdated?.Invoke(this, await GetMyTeams()); // Reload

            return isSuccess;
        }

        #endregion

        #region Member

        /// <summary>
        /// Adds the user to the target team
        /// </summary>
        /// <param name="invitation">Model to build required fields, used only under UI-logic</param>
        /// <returns>True on success, false on error</returns>
        public async Task<bool> InviteUser(Invitation invitation)
        {
            LogContext.PushProperty("Method",$"{nameof(InviteUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameter invitation={invitation}");

            var isSuccess = await MessengerService.InviteUser(invitation.UserId, invitation.TeamId);

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
        }

        /// <summary>
        /// Removes a user from a specific Team
        /// </summary>
        /// <param name="userId">Id of the user to be removed</param>
        /// <param name="teamId">Id of the team</param>
        /// <returns>True on success, false on error</returns>
        public async Task<bool> RemoveUser(string userId, uint teamId)
        {
            LogContext.PushProperty("Method", $"{nameof(RemoveUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters userId={userId}");

            var isSuccess = await MessengerService.RemoveUser(userId, teamId);

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
        }

        /// <summary>
        /// Returns a user by username and nameId
        /// </summary>
        /// <param name="username">DisplayName of the user</param>
        /// <param name="nameId">NameId of the user</param>
        /// <returns>List of User objects</returns>
        public async Task<User> GetUser(string username, uint nameId)
        {
            LogContext.PushProperty("Method", $"{nameof(GetUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters username={username}, nameId={nameId}");

            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            var user = await MessengerService.GetUserWithNameId(username, nameId);

            logger.Information($"Return value: {user}");

            return user;
        }

        /// <summary>
        /// Get all team Members of a team
        /// </summary>
        /// <param name="teamId">Id of the team</param>
        /// <returns>List of User objects</returns>
        public async Task<IEnumerable<User>> GetTeamMembers(uint teamId)
        {
            LogContext.PushProperty("Method", $"{nameof(GetTeamMembers)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function with parameters teamId={teamId}");

            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            logger.Information($"Return value: {teamId}");

            return await MessengerService.LoadTeamMembers(teamId);
        }

        /// <summary>
        /// Search for users matching the given user name
        /// </summary>
        /// <param name="username">User name to search for</param>
        /// <returns>String of user name with name id</returns>
        public async Task<IList<string>> SearchUser(string username)
        {
            LogContext.PushProperty("Method", $"{nameof(SearchUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

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
            LogContext.PushProperty("Method", $"{nameof(GetUserWithNameId)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters username={username}, nameId={nameId}");

            var user = await MessengerService.GetUserWithNameId(username, nameId); // Load from DB

            logger.Information($"Return value: {user}");

            return user;
        }

        #endregion

        #region PrivateChat

        /// <summary>
        /// Start a new chat and invokes registered events(TeamsUpdated)
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>True if successful, else false</returns>
        public async Task<bool> StartChat(string targetUserId)
        {
            LogContext.PushProperty("Method", $"{nameof(SearchUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters targetUserId={targetUserId}");

            uint? chatId = await MessengerService.StartChat(CurrentUser.Id, targetUserId);

            if (chatId == null)
            {
                return false; // Exit if entry was not created in DB
            }

            Team chat = await MessengerService.GetTeam((uint)chatId); // Get created chat from DB
            TeamViewModel viewModel = await TeamBuilder.Build(chat, CurrentUser.Id); // Convert to view model
            ChannelViewModel singleChannel = viewModel.Channels.FirstOrDefault();

            TeamManager.AddTeam(viewModel); // Add to cache

            TeamsUpdated?.Invoke(this, await GetMyTeams()); // Reload

            await SwitchChannel((uint)viewModel.Id, singleChannel.ChannelId); // Switch to the created chat

            return true;
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
            LogContext.PushProperty("Method", $"{nameof(OnMessageReceived)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Event fired: {nameof(OnMessageReceived)}");

            bool isValid = message != null
                && !string.IsNullOrEmpty(message.SenderId);

            if (!isValid)
            {
                return;
            }

            MessageViewModel vm = await MessageBuilder.Build(message, CurrentUser); // Convert to view model

            MessageManager.Add(vm); // Add to cache

            logger.Information($"Event {nameof(MessageReceived)} invoked with message: {message}");

            MessageReceived?.Invoke(this, vm); // Reload
        }

        /// <summary>
        /// Fires on "ReceiveInvitation"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="teamId">Id of the team that the user was invited to</param>
        private void OnInvitationReceived(object sender, uint teamId)
        {
            LogContext.PushProperty("Method", $"{nameof(OnInvitationReceived)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Event fired: {nameof(OnInvitationReceived)}");

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

            logger.Information($"Event {nameof(InvitationReceived)} invoked with teamId: {teamId}");

            // Invoke registered events
            InvitationReceived?.Invoke(this, teamId);
        }

        /// <summary>
        /// Fires on "MessageUpdated"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Id of the team that the user was invited to</param>
        private async void OnMessageUpdated(object sender, Message message)
        {
            if (message == null)
            {
                return;
            }

            MessageViewModel vm = await MessageBuilder.Build(message, CurrentUser); // Convert to view model

            MessageManager.Update(vm); // Update cache

            MessageUpdated?.Invoke(this, EventArgs.Empty); // Reload
        }

        /// <summary>
        /// Fires on "MessageDeleted"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Id of the team that the user was invited to</param>
        private void OnMessageDeleted(object sender, Message message)
        {
            bool isValid = message != null;

            if (!isValid)
            {
                return;
            }

            MessageManager.Remove(message); // Remove from cache

            MessageDeleted?.Invoke(this, EventArgs.Empty); // Reload
        }

        #endregion
    }
}
