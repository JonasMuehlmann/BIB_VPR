using Messenger.Core.Models;
using Messenger.Core.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Serilog.Context;
using System.Collections.Generic;

namespace Messenger.Core.Services
{
    /// <summary>
    /// Holds definitions for messages to send to the signalR hub
    /// </summary>
    public class SignalRService
    {
        #region Private

        private string HUB_URL = @"https://bib-vpr.azurewebsites.net/chathub";

        private HubConnection _connection;

        public Serilog.ILogger logger => GlobalLogger.Instance;

        #endregion

        /// <summary>
        /// Delegate on "ReceiveMessage"(Hub Method)
        /// </summary>
        public event EventHandler<SignalREventArgs<Message>> ReceiveMessage;
        public event EventHandler<SignalREventArgs<Team>> ReceiveInvitation;
        public event EventHandler<SignalREventArgs<Notification>> ReceiveNotification;

        public event EventHandler<SignalREventArgs<Message>> MessageUpdated;
        public event EventHandler<SignalREventArgs<Message>> MessageDeleted;
        public event EventHandler<SignalREventArgs<Message>> MessageReactionsUpdated;

        public event EventHandler<SignalREventArgs<Team>> TeamCreated;
        public event EventHandler<SignalREventArgs<Team>> TeamUpdated;
        public event EventHandler<SignalREventArgs<Team>> TeamDeleted;

        public event EventHandler<SignalREventArgs<Channel>> ChannelCreated;
        public event EventHandler<SignalREventArgs<Channel>> ChannelUpdated;
        public event EventHandler<SignalREventArgs<Channel>> ChannelDeleted;

        public event EventHandler<SignalREventArgs<TeamRole>> TeamRoleUpdated;
        public event EventHandler<SignalREventArgs<TeamRole>> TeamRoleDeleted;

        public event EventHandler<SignalREventArgs<User, Team>> MemberAdded;
        public event EventHandler<SignalREventArgs<User, Team>> MemberUpdated;
        public event EventHandler<SignalREventArgs<User, Team>> MemberRemoved;

        public event EventHandler<SignalREventArgs<User>> UserUpdated;

        public void Initialize()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .ConfigureLogging(log =>
                {
                    log.AddConsole();
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.Zero, TimeSpan.FromSeconds(10) })
                .Build();


            /** RECEIVE: PASSIVE EVENTS TRIGGERED ONLY FROM OTHER USERS **/
            _connection.On<Message>("ReceiveMessage", (message) => ReceiveMessage?.Invoke(typeof(SignalRService), BuildArgument(message)));
            _connection.On<Team>("ReceiveInvitation", (team) => ReceiveInvitation?.Invoke(typeof(SignalRService), BuildArgument(team)));
            _connection.On<Notification>("ReceiveNotification", (notification) => ReceiveNotification?.Invoke(typeof(SignalRService), BuildArgument(notification)));

            /** MESSAGE **/
            _connection.On<Message>("MessageUpdated", (message) => MessageUpdated?.Invoke(typeof(SignalRService), BuildArgument(message)));
            _connection.On<Message>("MessageDeleted", (message) => MessageDeleted?.Invoke(typeof(SignalRService), BuildArgument(message)));
            _connection.On<Message>("MessageReactionsUpdated", (message) => MessageReactionsUpdated?.Invoke(typeof(SignalRService), BuildArgument(message)));

            /** TEAM **/
            _connection.On<Team>("TeamCreated", (team) => TeamCreated?.Invoke(typeof(SignalRService), BuildArgument(team)));
            _connection.On<Team>("TeamUpdated", (team) => TeamUpdated?.Invoke(typeof(SignalRService), BuildArgument(team)));
            _connection.On<Team>("TeamDeleted", (team) => TeamDeleted?.Invoke(typeof(SignalRService), BuildArgument(team)));

