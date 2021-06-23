using Messenger.Core.Models;
using Messenger.Core.Helpers;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public class SignalRService
    {
        #region Private

        private const string HUB_URL = @"https://vpr.azurewebsites.net/chathub";

        private HubConnection _connection;

        public Serilog.ILogger logger = GlobalLogger.Instance;

        #endregion

        public string ConnectionId
        {
            get
            {
                return _connection.ConnectionId;
            }
        }

        /// <summary>
        /// Delegate on "ReceiveMessage"(Hub Method)
        /// </summary>
        public event EventHandler<Message> MessageReceived;
        public event EventHandler<uint> InviteReceived;
        public event EventHandler<Team> TeamUpdated;
        public event EventHandler<Message> MessageUpdated;
        public event EventHandler<Channel> ChannelUpdated;
        public event EventHandler<User> UserUpdated;
        public event EventHandler<uint> TeamRolesUpdated;
        public event EventHandler<uint> RolePermissionsUpdated;

        public SignalRService()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .ConfigureLogging(log =>
                {
                    log.AddConsole();
                })
                .Build();

            _connection.On<Message>("ReceiveMessage", (message)     => MessageReceived?.Invoke(this, message));
            _connection.On<uint>("ReceiveInvitation", (teamId)      => InviteReceived?.Invoke(this, teamId));
            _connection.On<Team>("TeamUpdated", (team)              => TeamUpdated?.Invoke(this, team));
            _connection.On<Message>("MessageUpdated", (message)     => MessageUpdated?.Invoke(this, message));
            _connection.On<Channel>("ChannelUpdated", (channel)     => ChannelUpdated?.Invoke(this, channel));
            _connection.On<User>("UserUpdated", (user)              => UserUpdated?.Invoke(this, user));
            _connection.On<uint>("TeamRolesUpdated", (teamId) => TeamRolesUpdated?.Invoke(this, teamId));
            _connection.On<uint>("RolePermissionsUpdated", (teamId) => RolePermissionsUpdated?.Invoke(this, teamId));
        }

        /// <summary>
        /// Starts the connection with the preset
        /// </summary>
        /// <param name="userId">Id of the current user</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task Open(string userId)
        {

            LogContext.PushProperty("Method","Open");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

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
        public async Task Close()
        {
            LogContext.PushProperty("Method","Close");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

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
        public async Task JoinTeam(string teamId)
        {
            LogContext.PushProperty("Method","JoinTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

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

        /// <summary>
        /// Sends a message to a hub group
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SendMessage(Message message)
        {
            LogContext.PushProperty("Method","SendMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter message={message}");

            await _connection.SendAsync("SendMessage", message);

            logger.Information($"Sending the message to the hub");
        }

        /// <summary>
        /// Adds the user to the hub group
        /// </summary>
        /// <param name="userId">Id of the user to add</param>
        /// <param name="teamId">Id the of team to add user to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task AddToTeam(string userId, string teamId)
        {
            LogContext.PushProperty("Method","AddToTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter userId={userId}, teamId={teamId}");

            await _connection.SendAsync("AddToTeam", userId, teamId);

            logger.Information($"Adding the user identified by userId={userId} to the hub group identified by {teamId}");
        }
        /// <summary>
        /// Update a message's data and notify other clients
        /// </summary>
        /// <param name="message">The updated message object</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task UpdateMessage(Message message)
        {
            await _connection.SendAsync("UpdateMessage", message);
        }

        /// <summary>
        /// Update a teams data and notify other clients
        /// </summary>
        /// <param name="team">The updated team object</param>
        /// <returns>Asynchronous task to be awaited</returns>

        public async Task UpdateTeam(Team team)
        {
            await _connection.SendAsync("UpdateTeam", team);
        }

        public async Task UpdateChannel(Channel channel)
        {
            await _connection.SendAsync("UpdateChannel", channel);

        }

        public async Task UpdateUser(User user)
        {
            await _connection.SendAsync("UpdateUser",user);
        }

        public async Task UpdateTeamRoles(uint teamId)
        {
            await _connection.SendAsync("UpdateTeamRoles", teamId);
        }

        public async Task UpdateRolePermission(uint teamId)
        {
            await _connection.SendAsync("UpdateRolePermission", teamId);
        }

        #region Helpers

        private async Task Reconnect(Exception e)
        {
            LogContext.PushProperty("Method","Reconnect");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");

            await Task.Delay(500);
            _connection = await CreateHubConnection();
            await _connection.StartAsync();

            logger.Information($"Building a new connection to the hub");
        }

        private async Task<HubConnection> CreateHubConnection()
        {
            LogContext.PushProperty("Method","CreateHubConnection");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");

            HubConnection hubConnection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .Build();

            hubConnection.Closed += Reconnect;

            await hubConnection.StartAsync();

            logger.Information($"Building a new connection to the hub");

            return hubConnection;
        }

        #endregion
    }
}
