﻿using Messenger.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Core.Services
{
    public class SignalRService
    {
        #region Private

        private const string HUB_URL = @"https://vpr.azurewebsites.net/chathub";

        private HubConnection _connection;

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

        public event EventHandler<Channel> ChannelUpdated;

        public SignalRService()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .ConfigureLogging(log =>
                {
                    log.AddConsole();
                })
                .Build();

            _connection.On<Message>("ReceiveMessage", (message) => MessageReceived?.Invoke(this, message));
            _connection.On<uint>("ReceiveInvitation", (teamId) => InviteReceived?.Invoke(this, teamId));
            _connection.On<Channel>("ChannelUpdated", (channel) => ChannelUpdated?.Invoke(this, channel));
        }

        /// <summary>
        /// Starts the connection with the preset
        /// </summary>
        /// <param name="userId">Id of the current user</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task Open(string userId)
        {
            try
            {
                if (_connection.State == HubConnectionState.Disconnected)
                {
                    await _connection.StartAsync();
                    await _connection.SendAsync("Register", userId);
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(SignalRService)}.{nameof(this.Open)} : {e.Message}");
            }
        }

        /// <summary>
        /// Closes the connection with the hub
        /// </summary>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task Close()
        {
            try
            {
                await _connection.StopAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(SignalRService)}.{nameof(this.Close)} : {e.Message}");
            }
        }

        /// <summary>
        /// Joins a hub group with the current connection id
        /// </summary>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task JoinTeam(string teamId)
        {
            try
            {
                await _connection.SendAsync("JoinTeam", teamId);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"{nameof(SignalRService)}.{nameof(this.JoinTeam)} : {e.Message}");
            }
        }

        /// <summary>
        /// Sends a message to a hub group
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task SendMessage(Message message)
        {
            await _connection.SendAsync("SendMessage", message);
        }

        /// <summary>
        /// Adds the user to the hub group
        /// </summary>
        /// <param name="userId">Id of the user to add</param>
        /// <param name="teamId">Id the of team to add user to</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task AddToTeam(string userId, string teamId)
        {
            await _connection.SendAsync("AddToTeam", userId, teamId);
        }

        public async Task UpdateChannel(Channel channel)
        {
            await _connection.SendAsync("UpdateChannel", channel);
        }

        #region Helpers

        private async Task Reconnect(Exception e)
        {
            await Task.Delay(500);
            _connection = await CreateHubConnection();
            await _connection.StartAsync();
        }

        private async Task<HubConnection> CreateHubConnection()
        {
            HubConnection hubConnection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .Build();

            hubConnection.Closed += Reconnect;

            await hubConnection.StartAsync();
            return hubConnection;
        }

        #endregion
    }
}