            /** CHANNEL **/
            _connection.On<Channel>("ChannelCreated", (channel) => ChannelCreated?.Invoke(typeof(SignalRService), BuildArgument(channel)));
            _connection.On<Channel>("ChannelUpdated", (channel) => ChannelUpdated?.Invoke(typeof(SignalRService), BuildArgument(channel)));
            _connection.On<Channel>("ChannelDeleted", (channel) => ChannelDeleted?.Invoke(typeof(SignalRService), BuildArgument(channel)));

            /** TEAM ROLES **/
            _connection.On<TeamRole>("TeamRoleUpdated", (role) => TeamRoleUpdated?.Invoke(typeof(SignalRService), BuildArgument(role)));
            _connection.On<TeamRole>("TeamRoleDeleted", (role) => TeamRoleDeleted?.Invoke(typeof(SignalRService), BuildArgument(role)));

            /** MEMBER **/
            _connection.On<User, Team>("MemberAdded", (user, team) => MemberAdded?.Invoke(typeof(SignalRService), BuildArgument(user, team)));
            _connection.On<User, Team>("MemberUpdated", (user, team) => MemberUpdated?.Invoke(typeof(SignalRService), BuildArgument(user, team)));
            _connection.On<User, Team>("MemberRemoved", (user, team) => MemberRemoved?.Invoke(typeof(SignalRService), BuildArgument(user, team)));

            /** USER **/
            _connection.On<User>("UserUpdated", (user) => UserUpdated?.Invoke(typeof(SignalRService), BuildArgument(user)));
        }

        #region Connection

        /// <summary>
        /// Starts the connection with the preset
        /// </summary>
        /// <param name="userId">Id of the current user</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task OpenConnection(string userId)
        {
            LogContext.PushProperty("Method", "Open");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameters userId={userId}");

            try
            {
                await _connection.StartAsync();
                await _connection.SendAsync("Register", userId);

                logger.Information($"Connecting the user identity by userId={userId} to the hub");
            }
            catch (Exception e)
            {
                logger.Information(e, "Returning");
            }
        }


        /// <summary>
        /// Closes the connection with the hub
        /// </summary>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task CloseConnection()
        {
            LogContext.PushProperty("Method", "Close");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called");

            try
            {
                await _connection.StopAsync();

                logger.Information($"Disconnecting the current user from the hub");
            }
            catch (Exception e)
            {
                logger.Information(e, "Returning");
            }
        }

        /// <summary>
        /// Joins a hub group with the current connection id
        /// </summary>
        /// <param name="userId">Id of the current user</param>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task JoinTeam(string userId, string teamId)
        {
            LogContext.PushProperty("Method", "JoinTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter teamId: {teamId}");

            try
            {
                await _connection.InvokeAsync("JoinTeam", userId, teamId);

                logger.Information($"Adding the current user to the hub group with the name {teamId}");
            }
            catch (Exception e)
            {
                logger.Information(e, "Returning");
            }
        }

        /// <summary>
        /// Leaves a hub group with the current connection id
        /// </summary>
        /// <param name="userId">Id of the current user</param>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task LeaveTeam(string userId, string teamId)
        {
            LogContext.PushProperty("Method", "LeaveTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter teamId: {teamId}");

            try
            {
                await _connection.SendAsync("LeaveTeam", userId, teamId);

                logger.Information($"Removing the current user from the hub group with the name {teamId}");
            }
            catch (Exception e)
            {
                logger.Information(e, "Returning");
            }
        }

        #endregion

        #region Notification

        public async Task SendNotificationToUser(Notification notification)
        {
            LogContext.PushProperty("Method", "SendNotificationToUser");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter notification={notification}");

            await _connection.SendAsync("SendNotificationToUser", notification);

            logger.Information($"Sent notification #{notification.Id} to user #{notification.RecipientId}");
        }

        #endregion

        #region Message

        /// <summary>
        /// Sends a message to a hub group
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SendMessage(Message message, uint teamId)
        {
            LogContext.PushProperty("Method", "SendMessage");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}");

            message.UploadFileData = null;

            await _connection.SendAsync("SendMessage", message, teamId.ToString());

