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
    public static class SignalRService
    {
        #region Private

        private static string HUB_URL = @"https://vpr.azurewebsites.net/chathub";

        private static HubConnection _connection;

        public static Serilog.ILogger logger => GlobalLogger.Instance;

        #endregion

        /// <summary>
        /// Delegate on "ReceiveMessage"(Hub Method)
        /// </summary>
        public static event EventHandler<SignalREventArgs<Message>> ReceiveMessage;

        public static event EventHandler<SignalREventArgs<Message>> MessageUpdated;
        public static event EventHandler<SignalREventArgs<Message>> MessageDeleted;
        public static event EventHandler<SignalREventArgs<Message, Reaction>> MessageReactionCreated;
        public static event EventHandler<SignalREventArgs<Message, Reaction>> MessageReactionDeleted;

        public static event EventHandler<SignalREventArgs<Team>> TeamCreated;
        public static event EventHandler<SignalREventArgs<Team>> TeamUpdated;
        public static event EventHandler<SignalREventArgs<Team>> TeamDeleted;

        public static event EventHandler<SignalREventArgs<Channel>> ChannelCreated;
        public static event EventHandler<SignalREventArgs<Channel>> ChannelUpdated;
        public static event EventHandler<SignalREventArgs<Channel>> ChannelDeleted;

        public static event EventHandler<SignalREventArgs<TeamRole>> TeamRoleCreated;
        public static event EventHandler<SignalREventArgs<TeamRole>> TeamRoleUpdated;
        public static event EventHandler<SignalREventArgs<TeamRole>> TeamRoleDeleted;
        public static event EventHandler<SignalREventArgs<TeamRole, IEnumerable<Permissions>>> RolePermissionsUpdated;

        public static event EventHandler<SignalREventArgs<User, Team>> ReceiveInvitation;
        public static event EventHandler<SignalREventArgs<User, Team>> MemberAdded;
        public static event EventHandler<SignalREventArgs<User, Team>> MemberUpdated;
        public static event EventHandler<SignalREventArgs<User, Team>> MemberRemoved;

        public static event EventHandler<SignalREventArgs<User>> UserUpdated;

        public static void Initialize()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .ConfigureLogging(log =>
                {
                    log.AddConsole();
                })
                .Build();

            /** MESSAGE **/
            _connection.On<Message>("ReceiveMessage", (message) => ReceiveMessage?.Invoke(typeof(SignalRService), BuildArgument(message)));
            _connection.On<Message>("MessageUpdated", (message) => MessageUpdated?.Invoke(typeof(SignalRService), BuildArgument(message)));
            _connection.On<Message>("MessageDeleted", (message) => MessageDeleted?.Invoke(typeof(SignalRService), BuildArgument(message)));
            _connection.On<Message, Reaction>("MessageReactionCreated", (message, reaction) => MessageReactionCreated?.Invoke(typeof(SignalRService), BuildArgument(message, reaction)));
            _connection.On<Message, Reaction>("MessageReactionDeleted", (message, reaction) => MessageReactionDeleted?.Invoke(typeof(SignalRService), BuildArgument(message, reaction)));

            /** TEAM **/
            _connection.On<Team>("TeamCreated", (team) => TeamCreated?.Invoke(typeof(SignalRService), BuildArgument(team)));
            _connection.On<Team>("TeamUpdated", (team) => TeamUpdated?.Invoke(typeof(SignalRService), BuildArgument(team)));
            _connection.On<Team>("TeamDeleted", (team) => TeamDeleted?.Invoke(typeof(SignalRService), BuildArgument(team)));

            /** CHANNEL **/
            _connection.On<Channel>("ChannelCreated", (channel) => ChannelCreated?.Invoke(typeof(SignalRService), BuildArgument(channel)));
            _connection.On<Channel>("ChannelUpdated", (channel) => ChannelUpdated?.Invoke(typeof(SignalRService), BuildArgument(channel)));
            _connection.On<Channel>("ChannelDeleted", (channel) => ChannelDeleted?.Invoke(typeof(SignalRService), BuildArgument(channel)));

            /** TEAM ROLES **/
            _connection.On<TeamRole>("TeamRoleCreated", (role) => TeamRoleCreated?.Invoke(typeof(SignalRService), BuildArgument(role)));
            _connection.On<TeamRole>("TeamRoleUpdated", (role) => TeamRoleUpdated?.Invoke(typeof(SignalRService), BuildArgument(role)));
            _connection.On<TeamRole>("TeamRoleDeleted", (role) => TeamRoleDeleted?.Invoke(typeof(SignalRService), BuildArgument(role)));
            _connection.On<TeamRole, IEnumerable<Permissions>>("RolePermissionsUpdated", (role, permissions) => RolePermissionsUpdated?.Invoke(typeof(SignalRService), BuildArgument(role, permissions)));

            /** MEMBER **/
            _connection.On<User, Team>("ReceiveInvitation", (user, team) => ReceiveInvitation?.Invoke(typeof(SignalRService), BuildArgument(user, team)));
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
        public static async Task OpenConnection(string userId)
        {
            LogContext.PushProperty("Method", "Open");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameters userId={userId}");

            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                    await _connection.SendAsync("Register", userId);

                    logger.Information($"Connecting the user identity by userId={userId} to the hub");
                }
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
        public static async Task CloseConnection()
        {
            LogContext.PushProperty("Method","Close");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called");

            try
            {
                await _connection.StopAsync();

                logger.Information($"Disconnecting the current user from the hub");
            }
            catch (Exception e)
            {
                logger.Information(e,"Returning");
            }
        }

        /// <summary>
        /// Joins a hub group with the current connection id
        /// </summary>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public static async Task JoinTeam(string teamId)
        {
            LogContext.PushProperty("Method", "JoinTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter teamId: {teamId}");

            try
            {
                await _connection.SendAsync("JoinTeam", teamId);

                logger.Information($"Adding the current user to the hub group with the name {teamId}");
            }
            catch (Exception e)
            {
                logger.Information(e, "Returning");
            }
        }

        public static async Task LeaveTeam(string teamId)
        {
            LogContext.PushProperty("Method", "LeaveTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter teamId: {teamId}");

            try
            {
                await _connection.SendAsync("LeaveTeam", teamId);

                logger.Information($"Removing the current user from the hub group with the name {teamId}");
            }
            catch (Exception e)
            {
                logger.Information(e, "Returning");
            }
        }

        #endregion

        #region Message

        /// <summary>
        /// Sends a message to a hub group
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public static async Task SendMessage(Message message)
        {
            LogContext.PushProperty("Method","SendMessage");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}");

            await _connection.SendAsync("SendMessage", message);

            logger.Information($"Sent message #{message.Id} to the channel #{message.RecipientId}");
        }

        /// <summary>
        /// Update a message's data and notify other clients
        /// </summary>
        /// <param name="message">The updated message object</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public static async Task UpdateMessage(Message message)
        {
            LogContext.PushProperty("Method", "UpdateMessage");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}");

            await _connection.SendAsync("UpdateMessage", message);

            logger.Information($"Updated message #{message.Id} from the channel #{message.RecipientId}");
        }

        public static async Task DeleteMessage(Message message)
        {
            LogContext.PushProperty("Method", "DeleteMessage");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}");

            await _connection.SendAsync("DeleteMessage", message);

            logger.Information($"Deleted message #{message.Id} from the channel #{message.RecipientId}");
        }

        public static async Task CreateMessageReaction(Message message, Reaction reaction)
        {
            LogContext.PushProperty("Method", "CreateMessageReaction");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}, reaction={reaction}");

            await _connection.SendAsync("CreateMessageReaction", message, reaction);

            logger.Information($"Created reaction #{reaction.Id} to the message #{message.Id} from the channel #{message.RecipientId}");
        }

        public static async Task DeleteMessageReaction(Message message, Reaction reaction)
        {
            LogContext.PushProperty("Method", "DeleteMessageReaction");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter message={message}, reaction={reaction}");

            await _connection.SendAsync("DeleteMessageReaction", message, reaction);

            logger.Information($"Deleted reaction #{reaction.Id} from the message #{message.Id} from the channel #{message.RecipientId}");
        }

        #endregion

        #region Team

        /// <summary>
        /// Adds the user to the hub group
        /// </summary>
        /// <param name="userId">Id of the user to add</param>
        /// <param name="teamId">Id the of team to add user to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public static async Task CreateTeam(Team team)
        {
            LogContext.PushProperty("Method", "CreateTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter team={team}");

            await _connection.SendAsync("CreateTeam", team);

            logger.Information($"Created team #{team.Id} ({team.CreationDate})");
        }

        public static async Task UpdateTeam(Team team)
        {
            LogContext.PushProperty("Method", "UpdateTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter team={team}");

            await _connection.SendAsync("UpdateTeam", team);

            logger.Information($"Updated team '{team.Name}' #{team.Id} ({team.CreationDate})");
        }

        public static async Task DeleteTeam(Team team)
        {
            LogContext.PushProperty("Method", "DeleteTeam");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter team={team}");

            await _connection.SendAsync("DeleteTeam", team);

            logger.Information($"Deleted team #{team.Id} ({team.CreationDate})");
        }

        #endregion

        #region Channel

        public static async Task CreateChannel(Channel channel)
        {
            LogContext.PushProperty("Method", "CreateChannel");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter channel={channel}");

            await _connection.SendAsync("CreateChannel", channel);

            logger.Information($"Created channel #{channel.ChannelId} in team #{channel.TeamId})");
        }

        public static async Task UpdateChannel(Channel channel)
        {
            LogContext.PushProperty("Method", "UpdateChannel");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter channel={channel}");

            await _connection.SendAsync("UpdateChannel", channel);

            logger.Information($"Updated channel #{channel.ChannelId} in team #{channel.TeamId})");
        }

        public static async Task DeleteChannel(Channel channel)
        {
            LogContext.PushProperty("Method", "DeleteChannel");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter channel={channel}");

            await _connection.SendAsync("DeleteChannel", channel);

            logger.Information($"Deleted channel #{channel.ChannelId} from team #{channel.TeamId})");
        }

        #endregion

        #region Team Roles

        public static async Task CreateTeamRole(TeamRole role)
        {
            LogContext.PushProperty("Method", "CreateTeamRole");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter role={role}");

            await _connection.SendAsync("CreateTeamRole", role);

            logger.Information($"Created team role '{role.Role}' in team #{role.TeamId})");
        }

        public static async Task UpdateTeamRole(TeamRole role)
        {
            LogContext.PushProperty("Method", "UpdateTeamRole");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter role={role}");

            await _connection.SendAsync("UpdateTeamRole", role);

            logger.Information($"Updated team role '{role.Role}' in team #{role.TeamId})");
        }

        public static async Task DeleteTeamRole(TeamRole role)
        {
            LogContext.PushProperty("Method", "DeleteTeamRole");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter role={role}");

            await _connection.SendAsync("DeleteTeamRole", role);

            logger.Information($"Deleted team role '{role.Role}' from team #{role.TeamId})");
        }

        public static async Task UpdateRolePermissions(TeamRole role, IEnumerable<Permissions> permissions)
        {
            LogContext.PushProperty("Method", "UpdateRolePermissions");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter role={role}, permissions={permissions}");

            await _connection.SendAsync("UpdateRolePermissions", role);

            logger.Information($"Updated team role permissions of '{role.Role}' in team #{role.TeamId} to '{string.Join(", ", permissions)}')");
        }

        #endregion

        #region Member

        public static async Task SendInvitation(User user, Team team)
        {
            LogContext.PushProperty("Method", "SendInvitation");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("SendInvitation", user, team);

            logger.Information($"Sent invitation to team #{team.Id} to user #{user.Id}");
        }

        public static async Task AddMember(User user, Team team)
        {
            LogContext.PushProperty("Method", "AddMember");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("AddMember", user, team);

            logger.Information($"Added member #{user.Id} to team #{team.Id}");
        }

        public static async Task UpdateMember(User user, Team team)
        {
            LogContext.PushProperty("Method", "UpdateMember");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("UpdateMember", user, team);

            logger.Information($"Updated member #{user.Id} in team #{team.Id}");
        }

        public static async Task RemoveMember(User user, Team team)
        {
            LogContext.PushProperty("Method", "RemoveMember");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}, team={team}");

            await _connection.SendAsync("RemoveMember", user, team);

            logger.Information($"Removed user #{user.Id} from team #{team.Id}");
        }

        #endregion

        #region User

        public static async Task UpdateUser(User user)
        {
            LogContext.PushProperty("Method", "UpdateUser");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called with parameter user={user}");

            await _connection.SendAsync("UpdateUser", user);

            logger.Information($"User information updated #{user.Id}");
        }

        #endregion

        #region Helpers

        private static async Task Reconnect(Exception e)
        {
            LogContext.PushProperty("Method","Reconnect");
            LogContext.PushProperty("SourceContext", "SignalRService");

            logger.Information($"Function called");

            await Task.Delay(500);
            _connection = await CreateHubConnection();
            await _connection.StartAsync();

            logger.Information($"Building a new connection to the hub");
        }

        private static async Task<HubConnection> CreateHubConnection()
        {
            LogContext.PushProperty("Method","CreateHubConnection");
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

        private static SignalREventArgs<T> BuildArgument<T>(T value)
        {
            return new SignalREventArgs<T>(value);
        }

        private static SignalREventArgs<TOne, TTwo> BuildArgument<TOne, TTwo>(TOne firstValue, TTwo secondValue)
        {
            return new SignalREventArgs<TOne, TTwo>(firstValue, secondValue);
        }

        #endregion
    }
}
