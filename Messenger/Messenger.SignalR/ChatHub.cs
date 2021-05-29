using Messenger.Core.Models;
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
        /// Global cache dictionary for connected users
        /// </summary>
        public static ConcurrentDictionary<string, List<string>> ConnectedUsers = new ConcurrentDictionary<string, List<string>>();

        /// <summary>
        /// Adds a current connection to the global cache dictionary
        /// </summary>
        /// <returns>Task to be awaited</returns>
        public override Task OnConnectedAsync()
        {
            Trace.TraceInformation($"MapHub started. ID: {Context.ConnectionId}");

            string username = Context.User.Identity.Name;

            // Try to get a list of existing user connections from the cache
            List<string> userConnectionIds;
            ConnectedUsers.TryGetValue(username, out userConnectionIds);

            // First user connection
            if (userConnectionIds == null)
            {
                userConnectionIds = new List<string>();
            }

            // Add current connection id
            userConnectionIds.Add(Context.ConnectionId);
            
            // Update global dictionary of connected users
            ConnectedUsers.TryAdd(username, userConnectionIds);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            string username = Context.User.Identity.Name;

            List<string> userConnectionIds;
            ConnectedUsers.TryGetValue(username, out userConnectionIds);

            // Remove current connection id from the list of the current user connections
            userConnectionIds.Remove(Context.ConnectionId);

            // If there are no more connections for the user,
            // remove the user from the global dictionary
            if (userConnectionIds.Count == 0)
            {
                List<string> garbage; // to be collected by the garbage collector
                ConnectedUsers.TryRemove(username, out garbage);
            }

            return base.OnDisconnectedAsync(exception);
        }

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
            
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("ReceiveMessage", message);
        }
    }
}