            logger.Information($"Sent message #{message.Id} to the channel #{message.RecipientId}");
        }

        /// <summary>
        /// Update a message's data and notify other clients
        /// </summary>
        /// <param name="message">The updated message object</param>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateMessage(Message message, uint teamId)
        {
            LogContext.PushProperty("Method", "UpdateMessage");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}");

            await _connection.SendAsync("UpdateMessage", message, teamId.ToString());

            logger.Information($"Updated message #{message.Id} from the channel #{message.RecipientId}");
        }

        /// <summary>
        /// Delete a message and notify other clients
        /// </summary>
        /// <param name="message">The updated message object</param>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task DeleteMessage(Message message, uint teamId)
        {
            LogContext.PushProperty("Method", "DeleteMessage");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}");

            await _connection.SendAsync("DeleteMessage", message, teamId.ToString());

            logger.Information($"Deleted message #{message.Id} from the channel #{message.RecipientId}");
        }

        /// <summary>
        /// Update a message's reactions and notify other clients
        /// </summary>
        /// <param name="message">The updated message object</param>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateMessageReactions(Message message, uint teamId)
        {
            LogContext.PushProperty("Method", "UpdateMessageReactions");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}");

            try
            {
                await _connection.SendAsync("UpdateMessageReactions", message, teamId.ToString());
            }
            catch (Exception e) {
                logger.Information($"Send error={e.Message}");
            }
            logger.Information($"Updated reactions in message #{message.Id} from the channel #{message.RecipientId}");
        }

        #endregion

        #region Team

        /// <summary>
        /// Adds the user to the hub group
        /// </summary>
        /// <param name="team">A team object to create a team from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task CreateTeam(Team team)
        {
            LogContext.PushProperty("Method", "CreateTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter team={team}");

            await _connection.SendAsync("CreateTeam", team);

            logger.Information($"Created team #{team.Id} ({team.CreationDate})");
        }

        /// <summary>
        /// Update a team's data and notify other clients
        /// </summary>
        /// <param name="team">A team object to update a team from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateTeam(Team team)
        {
            LogContext.PushProperty("Method", "UpdateTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter team={team}");

            await _connection.SendAsync("UpdateTeam", team);

            logger.Information($"Updated team '{team.Name}' #{team.Id} ({team.CreationDate})");
        }

        /// <summary>
        /// Delete a team and notify other clients
        /// </summary>
        /// <param name="team">A team object to delete a team from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task DeleteTeam(Team team)
        {
            LogContext.PushProperty("Method", "DeleteTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter team={team}");

            await _connection.SendAsync("DeleteTeam", team);

            logger.Information($"Deleted team #{team.Id} ({team.CreationDate})");
        }

        #endregion

        #region Channel

        /// <summary>
        /// Create a channel and and notify other clients other clients
        /// </summary>
        /// <param name="channel">Channel object to create channel from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task CreateChannel(Channel channel)
        {
            LogContext.PushProperty("Method", "CreateChannel");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter channel={channel}");

            await _connection.SendAsync("CreateChannel", channel);

            logger.Information($"Created channel #{channel.ChannelId} in team #{channel.TeamId})");
        }

        /// <summary>
        /// Update a channel and and notify other clients other clients
        /// </summary>
        /// <param name="channel">Channel object to update channel from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateChannel(Channel channel)
        {
            LogContext.PushProperty("Method", "UpdateChannel");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter channel={channel}");

            await _connection.SendAsync("UpdateChannel", channel);

            logger.Information($"Updated channel #{channel.ChannelId} in team #{channel.TeamId})");
        }

        /// <summary>
        /// Delete a channel and and notify other clients other clients
        /// </summary>
        /// <param name="channel">Channel object to delete channel from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task DeleteChannel(Channel channel)
        {
            LogContext.PushProperty("Method", "DeleteChannel");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter channel={channel}");

            await _connection.SendAsync("DeleteChannel", channel);

            logger.Information($"Deleted channel #{channel.ChannelId} from team #{channel.TeamId})");
        }

        #endregion

        #region Team Roles

        /// <summary>
        /// Add or update a team role and notify other clients
        /// </summary>
        /// <param name="role">TeamRole object to create/update role from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task AddOrUpdateTeamRole(TeamRole role)
        {
            LogContext.PushProperty("Method", "AddOrUpdateTeamRole");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter role={role}");

            await _connection.SendAsync("AddOrUpdateTeamRole", role);

            logger.Information($"Added/Updated team role '{role.Role}' in team #{role.TeamId})");
        }

        /// <summary>
        /// Delete a team role and notify other clients
        /// </summary>
        /// <param name="role">TeamRole object to delete role from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task DeleteTeamRole(TeamRole role)
        {
            LogContext.PushProperty("Method", "DeleteTeamRole");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter role={role}");

            await _connection.SendAsync("DeleteTeamRole", role);

            logger.Information($"Deleted team role '{role.Role}' from team #{role.TeamId})");
        }

        #endregion

        #region Member

        /// <summary>
        /// Send a user an invitation to a team and notify other clients
        /// </summary>
        /// <param name="user">Object of user to invite</param>
        /// <param name="team">Objecte of team to invite user to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SendInvitation(User user, Team team)
        {
            LogContext.PushProperty("Method", "SendInvitation");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("SendInvitation", user, team);

            logger.Information($"Sent invitation to team #{team.Id} to user #{user.Id}");
        }

        /// <summary>
        /// Add a member to a team and notify other clients
        /// </summary>
        /// <param name="user">Object of user to add</param>
        /// <param name="team">Objecte of team to add user to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task AddMember(User user, Team team)
        {
            LogContext.PushProperty("Method", "AddMember");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("AddMember", user, team);

            logger.Information($"Added member #{user.Id} to team #{team.Id}");
        }

        public async Task UpdateMember(User user, Team team)
        {
            LogContext.PushProperty("Method", "UpdateMember");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("UpdateMember", user, team);

            logger.Information($"Updated member #{user.Id} in team #{team.Id}");
        }

        /// <summary>
        /// Remove a member from a team and notify other clients
        /// </summary>
        /// <param name="user">Object of user to remove</param>
        /// <param name="team">Objecte of team to remove user from</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task RemoveMember(User user, Team team)
        {
            LogContext.PushProperty("Method", "RemoveMember");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("RemoveMember", user, team);

            logger.Information($"Removed user #{user.Id} from team #{team.Id}");
        }

        #endregion

        #region User

        /// <summary>
        /// Update a user's data and notify other clients
        /// </summary>
        /// <param name="user">Updated user object</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateUser(User user)
        {
            LogContext.PushProperty("Method", "UpdateUser");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}");

            await _connection.SendAsync("UpdateUser", user);

            logger.Information($"User information updated #{user.Id}");
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Reconnect to the signalR hub after a 500ms delay
        /// </summary>
        /// <returns>Asynchronous task to be awaited</returns>
        private async Task Reconnect(Exception e)
        {
            LogContext.PushProperty("Method", "Reconnect");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called");

            await Task.Delay(500);
            _connection = await CreateHubConnection();
            await _connection.StartAsync();

            logger.Information($"Building a new connection to the hub");
        }

        /// <summary>
        /// Connect to the configured signalR hub
        /// </summary>
        /// <returns>Asynchronous task to be awaited</returns>
        private async Task<HubConnection> CreateHubConnection()
        {
            LogContext.PushProperty("Method", "CreateHubConnection");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called");

            HubConnection hubConnection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .Build();

            hubConnection.Closed += Reconnect;

            await hubConnection.StartAsync();

            logger.Information($"Building a new connection to the hub");

            return hubConnection;
        }

        /// <summary>
        /// Build a SignalREventArgs instance with one value
        /// </summary>
        private SignalREventArgs<T> BuildArgument<T>(T value)
        {
            return new SignalREventArgs<T>(value);
        }

        /// <summary>
        /// Build a SignalREventArgs instance with two values
        /// </summary>
        private SignalREventArgs<TOne, TTwo> BuildArgument<TOne, TTwo>(TOne firstValue, TTwo secondValue)
        {
            return new SignalREventArgs<TOne, TTwo>(firstValue, secondValue);
        }

        #endregion
    }
}
