using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Messenger.Services
{
    public class ChatHubService
    {
        #region Private

        private MessengerService MessengerService => Singleton<MessengerService>.Instance;
        private UserDataService UserDataService => Singleton<UserDataService>.Instance;
        private UserService UserService => Singleton<UserService>.Instance;

        #endregion

        #region Properties

        public ConcurrentDictionary<uint, List<Message>> MessagesByConnectedTeam { get; }

        public uint? CurrentTeamId { get; set; }

        public UserViewModel CurrentUser { get; private set; }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Event handler for "ReceiveMessage"
        /// </summary>
        public event EventHandler<Message> MessageReceived;

        /// <summary>
        /// Event handler for "ReceiveInvitation"
        /// </summary>
        public event EventHandler<uint> InvitationReceived;

        #endregion

        public ChatHubService()
        {
            MessagesByConnectedTeam = new ConcurrentDictionary<uint, List<Message>>();

            Initialize();
        }

        private async void Initialize()
        {
            CurrentUser = await UserDataService.GetUserAsync();

            MessengerService.RegisterListenerForMessages(OnMessageReceived);
            MessengerService.RegisterListenerForInvites(OnInvitationReceived);
        }

        /// <summary>
        /// Gets all messages of the current team
        /// </summary>
        /// <returns>List of messages</returns>
        public async Task<IEnumerable<Message>> GetMessages()
        {
            if (CurrentTeamId == null)
            {
                return null;
            }

            // Checks the cache if the messages has been loaded for the team
            if (MessagesByConnectedTeam.TryGetValue((uint)CurrentTeamId, out List<Message> fromCache))
            {
                // Loads from cache
                return fromCache;
            }
            else
            {
                // Loads from database
                return await MessengerService.LoadMessages((uint)CurrentTeamId);
            }
        }

        /// <summary>
        /// Gets the list of teams of the current user
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<Team>> GetTeamsList()
        {
            if (CurrentUser == null)
            {
                return null;
            }

            return await MessengerService.LoadTeams(CurrentUser.Id);
        }

        /// <summary>
        /// Sends a message to the current team
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task SendMessage(string content)
        {
            var message = new Message()
            {
                Content = content,
                CreationTime = DateTime.Now,
                SenderId = CurrentUser.Id,
                RecipientId = (uint)CurrentTeamId
            };

            await MessengerService.SendMessage(message);
        }

        /// <summary>
        /// Loads the sender information and saves the message to the cache
        /// Fires on "ReceiveMessage"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Received message object</param>
        private async void OnMessageReceived(object sender, Message message)
        {
            Debug.WriteLine($"ChatHubService.{nameof(this.OnMessageReceived)}::" +
                $"{message.Content} From {message.SenderId} To Team #{message.RecipientId}::{message.CreationTime}");

            var teamId = message.RecipientId;

            // Loads user data of the sender
            message.Sender = await UserService.GetUser(message.SenderId);

            // Adds to message dictionary
            MessagesByConnectedTeam.AddOrUpdate(
                teamId,
                new List<Message>() { message },
                (key, collection) => {
                    collection.Add(message);
                    return collection;
                });

            // Invoke registered ui events
            MessageReceived?.Invoke(this, message);
        }

        /// <summary>
        /// Fires on "ReceiveInvitation"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="teamId">Id of the team that the user was invited to</param>
        private void OnInvitationReceived(object sender, uint teamId)
        {
            Debug.WriteLine($"ChatHubService.{nameof(this.OnInvitationReceived)}::" +
                $"Invitation To Team #{teamId}");

            // Invoke registered ui events
            InvitationReceived?.Invoke(this, teamId);
        }
    }
}
