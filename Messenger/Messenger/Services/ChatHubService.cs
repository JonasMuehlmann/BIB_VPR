using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Context;
using Serilog;
using System.Linq;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System.Collections.ObjectModel;
using Messenger.Helpers.MessageHelpers;
using Messenger.Helpers.TeamHelpers;

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
        private UserViewModel _currentUser;
        private TeamViewModel _selectedTeam;
        private ChannelViewModel _selectedChannel;

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
        public TeamViewModel SelectedTeam
        {
            get { return _selectedTeam; }
            set { _selectedTeam = value; }
        }

        /// <summary>
        /// Currently selected channel view model
        /// </summary>
        public ChannelViewModel SelectedChannel
        {
            get { return _selectedChannel; }
            set { _selectedChannel = value; }
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

        public event EventHandler<MessageViewModel> MessageUpdated;

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

        public event EventHandler<IEnumerable<PrivateChatViewModel>> ChatsUpdated;

        /// <summary>
        /// Event handler for update at TeamDescription and TeamName
        /// </summary>
        public event EventHandler<TeamViewModel> TeamUpdated;

        public event EventHandler<ChannelViewModel> ChannelUpdated;

        #endregion

        /// <summary>
        /// Provides view models with teams/channels information and messages
        /// </summary>
        public ChatHubService()
        {
            /** MANAGERS: instances to hold messages/teams **/
            MessageManager = MessageManager.CreateMessageManager();
            TeamManager = TeamManager.CreateTeamManager();

            UserDataService.UserDataUpdated += (sender, user) =>
            {
                CurrentUser = user;
            };

            InitializeAsync();
        }

        /// <summary>
        /// Initialize the service
        /// loads the following data:
        /// • current user
        /// • my teams (broadcasted/list managed in TeamManager instance)
        /// • messages (entries created in MessageManager instance)
        /// </summary>
        private async void InitializeAsync()
        {
            LogContext.PushProperty("Method", $"{nameof(InitializeAsync)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Initializing ChatHubService");

            /** (REQUIRED) REGISTER LISTENERS **/
            MessengerService.RegisterListenerForMessages(OnMessageReceived);
            MessengerService.RegisterListenerForInvites(OnInvitationReceived);
            MessengerService.RegisterListenerForMessageUpdate(OnMessageUpdated);
            MessengerService.RegisterListenerForMessageDelete(OnMessageDeleted);
            MessengerService.RegisterListenerForChannelUpdate(OnChannelUpdated);

            /** (REQUIRED) LOAD USER **/
            CurrentUser = await UserDataService.GetUserAsync();
            TeamManager.CurrentUser = CurrentUser;
            MessageManager.CurrentUser = CurrentUser;

            /** (REQUIRED) LOAD TEAMS **/
            IReadOnlyCollection<TeamViewModel> teamViewModels = await GetMyTeams();

            /** EXIT IF NO TEAMS **/
            if (teamViewModels == null || teamViewModels.Count() <= 0) 
            {
                logger.Information($"Event {nameof(TeamsUpdated)} invoked with no team");

                TeamManager.Clear();
                TeamsUpdated?.Invoke(this, null);

                return;
            }

            /** LOAD MESSAGES FOR TEAMS **/
            foreach (TeamViewModel teamViewModel in TeamManager.MyTeams)
            {
                /* FOR EACH CHANNEL */
                foreach (ChannelViewModel channelViewModel in teamViewModel.Channels)
                {
                    /* ALWAYS LOAD FROM DATABASE */
                    var messages = await MessengerService.LoadMessages((uint)channelViewModel.ChannelId);

                    if (messages == null || messages.Count() <= 0)
                    {
                        continue;
                    }

                    /* ADD TO CACHE */
                    await MessageManager.AddMessage(messages);

                    /* GET LAST MESSAGE FROM CACHE */
                    if (MessageManager.TryGetLastMessage(channelViewModel.ChannelId, out MessageViewModel lastMessage))
                    {
                        channelViewModel.LastMessage = lastMessage;
                    }
                }
            }

            /** LOAD MESSAGES FOR PRIVATE CHATS **/
            foreach (PrivateChatViewModel chatViewModel in TeamManager.MyChats)
            {
                /* PRIVATE CHAT HAS ONLY ONE MAIN CHANNEL */
                /* ALWAYS LOAD FROM DATABASE */
                var messages = await MessengerService.LoadMessages(chatViewModel.MainChannel.ChannelId);

                if (messages == null || messages.Count() <= 0)
                {
                    continue;
                }

                /* ADD TO CACHE */
                await MessageManager.AddMessage(messages);

                /* GET LAST MESSAGE FROM CACHE */
                if (MessageManager.TryGetLastMessage(chatViewModel.MainChannel.ChannelId, out MessageViewModel lastMessage))
                {
                    chatViewModel.LastMessage = lastMessage;
                }
            }

            /** DEFAULT SELECTED TEAM/CHANNEL **/
            SelectedTeam = TeamManager.MyTeams.FirstOrDefault();
            SelectedChannel = SelectedTeam.Channels.FirstOrDefault();

            logger.Information($"Event {nameof(TeamsUpdated)} invoked with {teamViewModels.Count()} teams");

            /** BROADCAST MY TEAMS & MY CHATS **/
            TeamsUpdated?.Invoke(this, TeamManager.MyTeams);
            ChatsUpdated?.Invoke(this, TeamManager.MyChats);
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

            /** EXIT IF NO TEAM SELECTED **/
            if (SelectedTeam == null
                || SelectedChannel == null)
            {
                logger.Information("No current team set, exiting");
                logger.Information("Return value: null");

                return null;
            }

            uint channelId = SelectedChannel.ChannelId;

            /** FROM CACHE **/
            if (MessageManager.TryGetMessages(channelId, out ObservableCollection<MessageViewModel> fromCache)
                && fromCache.Count > 0)
            {
                /* GET FROM CACHE */
                logger.Information($"Return value: {fromCache}");

                return fromCache;
            }
            /** FROM DATABASE **/
            else
            {
                /* LOAD FROM DATABASE */
                var fromDb = await MessengerService.LoadMessages(channelId);

                if (fromDb == null)
                {
                    return null;
                }

                /* ADD TO CACHE */
                await MessageManager.AddMessage(fromDb);

                /* GET FROM CACHE */
                ObservableCollection<MessageViewModel> viewModels = MessageManager.GetMessages(channelId);

                logger.Information($"Return value: {viewModels}");

                return viewModels;
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
            message.RecipientId = (uint)SelectedChannel.ChannelId;

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

            /** EXIT IF NO USER **/
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
            /** FROM DATABASE **/
            else
            {
                /* LOAD FROM DATABASE */
                var teams = await MessengerService.LoadTeams(CurrentUser.Id);

                /* ADD TO CACHE */
                await TeamManager.AddTeam(teams);

                logger.Information($"Return value: {string.Join(", ", TeamManager.MyTeams)}");

                /* GET FROM CACHE */
                return TeamManager.MyTeams;
            }
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

            /** SAVE TO DATABASE **/
            uint? teamId = await MessengerService.CreateTeam(CurrentUser.Id, teamName, teamDescription);

            /** EXIT IF FAILED TO SAVE TO DATABASE **/
            if (teamId == null)
            {
                return;
            }

            /** LOAD CREATED TEAM FROM DATABASE **/
            Team team = await MessengerService.GetTeam((uint)teamId);

            /** SAVE TO CACHE **/
            TeamViewModel viewModel = await TeamManager.AddTeam(team);

            /** SWITCH TO MAIN CHANNEL OF THE TEAM **/
            await SwitchChannel(viewModel.Channels[0].ChannelId);

            logger.Information($"Event {nameof(TeamsUpdated)} invoked with {TeamManager.MyTeams.Count()} teams");

            /** TRIGGERS TEAM LIST COMPONENTS TO RELOAD **/
            TeamsUpdated?.Invoke(this, TeamManager.MyTeams);
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

            bool isSuccess = true;

            isSuccess &= await MessengerService.ChangeTeamName(teamName, (uint)SelectedTeam.Id);
            isSuccess &= await MessengerService.ChangeTeamDescription(teamDescription, (uint)SelectedTeam.Id);

            if (!isSuccess)
            {
                return;
            }

            SelectedTeam.TeamName = teamName;
            SelectedTeam.Description = teamDescription;

            TeamUpdated?.Invoke(this, SelectedTeam);
        }

        /// <summary>
        /// Updates CurrentTeam and invokes registered events(TeamSwitched)
        /// </summary>
        /// <param name="channelId">Id of the team to switch to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SwitchChannel(uint channelId)
        {
            LogContext.PushProperty("Method","SwitchTeam");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameter teamId={channelId}");

            /** GET CHANNEL AND TEAM FROM CACHE **/
            ChannelViewModel channel = TeamManager.GetChannel(channelId);
            TeamViewModel team = TeamManager.GetTeam(channel.TeamId);

            /** EXIT IF THE CHANNEL DOES NOT EXIST IN CACHE **/
            if (channel == null
                || team == null
                || team.Id == null)
            {
                return;
            }

            /** UPDATE SELECTED TEAM/CHANNEL **/
            SelectedTeam = team;
            SelectedChannel = channel;

            /** LOAD MESSAGES FOR CURRENT TEAM/CHANNEL **/
            IEnumerable<MessageViewModel> messages = await GetMessages();

            logger.Information($"Event {nameof(TeamSwitched)} invoked with {messages?.Count()} messages");

            /** TRIGGERS NAVIGATION AND MESSAGE CONTROLS **/
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

            /** SAVE TO DATABASE **/
            bool isSuccess = await MessengerService.CreateChannel(channelName, (uint)SelectedTeam.Id);

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

            /** REMOVE FROM DATABASE **/
            bool isSuccess = await MessengerService.RemoveChannel(channelId);

            /** REMOVE FROM CACHE **/
            MessageManager.RemoveEntry(channelId);

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

        public async Task<IReadOnlyCollection<PrivateChatViewModel>> GetMyChats()
        {
            LogContext.PushProperty("Method", $"{nameof(GetMyChats)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            /** EXIT IF NO USER **/
            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            /** FROM CACHE **/
            if (TeamManager.MyChats != null
                && TeamManager.MyChats.Count > 0)
            {
                return TeamManager.MyChats;
            }
            /** FROM DATABASE **/
            else
            {
                /* LOAD FROM DATABASE */
                var teams = await MessengerService.LoadTeams(CurrentUser.Id);

                /* ADD TO CACHE */
                await TeamManager.AddTeam(teams);

                logger.Information($"Return value: {string.Join(", ", TeamManager.MyChats)}");

                /* GET FROM CACHE */
                return TeamManager.MyChats;
            }
        }

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

            /** SAVE TO DATABASE **/
            uint? chatId = await MessengerService.StartChat(CurrentUser.Id, targetUserId);

            /** EXIT IF FAILED TO SAVE TO DATABASE **/
            if (chatId == null)
            {
                return false;
            }

            /** LOAD FROM DATABASE **/
            Team chat = await MessengerService.GetTeam((uint)chatId);

            /** ADD TO CACHE **/
            PrivateChatViewModel viewModel = (PrivateChatViewModel)await TeamManager.AddTeam(chat);

            /** TRIGGERS CHAT LIST COMPONENTS TO RELOAD **/
            TeamsUpdated?.Invoke(this, TeamManager.MyChats);

            /** SWITCH TO CHAT **/
            await SwitchChannel(viewModel.MainChannel.ChannelId);

            return true;
        }

        /// <summary>
        /// Updates SelectedTeam and SelectedChannel for the selected private chat
        /// </summary>
        /// <param name="channelId">Id of the team to switch to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SwitchChat(uint chatId)
        {
            LogContext.PushProperty("Method", "SwitchChat");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameter chatId={chatId}");

            /** GET FROM CACHE **/
            PrivateChatViewModel viewModel = TeamManager.GetChat(chatId);

            /** UPDATES TEAM AS PRIVATE CHAT AND SETS CHANNEL TO MAIN **/
            SelectedTeam = viewModel;
            SelectedChannel = viewModel.MainChannel;

            /** LOAD MESSAGES **/
            ObservableCollection<MessageViewModel> messages = await GetMessages();

            logger.Information($"Event {nameof(TeamSwitched)} invoked with {messages?.Count()} messages");

            /** TRIGGERS NAVIGATION AND MESSAGE CONTROLS **/
            TeamSwitched?.Invoke(this, messages);
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

            /** EXIT IF MESSAGE IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            /** ADD TO CACHE **/
            MessageViewModel viewModel = await MessageManager.AddMessage(message);

            logger.Information($"Event {nameof(MessageReceived)} invoked with message: {viewModel}");

            /** BROADCAST VIEWMODEL TO SUBSCRIBERS **/
            MessageReceived?.Invoke(this, viewModel);
        }

        /// <summary>
        /// Fires on "ReceiveInvitation"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="teamId">Id of the team that the user was invited to</param>
        private async void OnInvitationReceived(object sender, uint teamId)
        {
            LogContext.PushProperty("Method", $"{nameof(OnInvitationReceived)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Event fired: {nameof(OnInvitationReceived)}");

            /** LOAD TEAM FROM DATABASE **/
            var team = await MessengerService.GetTeam(teamId);

            dynamic viewModel = await TeamManager.AddTeam(team);

            /** TRIGGER CHATS UPDATED EVENT **/
            if (viewModel is PrivateChatService)
            {
                ChatsUpdated?.Invoke(this, TeamManager.MyChats);
            }
            /** TRIGGER TEAMS UPDATED EVENT **/
            else
            {
                TeamsUpdated?.Invoke(this, TeamManager.MyTeams);
            }

            /** TRIGGER GENERAL EVENT **/
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

            /** UPDATE IN CACHE **/
            MessageViewModel viewModel = await MessageManager.UpdateMessage(message);

            /** TRIGGERS MESSAGE CONTROLS TO UPDATE **/
            MessageUpdated?.Invoke(this, viewModel);
        }

        /// <summary>
        /// Fires on "MessageDeleted"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Id of the team that the user was invited to</param>
        private void OnMessageDeleted(object sender, Message message)
        {
            if (message == null)
            {
                return;
            }

            /** REMOVE FROM CACHE **/
            MessageManager.RemoveMessage(message);

            /** TRIGGERS MESSAGE CONTROLS TO RELOAD **/
            MessageDeleted?.Invoke(this, EventArgs.Empty);
        }

        private void OnChannelUpdated(object sender, Channel channel)
        {
            if (channel == null)
            {
                return;
            }

            /** ADD TO CACHE **/
            ChannelViewModel viewModel = TeamManager.AddChannel(channel);

            /** TRIGGERS CHANNEL LIST CONTROLS TO UPDATE **/
            ChannelUpdated?.Invoke(this, viewModel);
        }

        #endregion
    }
}
