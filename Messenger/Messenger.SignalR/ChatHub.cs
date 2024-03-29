﻿using Messenger.Core.Models;
using Messenger.SignalR.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Messenger.SignalR
{
    /// <summary>
    /// Hub definition for SignalR service, that clients can connect to
    /// Defines various hub methods to be consumed by application service
    /// </summary>
    public class ChatHub : Hub
    {
        #region Private

        private readonly static ConnectionMapping _connections = new ConnectionMapping();

        #endregion

        #region Connection

        /// <summary>
        /// Adds the current connection id to the given user id
        /// </summary>
        /// <param name="userId">Id of the user logged in</param>
        /// <returns>Task to be awaited</returns>
        public Task Register(string userId)
        {
            _connections.Add(userId, Context.ConnectionId);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Adds the current connection id to a client group on the hub
        /// </summary>
        /// <param name="teamId">Client group name to be added to</param>
        /// <returns>Task to be awaited</returns>
        public async Task JoinTeam(string userId, string teamId)
        {
            foreach (string connection in _connections.GetConnections(userId))
            {
                await Groups.AddToGroupAsync(connection, teamId);
            }
        }

        public async Task LeaveTeam(string userId, string teamId)
        {
            foreach (string connection in _connections.GetConnections(userId))
            {
                await Groups.RemoveFromGroupAsync(connection, teamId);
            }
        }

        #endregion

        #region Notification

        public async Task SendNotificationToUser(Notification notification)
        {
            await Clients.Clients(_connections.GetConnections(notification.RecipientId)).SendAsync("ReceiveNotification", notification);
        }

        #endregion

        #region Message

        /// <summary>
        /// Sends the message to the recipients
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Task to be awaited</returns>
        public async Task SendMessage(Message message, string teamId)
        {
            await Clients.Group(teamId).SendAsync("ReceiveMessage", message);
        }

        /// <summary>
        /// Send updated message to the recipients
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Task to be awaited</returns>
        public async Task UpdateMessage(Message message, string teamId)
        {
            await Clients.Group(teamId).SendAsync("MessageUpdated", message);
        }
        
        public async Task UpdateMessageReactions(Message message, string teamId)
        {
            await Clients.Group(teamId).SendAsync("MessageReactionsUpdated", message);
        }

        public async Task DeleteMessage(Message message, string teamId)
        {
            await Clients.Group(teamId).SendAsync("MessageDeleted", message);
        }

        #endregion

        #region Team

        public async Task CreateTeam(Team team)
        {
            await Clients.Group(team.Id.ToString()).SendAsync("TeamCreated", team);
        }

        public async Task UpdateTeam(Team team)
        {
            await Clients.Group(team.Id.ToString()).SendAsync("TeamUpdated", team);
        }

        public async Task DeleteTeam(Team team)
        {
            await Clients.Group(team.Id.ToString()).SendAsync("TeamDeleted", team);
        }

        #endregion

        #region Channel

        public async Task CreateChannel(Channel channel)
        {
            await Clients.Group(channel.TeamId.ToString()).SendAsync("ChannelCreated", channel);
        }

        public async Task UpdateChannel(Channel channel)
        {
            await Clients.Group(channel.TeamId.ToString()).SendAsync("ChannelUpdated", channel);
        }

        public async Task DeleteChannel(Channel channel)
        {
            await Clients.Group(channel.TeamId.ToString()).SendAsync("ChannelDeleted", channel);
        }

        #endregion

        #region Team Roles

        /// <summary>
        /// Update a team's roles and notify other clients
        /// </summary>
        /// <param name="teamId">The id of the team whose roles got updated</param>
        /// <returns>Task to be awaited</returns>
        public async Task AddOrUpdateTeamRole(TeamRole role)
        {
            await Clients.Group(role.TeamId.ToString()).SendAsync("TeamRoleUpdated", role);
        }

        /// <summary>
        /// Update a team's roles and notify other clients
        /// </summary>
        /// <param name="teamId">The id of the team whose roles got updated</param>
        /// <returns>Task to be awaited</returns>
        public async Task DeleteTeamRole(TeamRole role)
        {
            await Clients.Group(role.TeamId.ToString()).SendAsync("TeamRoleDeleted", role);
        }

        #endregion

        #region Member

        public async Task SendInvitation(User user, Team team)
        {
            foreach (var connectionId in _connections.GetConnections(user.Id))
            {
                // Add the user to the hub group
                await Groups.AddToGroupAsync(connectionId, team.Id.ToString());

                // Notify target client with team id
                await Clients.Client(connectionId).SendAsync("ReceiveInvitation", team);
            }

            await AddMember(user, team);
        }

        public async Task AddMember(User user, Team team)
        {
            await Clients.Group(team.Id.ToString()).SendAsync("MemberAdded", user, team);
        }

        public async Task UpdateMember(User user, Team team)
        {
            await Clients.Group(team.Id.ToString()).SendAsync("MemberUpdated", user, team);
        }

        public async Task RemoveMember(User user, Team team)
        {
            // Notify target client with team id
            await Clients.Group(team.Id.ToString()).SendAsync("MemberRemoved", user, team);

            foreach (var connectionId in _connections.GetConnections(user.Id))
            {
                // Remove the user from the hub group
                await Groups.RemoveFromGroupAsync(connectionId, team.Id.ToString());
            }
        }

        #endregion

        #region User

        public async Task UpdateUser(User user)
        {
            await Clients.Clients(_connections.GetConnections(user.Id)).SendAsync("UserUpdated", user);
        }

        #endregion
    }
}
