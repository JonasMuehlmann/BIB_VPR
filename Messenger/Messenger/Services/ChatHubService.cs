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
using Messenger.Helpers.TeamHelpers;
using Messenger.Services.Providers;
using Messenger.Helpers;

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

        #endregion

        #region Properties

        /// <summary>
        /// Represents the current states of the service
        /// • Loading: user or teams have not been loaded yet
        /// • NoDataFound: user is loaded with empty teams list
        /// • LoadedWithData: user is loaded with teams list
        /// </summary>
        public ChatHubConnectionState ConnectionState
        {
            get
            {
                if (App.StateProvider.CurrentUser == null
                    || CacheQuery.GetMyTeams() == null
                    || CacheQuery.GetMyChats() == null)
                {
                    return ChatHubConnectionState.Loading;
                }
                else if (CacheQuery.GetMyTeams().Count()
                    + CacheQuery.GetMyChats().Count() <= 0)
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

        /// <summary>
        /// Provides view models with teams/channels information and messages
        /// </summary>
        public ChatHubService()
        {
            InitializeAsync();
        }

        /// <summary>
        /// Initializes the cache and loads following data:
        /// • Teams (Queryable via CacheQuery)
        /// • Private Chats (Queryable via CacheQuery)
        /// • Messages (Queryable via CacheQuery)
        /// </summary>
        private async void InitializeAsync()
        {
            LogContext.PushProperty("Method", $"{nameof(InitializeAsync)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Initializing ChatHubService");

            /** LOAD ALL TEAMS FROM DATABASE (TEAMS & CHATS) **/
            UserViewModel currentUser = await UserDataService.GetUserAsync();
            IEnumerable<Team> data = await MessengerService.GetTeams(currentUser.Id);

            /* EXIT IF NO DATA */
            if (data == null || data.Count() <= 0)
            {
                return;
            }

            /** SAVE TO CACHE **/
            IEnumerable<TeamViewModel> viewModels = await CacheQuery.AddOrUpdate<IEnumerable<TeamViewModel>>(data);

            /** LOAD MESSAGES FROM DATABASE **/
            IEnumerable<TeamViewModel> teams = CacheQuery.GetMyTeams();
            IEnumerable<PrivateChatViewModel> chats = CacheQuery.GetMyChats();

            /** LOAD MESSAGES FOR TEAMS **/
            if (teams != null && teams.Count() > 0)
            {
                foreach (TeamViewModel teamViewModel in teams)
                {
                    foreach (ChannelViewModel channelViewModel in teamViewModel.Channels)
                    {
                        /* LOAD FROM DATABASE */
                        IEnumerable<Message> messages = await MessengerService.GetMessages(channelViewModel.ChannelId);

                        /* SKIP IF NONE EXISTS */
                        if (messages == null || messages.Count() <= 0)
                        {
                            continue;
                        }

                        /* SAVE TO CACHE */
                        IEnumerable<MessageViewModel> messageViewModels = await CacheQuery.AddOrUpdate<IEnumerable<MessageViewModel>>(messages);

                        channelViewModel.LastMessage = messageViewModels.Last();
                    }
                }

                /* BROADCAST MY CHATS */
                App.EventProvider.Broadcast(
                    BroadcastOptions.TeamsLoaded,
                    BroadcastReasons.Loaded);
            }

            /** LOAD MESSAGES FOR PRIVATE CHATS **/
            if (chats != null && chats.Count() > 0)
            {
                foreach (PrivateChatViewModel chatViewModel in chats)
                {
                    /* PRIVATE CHAT HAS ONLY ONE MAIN CHANNEL */
                    /* LOAD FROM DATABASE */
                    IEnumerable<Message> messages = await MessengerService.GetMessages(chatViewModel.MainChannel.ChannelId);

                    /* SKIP IF NONE EXISTS */
                    if (messages == null || messages.Count() <= 0)
                    {
                        continue;
                    }

                    /* SAVE TO CACHE */
                    IEnumerable<MessageViewModel> messageViewModels = await CacheQuery.AddOrUpdate<IEnumerable<MessageViewModel>>(messages);

                    chatViewModel.LastMessage = messageViewModels.Last();
                }

                /* BROADCAST MY CHATS */
                App.EventProvider.Broadcast(
                    BroadcastOptions.ChatsLoaded,
                    BroadcastReasons.Loaded);
            }
        }

        #region Message

        /// <summary>
        /// Gets all messages of the current team in two possible ways:
        /// • loads from the cache dwelling in MessageManager
        /// • loads from the server database
        /// </summary>
        /// <returns>List of messages</returns>
        public async Task<ObservableCollection<MessageViewModel>> GetMessagesForSelectedChannel(bool forceDBLoad = false)
        {
            LogContext.PushProperty("Method",$"{nameof(GetMessagesForSelectedChannel)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            /** EXIT IF NO TEAM SELECTED **/
            if (App.StateProvider.SelectedTeam == null
                || App.StateProvider.SelectedChannel == null)
            {
                logger.Information("No current team set, exiting");
                logger.Information("Return value: null");

                return null;
            }

            uint channelId = App.StateProvider.SelectedChannel.ChannelId;

            /** FROM CACHE **/
            if (!forceDBLoad
                && CacheQuery.TryGetMessages(channelId, out ObservableCollection<MessageViewModel> fromCache))
            {
                /* GET FROM CACHE */
                logger.Information($"Return value: {fromCache}");

                return fromCache;
            }
            /** FROM DATABASE **/
            else
            {
                /* LOAD FROM DATABASE */
                IEnumerable<Message> fromDb = await MessengerService.GetMessages(channelId);

                if (fromDb == null)
                {
                    return null;
                }

                /* ADD TO CACHE */
                IEnumerable<MessageViewModel> viewModels = await CacheQuery.AddOrUpdate<IEnumerable<MessageViewModel>>(fromDb);

                logger.Information($"Return value: {viewModels}");

                return new ObservableCollection<MessageViewModel>(viewModels);
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

            message.SenderId = App.StateProvider.CurrentUser.Id;
            message.RecipientId = App.StateProvider.SelectedChannel.ChannelId;

            bool isSuccess = await MessengerService.SendMessage(message, App.StateProvider.SelectedTeam.Id);

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

            if (App.StateProvider.CurrentUser == null)
            {
                logger.Information($"Return value: false");
                return false;
            }

            bool isSuccess = await MessengerService.UpdateMessage(messageId, newContent, App.StateProvider.SelectedTeam.Id);

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

            if (App.StateProvider.CurrentUser == null)
            {
                logger.Information($"Return value: false");
                return false;
            }

            bool isSuccess = await MessengerService.DeleteMessage(messageId, App.StateProvider.SelectedTeam.Id);

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

            Reaction reaction = await MessengerService.CreateMessageReaction(messageId, App.StateProvider.CurrentUser.Id, App.StateProvider.SelectedTeam.Id, type.ToString());

            logger.Information($"Return value: {reaction}");

            return reaction != null;
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

            Reaction reaction = await MessengerService.DeleteMessageReaction(messageId, App.StateProvider.CurrentUser.Id, App.StateProvider.SelectedTeam.Id, type.ToString());

            logger.Information($"Return value: {reaction}");

            return reaction != null;
        }


        #endregion

        #region Team

        /// <summary>
        /// Loads teams from the database and converts to view models
        /// </summary>
        /// <returns>List of team view models</returns>
        public async Task<IEnumerable<TeamViewModel>> GetMyTeams
            (bool forceDBLoad = false)
        {
            LogContext.PushProperty("Method",$"{nameof(GetMyTeams)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            /** FROM CACHE **/
            if (!forceDBLoad
                && App.StateProvider != null)
            {
                return CacheQuery.GetMyTeams();
            }
            /** FROM DATABASE **/
            else
            {
                /* LOAD ALL TEAMS FROM DATABASE */
                IEnumerable<Team> teams = await MessengerService.GetTeams(App.StateProvider.CurrentUser.Id);

                if (teams.Count() <= 0)
                {
                    return null;
                }

                /* SAVE TO CACHE */
                await CacheQuery.AddOrUpdate<IEnumerable<TeamViewModel>>(teams);

                /* GET VIEW MODELS FROM CACHE */
                IEnumerable<TeamViewModel> viewModels = CacheQuery.GetMyTeams();

                logger.Information($"Return value: {string.Join(", ", viewModels)}");

                return viewModels;
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
            uint? teamId = await MessengerService.CreateTeam(App.StateProvider.CurrentUser.Id, teamName, teamDescription);

            /** EXIT IF FAILED TO SAVE TO DATABASE **/
            if (teamId == null)
            {
                return;
            }

            /** LOAD CREATED TEAM FROM DATABASE **/
            Team team = await MessengerService.GetTeam((uint)teamId);

            /** SAVE TO CACHE **/
            TeamViewModel viewModel = await CacheQuery.AddOrUpdate<TeamViewModel>(team);

            /** SWITCH TO MAIN CHANNEL OF THE TEAM **/
            SwitchTeamChannel(viewModel.Channels[0].ChannelId);

            /** TRIGGERS TEAM LIST COMPONENTS TO RELOAD **/
            App.EventProvider.Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Created,
                viewModel);
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

            TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

            isSuccess &= await MessengerService.UpdateTeamName(teamName, selectedTeam.Id);
            isSuccess &= await MessengerService.UpdateTeamDescription(teamDescription, selectedTeam.Id);

            if (!isSuccess)
            {
                return;
            }

            App.StateProvider.SelectedTeam.TeamName = teamName;
            App.StateProvider.SelectedTeam.Description = teamDescription;

            App.EventProvider.Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                selectedTeam);
        }

        /// <summary>
        /// Updates CurrentTeam and invokes registered events(TeamSwitched)
        /// </summary>
        /// <param name="channelId">Id of the team to switch to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public void SwitchTeamChannel(uint channelId)
        {
            LogContext.PushProperty("Method","SwitchTeam");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameter teamId={channelId}");

            /** GET CHANNEL AND TEAM FROM CACHE **/
            ChannelViewModel channel = CacheQuery.Get<ChannelViewModel>(channelId);
            TeamViewModel team = CacheQuery.Get<TeamViewModel>(channel.TeamId);

            /** EXIT IF THE CHANNEL DOES NOT EXIST IN CACHE **/
            if (channel == null
                || team == null)
            {
                return;
            }

            /** UPDATE SELECTED TEAM/CHANNEL **/
            App.StateProvider.SelectedTeam = team;
            App.StateProvider.SelectedChannel = channel;

            /** TRIGGERS NAVIGATION AND MESSAGE CONTROLS **/
            App.EventProvider.Broadcast(
                BroadcastOptions.MessagesSwitched,
                BroadcastReasons.Loaded);
        }

        /// <summary>
        /// Creates a new channel with the given channel name
        /// </summary>
        /// <param name="channelName">Name of the new channel</param>
        /// <returns>Task to await</returns>
        public async Task<ChannelViewModel> CreateChannel(string channelName)
        {
            LogContext.PushProperty("Method", "CreateChannel");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameter channelName={channelName}");

            /** SAVE TO DATABASE **/
            uint? channelId = await MessengerService.CreateChannel(channelName, App.StateProvider.SelectedTeam.Id);

            if (channelId == null)
            {
                return null;
            }

            Channel channel = await ChannelService.GetChannel((uint)channelId);
            ChannelViewModel channelViewModel = await CacheQuery.AddOrUpdate<ChannelViewModel>(channel);
            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>(channelViewModel.TeamId);

            App.EventProvider.Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                teamViewModel);

            return channelViewModel;
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
            Channel channel = await MessengerService.DeleteChannel(channelId);

            if (channel == null)
            {
                return false;
            }

            /** REMOVE FROM CACHE **/
            ChannelViewModel removed = CacheQuery.Remove<ChannelViewModel>(channelId);

            App.EventProvider.Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                removed);

            return true;
        }

        #endregion

        #region Member

        /// <summary>
        /// Adds the user to the target team
        /// </summary>
        /// <param name="invitation">Model to build required fields, used only under UI-logic</param>
        /// <returns>True on success, false on error</returns>
        public async Task<MemberViewModel> InviteUser(string userId, uint teamId)
        {
            LogContext.PushProperty("Method",$"{nameof(InviteUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameter userId={userId}, teamId={teamId}");

            User user = await UserService.GetUser(userId);

            if (user == null)
            {
                return null;
            }

            var isSuccess = await MessengerService.SendInvitation(userId, teamId);

            if (!isSuccess)
            {
                return null;
            }

            MemberViewModel member = await CacheQuery.AddOrUpdate<MemberViewModel>(teamId, user);

            logger.Information($"Return value: {isSuccess}");

            return member;
        }

        /// <summary>
        /// Removes a user from a specific Team
        /// </summary>
        /// <param name="userId">Id of the user to be removed</param>
        /// <param name="teamId">Id of the team</param>
        /// <returns>True on success, false on error</returns>
        public async Task<MemberViewModel> RemoveUser(string userId, uint teamId)
        {
            LogContext.PushProperty("Method", $"{nameof(RemoveUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters userId={userId}");

            User user = await UserService.GetUser(userId);

            if (user == null)
            {
                return null;
            }

            var isSuccess = await MessengerService.RemoveMember(userId, teamId);

            if (!isSuccess)
            {
                return null;
            }

            MemberViewModel member = await CacheQuery.AddOrUpdate<MemberViewModel>(teamId, user);

            logger.Information($"Return value: {member}");

            return member;
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

            User user = await MessengerService.GetUserWithNameId(username, nameId);

            logger.Information($"Return value: {user}");

            return user;
        }

        /// <summary>
        /// Get all team Members of a team from the database
        /// </summary>
        /// <param name="teamId">Id of the team</param>
        /// <returns>List of User objects</returns>
        public async Task<IEnumerable<MemberViewModel>> GetTeamMembers(uint teamId)
        {
            LogContext.PushProperty("Method", $"{nameof(GetTeamMembers)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function with parameters teamId={teamId}");
            logger.Information($"Return value: {teamId}");

            /** LOAD FROM DB **/
            IEnumerable<User> users = await MessengerService.GetTeamMembers(teamId);

            /** ADD TO CACHE **/
            IEnumerable<MemberViewModel> members = await CacheQuery.AddOrUpdate<IEnumerable<MemberViewModel>>(teamId, users);

            return members;
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

            User user = await MessengerService.GetUserWithNameId(username, nameId);

            logger.Information($"Return value: {user}");

            return user;
        }

        #endregion

        #region PrivateChat

        public async Task<IEnumerable<PrivateChatViewModel>> GetMyChats(bool forceDBLoad = false)
        {
            LogContext.PushProperty("Method", $"{nameof(GetMyChats)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            /** FROM CACHE **/
            if (!forceDBLoad
                && App.StateProvider != null)
            {
                return CacheQuery.GetMyChats();
            }
            /** FROM DATABASE **/
            else
            {
                /* LOAD ALL TEAMS FROM DATABASE */
                IEnumerable<Team> data = await MessengerService.GetTeams(App.StateProvider.CurrentUser.Id);

                /* SAVE TO CACHE */
                await CacheQuery.AddOrUpdate<IEnumerable<TeamViewModel>>(data);

                /* GET VIEW MODELS FROM CACHE */
                IEnumerable<PrivateChatViewModel> viewModels = CacheQuery.GetMyChats();

                logger.Information($"Return value: {string.Join(", ", viewModels)}");

                return viewModels;
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
            uint? chatId = await MessengerService.StartChat(App.StateProvider.CurrentUser.Id, targetUserId);

            /** EXIT IF FAILED TO SAVE TO DATABASE **/
            if (chatId == null)
            {
                return false;
            }

            /** LOAD FROM DATABASE **/
            Team data = await MessengerService.GetTeam((uint)chatId);

            /** ADD TO CACHE **/
            PrivateChatViewModel viewModel = await CacheQuery.AddOrUpdate<PrivateChatViewModel>(data);

            /** SWITCH TO CHAT **/
            SwitchChat(viewModel.Id);

            return true;
        }

        /// <summary>
        /// Updates SelectedTeam and SelectedChannel for the selected private chat
        /// </summary>
        /// <param name="channelId">Id of the team to switch to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public void SwitchChat(uint chatId)
        {
            LogContext.PushProperty("Method", "SwitchChat");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameter chatId={chatId}");

            /** GET FROM CACHE **/
            PrivateChatViewModel viewModel = CacheQuery.Get<PrivateChatViewModel>(chatId);

            /** UPDATES TEAM AS PRIVATE CHAT AND SETS CHANNEL TO MAIN **/
            App.StateProvider.SelectedTeam = viewModel;
            App.StateProvider.SelectedChannel = viewModel.MainChannel;

            /** TRIGGERS NAVIGATION AND MESSAGE CONTROLS **/
            App.EventProvider.Broadcast(
                BroadcastOptions.MessagesSwitched,
                BroadcastReasons.Loaded);
        }

        #endregion
    }
}
