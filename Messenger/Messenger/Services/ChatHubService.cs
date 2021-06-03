using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public ConcurrentDictionary<uint, List<Message>> MessagesByConnectedTeam { get; }

        public uint? CurrentTeamId { get; set; }

        public UserViewModel CurrentUser { get; private set; }

        #region Event Handlers

        public event EventHandler<Message> MessageReceived;

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

            MessengerService.RegisterListenerForMessages(MessageReceived);
            MessengerService.RegisterListenerForInvites(InvitationReceived);

            MessageReceived += OnMessageReceived;
        }

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

        public async Task<IEnumerable<Team>> GetTeamsList()
        {
            if (CurrentUser == null)
            {
                return null;
            }

            return await MessengerService.LoadTeams(CurrentUser.Id);
        }

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

        private async void OnMessageReceived(object sender, Message message)
        {
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
        }
    }
}
