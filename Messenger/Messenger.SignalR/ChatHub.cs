using Messenger.Core.Models;
using Messenger.Core.Helpers;
using Messenger.SignalR.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Serilog.Context;
using Serilog;

namespace Messenger.SignalR
{
    /// <summary>
    /// Hub definition for SignalR service, that clients can connect to
    /// Defines various hub methods to be consumed by application service
    /// </summary>
    public class ChatHub : Hub
    {
        private readonly static ConnectionMapping _connections = new ConnectionMapping();

        private string _userId;

        public ILogger logger => GlobalLogger.Instance;

        /// <summary>
        /// Adds the current connection id to the given user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <returns>Task to be awaited</returns>
        public Task Register(string userId)
        {
            LogContext.PushProperty("Method","Register");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}");

            _userId = userId;
            _connections.Add(userId, Context.ConnectionId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds the current connection id to a client group on the hub
        /// </summary>
        /// <param name="teamId">Client group name to be added to</param>
        /// <returns>Task to be awaited</returns>
        public async Task JoinTeam(string teamId)
        {
            LogContext.PushProperty("Method","JoinTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter teamId={teamId}");

            await Groups.AddToGroupAsync(Context.ConnectionId, teamId);

            logger.Information($"Adding user identified by connectionId={Context.ConnectionId} to the team identified by {teamId}");
        }

        /// <summary>
        /// Adds the given connection id to a client group on the hub
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <param name="teamId">Client group name to be added to</param>
        /// <returns>Task to be awaited</returns>
        public async Task AddToTeam(string userId, string teamId)
        {
            LogContext.PushProperty("Method","AddToTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            foreach (var connectionId in _connections.GetConnections(userId))
            {
                // Add the user to the hub group
                await Groups.AddToGroupAsync(connectionId, teamId);

            logger.Information($"Adding user identified by connectionId={Context.ConnectionId} to the team identified by {teamId}");

                // Notify target client with team id
                await Clients.Client(connectionId).SendAsync("ReceiveInvitation", Convert.ToUInt32(teamId));

                logger.Information($"Sending invitation to the user identified by {userId}");
            }
        }

        /// <summary>
        /// Sends the message to the recipients
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Task to be awaited</returns>
        public async Task SendMessage(Message message)
        {
            LogContext.PushProperty("Method","SendMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters message={message}");

            string groupName = message.RecipientId.ToString();

            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);

            logger.Information($"Sending message to the group with the name {groupName}");
        }

        public async Task UpdateChanel(Channel channel)
        {
            await Clients.Group(channel.TeamId.ToString()).SendAsync("ChannelUpdated", channel);
        }

        /// <summary>
        /// Send updated message to the recipients
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Task to be awaited</returns>
        public async Task UpdateMessage(Message message)
        {
            await Clients.Group(message.Id.ToString()).SendAsync("MessageUpdated", message);
        }

        /// <summary>
        /// Removes the current connection id on disconnection
        /// </summary>
        /// <param name="exception">Exceptions to be handled on disconnection(handled by SignalR)</param>
        /// <returns>Task to be awaited</returns>
        public override Task OnDisconnectedAsync(Exception exception)
        {
            LogContext.PushProperty("Method","OnDisconnectedAsync");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");

            _connections.Remove(_userId, Context.ConnectionId);

            logger.Information($"Closed the session with connectionId={Context.ConnectionId}");

            return base.OnDisconnectedAsync(exception);
        }
    }
}
