using Messenger.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
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
        
        private readonly HubConnection _connection;

        #endregion

        /// <summary>
        /// Delegate on "ReceiveMessage"(Hub Method)
        /// </summary>
        public event Action<Message> MessageReceived;

        public SignalRService()
        {
            _connection = new HubConnectionBuilder()
                .WithUrl(HUB_URL)
                .Build();

            _connection.On<Message>("ReceiveMessage", (message) => MessageReceived?.Invoke(message));
        }

        /// <summary>
        /// Starts the connection with the preset
        /// </summary>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task Open()
        {
            try
            {
                await _connection.StartAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Signal-R Exception: {e.Message}");
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
                Debug.WriteLine($"Signal-R Exception: {e.Message}");
            }
        }

        /// <summary>
        /// Joins a hub group with the current connection id
        /// </summary>
        /// <param name="teamId">Team id as the group name</param>
        /// <returns>Asynchronous task to be awaited</returns>
        public async Task JoinTeam(string teamId)
        {
            await _connection.SendAsync("JoinTeam", teamId);
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
    }
}
