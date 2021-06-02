using Messenger.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Messenger.SignalR
{
    /// <summary>
    /// Hub definition for SignalR service, that clients can connect to
    /// Defines various hub methods to be consumed by application service
    /// </summary>
    public class ChatHub : Hub
    {
        /// <summary>
        /// Adds the current connection id to a client group on the hub
        /// </summary>
        /// <param name="teamId">Client group name to be added to</param>
        /// <returns>Task to be awaited</returns>
        public async Task JoinTeam(string teamId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, teamId);
        }

        /// <summary>
        /// Adds the given connection id to a client group on the hub
        /// </summary>
        /// <param name="connectionId">Current connection id of the user</param>
        /// <param name="teamId">Client group name to be added to</param>
        /// <returns>Task to be awaited</returns>
        public async Task AddToTeam(string connectionId, string teamId)
        {
            await Groups.AddToGroupAsync(connectionId, teamId);
        }

        /// <summary>
        /// Sends the message to the recipients
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Task to be awaited</returns>
        public async Task SendMessage(Message message)
        {
            string groupName = message.RecipientId.ToString();
            
            await Clients.Group(groupName).SendAsync("ReceiveMessage", message);
        }
    }
}