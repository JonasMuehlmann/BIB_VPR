﻿using System;
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

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private UserService UserService => Singleton<UserService>.Instance;

        #endregion

        #region Properties

        public ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>> MessagesByConnectedTeam { get; }

        public uint? CurrentTeamId { get; set; }

        public UserViewModel CurrentUser { get; private set; }

        public ILogger logger = GlobalLogger.Instance;

        public ChatHubConnectionState ConnectionState
        {
            get
            {
                if (CurrentUser == null || CurrentUser?.Teams == null)
                {
                    return ChatHubConnectionState.Loading;
                }
                else if (CurrentUser.Teams.Count == 0)
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
        public event EventHandler<IEnumerable<Team>> TeamsUpdated;

        /// <summary>
        /// Event handler for update at TeamDescription and TeamName
        /// </summary>
        public event EventHandler<Team> TeamUpdated;

        #endregion

        public ChatHubService()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>>();

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            LogContext.PushProperty("Method", $"{nameof(InitializeAsync)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Initializing ChatHubService");

            MessengerService.RegisterListenerForMessages(OnMessageReceived);
            MessengerService.RegisterListenerForInvites(OnInvitationReceived);
            MessengerService.RegisterListenerForMessageUpdate(OnMessageUpdated);
            MessengerService.RegisterListenerForMessageDelete(OnMessageDeleted);

            CurrentUser = await UserDataService.GetUserAsync();

            var teams = await MessengerService.LoadTeams(CurrentUser.Id);

            if (teams == null || teams.Count() <= 0)
            {
                logger.Information($"Event {nameof(TeamsUpdated)} invoked with no team");

                CurrentUser.Teams = new List<Team>();
                TeamsUpdated?.Invoke(this, null);

                return;
            }

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

                if (messages != null)
                {
                    ObservableCollection<MessageViewModel> parents = new ObservableCollection<MessageViewModel>(MessageViewModel.FromDbModel(messages));

                    CreateEntryForCurrentTeam(team.Id, parents);
                }
            }

            // Sets the first team as the selected team
            CurrentTeamId = CurrentUser.Teams.FirstOrDefault().Id;

            logger.Information($"Event {nameof(TeamsUpdated)} invoked with {CurrentUser.Teams.Count} teams");

            // Broadcast Teams
            TeamsUpdated?.Invoke(this, CurrentUser.Teams);
        }

        #region Message

        /// <summary>
        /// Gets all messages of the current team
        /// </summary>
        /// <returns>List of messages</returns>
        public async Task<ObservableCollection<MessageViewModel>> GetMessages()
        {
            LogContext.PushProperty("Method",$"{nameof(GetMessages)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            if (CurrentTeamId == null)
            {
                logger.Information("No current team set, exiting");
                logger.Information("Return value: null");

                return null;
            }

            uint teamId = (uint)CurrentTeamId;

            // Checks the cache if the messages has been loaded for the team
            if (MessagesByConnectedTeam.TryGetValue(teamId, out ObservableCollection<MessageViewModel> fromCache))
            {
                // Loads from cache
                logger.Information($"Return value: {fromCache}");

                return fromCache;
            }
            else
            {
                // Loads from database
                var fromDb = await MessengerService.LoadMessages(teamId);

                if (fromDb == null)
                {
                    return null;
                }

                ObservableCollection<MessageViewModel> messages = new ObservableCollection<MessageViewModel>(MessageViewModel.FromDbModel(fromDb));

                CreateEntryForCurrentTeam(teamId, messages);

                logger.Information($"Return value: {messages}");

                return messages;
            }
        }

        /// <summary>
        /// Sends a message to the current team
        /// </summary>
        /// <param name="content">Content to be written in the message</param>
        /// <returns>True on success, false on error</returns>
        public async Task<bool> SendMessage(Message message)
        {
            LogContext.PushProperty("Method", $"{nameof(SendMessage)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentUser == null)
            {
                logger.Information($"Error while fetching user data");
                logger.Information($"Return value: false");
                return false;
            }

            // Set sender and recipient ids
            message.SenderId = CurrentUser.Id;
            message.RecipientId = (uint)CurrentTeamId;

            bool isSuccess = await MessengerService.SendMessage(message);

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
        }

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

        public async Task<bool> DeleteMessage(uint messageId, MessageType type)
        {
            LogContext.PushProperty("Method", $"{nameof(SendMessage)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentUser == null)
            {
                logger.Information($"Return value: false");
                return false;
            }

            bool isSuccess = await MessengerService.DeleteMessage(messageId);

            FindAndRemoveMessage(messageId, type);

            logger.Information($"Return value: {isSuccess}");

            return isSuccess;
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
            LogContext.PushProperty("Method",$"{nameof(GetTeamsList)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            if (CurrentUser == null)
            {
                logger.Information("Return value: null");

                return null;
            }

            var teams = await MessengerService.LoadTeams(CurrentUser.Id);

            logger.Information($"Loading members for teams {string.Join(", ", teams.Select(team => team.Id))}");

            teams = teams.Select((team) =>
            {
                Task.Run(async () =>
                {
                    var members = await MessengerService.LoadTeamMembers(team.Id);
                    SetMembers(ref team, members);
                }).GetAwaiter().GetResult();

                return team;
            });

            //get all channels
            teams = await GetChannelsForAllTeams(teams);

            // Updates the teams list under the current user
            CurrentUser.Teams.Clear();
            foreach (var team in teams)
            {
                CurrentUser.Teams.Add(team);
            }

            logger.Information($"Return value: {string.Join(", ", teams)}");

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

            var teams = await GetTeamsList();

            logger.Information($"Event {nameof(TeamsUpdated)} invoked with {teams?.Count()} teams");

            TeamsUpdated?.Invoke(this, teams);
        }

        /// <summary>
        /// Updates the teamName and teamDescription of the current tem
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="teamDescription"></param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateTeam(string teamName, string teamDescription)
        {
            LogContext.PushProperty("Method", "UpdateTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamName={teamName}, teamDescription={teamDescription}");

            await MessengerService.ChangeTeamName(teamName, (uint)CurrentTeamId);
            await MessengerService.ChangeTeamDescription(teamDescription, (uint)CurrentTeamId);

            for (int i = 0; i < CurrentUser.Teams.Count; i++)
            {
                if (CurrentUser.Teams[i].Id == (uint)CurrentTeamId)
                {
                    CurrentUser.Teams[i].Name = teamName;
                    CurrentUser.Teams[i].Description = teamDescription;
                }
            }

            TeamUpdated?.Invoke(this, await GetCurrentTeam());
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

            var messages = await GetMessages();

            logger.Information($"Event {nameof(TeamSwitched)} invoked with {messages?.Count()} messages");

            TeamSwitched?.Invoke(this, messages);
        }

        /// <summary>
        /// Returns the current team model from the loaded list
        /// </summary>
        /// <returns>A complete team object</returns>
        public async Task<Team> GetCurrentTeam()
        {
            LogContext.PushProperty("Method", $"{nameof(GetCurrentTeam)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            if (CurrentTeamId == null)
            {
                logger.Information($"Current team id is not set valid: {CurrentTeamId}");

                return null;
            }

            var currentTeam = CurrentUser.Teams
                .Where(t => t.Id == CurrentTeamId)
                .FirstOrDefault();

            currentTeam.FilterAndUpdateChannels(await GetChannelsList());

            logger.Information($"Return value: {currentTeam}");

            return currentTeam;
        }


        /// <summary>
        /// creates a new channel with name
        /// </summary>
        /// <param name="channelName"></param>
        /// <returns>Task to await</returns>
        public async Task CreateChannel(string channelName)
        {
            LogContext.PushProperty("Method", "CreateChannel");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter channelName={channelName}");

            await MessengerService.CreateChannel(channelName, (uint)CurrentTeamId);

            TeamsUpdated?.Invoke(this, await GetTeamsList());
        }

        /// <summary>
        /// get Channels by Team
        /// </summary>
        /// <returns>Channels</returns>
        public async Task<IEnumerable<Channel>> GetChannelsList()
        {
            LogContext.PushProperty("Method", "GetChannelsList");
            LogContext.PushProperty("SourceContext", this.GetType().Name);


            return await MessengerService.GetChannelsForTeam((uint)CurrentTeamId);
        }

        private async Task<IEnumerable<Team>> GetChannelsForAllTeams(IEnumerable<Team> teams)
        {
            LogContext.PushProperty("Method", "GetChannelsList");
            LogContext.PushProperty("SourceContext", this.GetType().Name);


            logger.Information($"Function called with parameter teams={string.Join(",", teams)}");


            foreach (Team t in teams) {
                t.FilterAndUpdateChannels(await MessengerService.GetChannelsForTeam(t.Id));
            }

            return teams;
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

            var user = await MessengerService.GetUserWithNameId(username, nameId);

            logger.Information($"Return value: {user}");

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
        public async Task<bool> StartChat(string targetUserId)
        {
            LogContext.PushProperty("Method", $"{nameof(SearchUser)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters targetUserId={targetUserId}");

            uint? chatId = await MessengerService.StartChat(CurrentUser.Id, targetUserId);

            if (chatId == null)
            {
                return false;
            }

            var chat = await MessengerService.GetTeam((uint)chatId);
            var members = await MessengerService.LoadTeamMembers(chat.Id);

            SetMembers(ref chat, members);

            CurrentUser.Teams.Add(chat);

            await SwitchTeam((uint)chatId);

            logger.Information($"Event {nameof(TeamsUpdated)} invoked with {CurrentUser.Teams.Count()} messages");

            TeamsUpdated?.Invoke(this, CurrentUser.Teams);

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

            // Loads user data of the sender
            message.Sender = await UserService.GetUser(message.SenderId);

            if (message.Sender == null)
            {
                return;
            }

            MessageViewModel viewModel = SortAndAddMessage(message);

            logger.Information($"Event {nameof(MessageReceived)} invoked with message: {message}");

            // Invoke registered events
            MessageReceived?.Invoke(this, viewModel);
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

        private void OnMessageUpdated(object sender, Message message)
        {
            bool isValid = message != null;

            if (!isValid)
            {
                return;
            }

            MessageViewModel vm = SortAndUpdateMessage(message);

            MessageUpdated?.Invoke(this, vm);
        }

        private void OnMessageDeleted(object sender, Message message)
        {
            bool isValid = message != null;

            if (!isValid)
            {
                return;
            }

            MessageDeleted?.Invoke(this, EventArgs.Empty);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Safely creates a new entry in concurrent dictionary for a team
        /// </summary>
        /// <param name="teamId">Id of the team for the entry</param>
        /// <param name="messages">List of messages to initialize with</param>
        private void CreateEntryForCurrentTeam(uint teamId, IEnumerable<MessageViewModel> messages)
        {
            LogContext.PushProperty("Method", $"{nameof(CreateEntryForCurrentTeam)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

            MessagesByConnectedTeam.AddOrUpdate(
                teamId,
                (key) =>
                new ObservableCollection<MessageViewModel>(messages),
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
            LogContext.PushProperty("Method", $"{nameof(SetMembers)}");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called");

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

        private MessageViewModel SortAndAddMessage(Message message)
        {
            var type = MessageViewModel.ConvertAndGetType(message, out MessageViewModel viewModel);

            switch (type)
            {
                case MessageType.Parent:
                    // Adds to message dictionary
                    MessagesByConnectedTeam.AddOrUpdate(
                        (uint)viewModel.TeamId,
                        new ObservableCollection<MessageViewModel>() { viewModel },
                        (key, list) =>
                        {
                            list.Add(viewModel);
                            return list;
                        });
                    break;
                case MessageType.Reply:
                    // Adds to the list of replies of the message
                    if (MessagesByConnectedTeam.TryGetValue(
                        (uint)viewModel.TeamId,
                        out ObservableCollection<MessageViewModel> messages))
                    {
                        messages.Select(vm =>
                        {
                            if (vm.Id == viewModel.TeamId)
                            {
                                vm.Replies.Add(viewModel);
                            }

                            return vm;
                        });
                    }
                    break;
                default:
                    break;
            }

            return viewModel;
        }

        private MessageViewModel SortAndUpdateMessage(Message message)
        {
            var type = MessageViewModel.ConvertAndGetType(message, out MessageViewModel viewModel);

            switch (type)
            {
                case MessageType.Parent:
                    // Adds to message dictionary
                    MessagesByConnectedTeam.AddOrUpdate(
                        (uint)viewModel.TeamId,
                        new ObservableCollection<MessageViewModel>() { viewModel },
                        (key, list) =>
                        {
                            list.Select(m =>
                            {
                                if (m.Id == viewModel.Id)
                                {
                                    m = viewModel;
                                }

                                return m;
                            });

                            return list;
                        });
                    break;
                case MessageType.Reply:
                    // Adds to the list of replies of the message
                    if (MessagesByConnectedTeam.TryGetValue(
                        (uint)viewModel.TeamId,
                        out ObservableCollection<MessageViewModel> messages))
                    {
                        messages.Select(vm =>
                        {
                            if (vm.Id == viewModel.TeamId)
                            {
                                vm.Replies.Select(r =>
                                {
                                    if (r.Id == viewModel.Id)
                                    {
                                        r = viewModel;
                                    }

                                    return r;
                                });
                            }

                            return vm;
                        });
                    }
                    break;
                default:
                    break;
            }

            return viewModel;
        }

        private void FindAndRemoveMessage(uint id, MessageType type)
        {
            switch (type)
            {
                case MessageType.Parent:
                    MessagesByConnectedTeam.AddOrUpdate(
                        (uint)CurrentTeamId,
                        new ObservableCollection<MessageViewModel>(),
                        (key, list) =>
                        {
                            var updated = list
                                .Where(m => m.Id != id);

                            return new ObservableCollection<MessageViewModel>(updated);
                        });
                    break;
                case MessageType.Reply:
                    if (MessagesByConnectedTeam.TryGetValue(
                        (uint)CurrentTeamId,
                        out ObservableCollection<MessageViewModel> messages))
                    {
                        messages.Select(m =>
                        {
                            bool found = m.Replies.Any(r => r.Id == id);

                            if (found)
                            {
                                var updated = m.Replies.Where(r => r.Id != id);

                                m.Replies = new ObservableCollection<MessageViewModel>(updated);
                            }

                            return m;
                        });
                    }
                    break;
                default:
                    break;
            }

            MessageDeleted?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
