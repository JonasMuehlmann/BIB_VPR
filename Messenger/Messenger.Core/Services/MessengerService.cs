using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Core.Services
{
    /// <summary>
    /// Container service class for message service, team service and signal-r service
    /// </summary>
    public class MessengerService
    {
        private MessageService MessageService => Singleton<MessageService>.Instance;

        private TeamService TeamService => Singleton<TeamService>.Instance;

        private SignalRService SignalRService => Singleton<SignalRService>.Instance;

        #region Initializers

        /// <summary>
        /// Connects the given user to the teams he is a member of
        /// </summary>
        /// <param name="userId">The user to connect to his teams</param>
        /// <returns>true on success, false on invalid user id (error will be handled in each service)</returns>
        public async Task<bool> Initialize(string userId)
        {
            // Open the connection to hub
            await SignalRService.Open();

            // Check the validity of user id
            if (string.IsNullOrWhiteSpace(userId))
            {
                Debug.WriteLine($"Messenger Exception: invalid user id");
                return false;
            }

            var memberships = await TeamService.GetAllMembershipByUserId(userId);

            // Exit if the user has no membership
            if (memberships.Count <= 0)
            {
                Debug.WriteLine($"No membership found");
                return true;
            }

            // Subscribe to corresponding hub groups
            //foreach (var teamId in memberships.Select(m => m.TeamId.ToString())) 
            //{
            //    await SignalRService.JoinTeam(teamId);
            //}
            await SignalRService.JoinTeam("1");

            return true;
        }

        /// <summary>
        /// Registers the action from the view model to signal-r event
        /// </summary>
        /// <param name="onMessageReceived">Action to run upon receiving a message</param>
        public void RegisterListener(Action<Message> onMessageReceived)
        {
            SignalRService.MessageReceived += onMessageReceived;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Saves the message to the database and broadcasts simultaneously to the connected Signal-R hub
        /// </summary>
        /// <param name="message">A complete message object to send</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> SendMessage(Message message)
        {
            // Check the validity of the message
            if (!ValidateMessage(message))
            {
                return false;
            }

            // Check if the message is a reply of a message
            if (message.ParentMessageId != null)
            {
                // Save to database with the parent message id
                await MessageService.CreateMessage(
                    message.RecipientId,
                    message.SenderId,
                    message.Content,
                    message.ParentMessageId);
            }
            else
            {
                // Save to database without the parent message id(null)
                await MessageService.CreateMessage(
                    message.RecipientId,
                    message.SenderId,
                    message.Content);
            }

            // Broadcasts the message to the hub
            await SignalRService.SendMessage(message);

            return true;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Checks the validity of the message to be sent
        /// </summary>
        /// <param name="message">A complete message object to be sent</param>
        /// <returns>true on valid, false on invalid</returns>
        private bool ValidateMessage(Message message)
        {
            // Sender / Recipient Id
            if (message == null
                || !uint.TryParse(message.RecipientId.ToString(), out uint recipientId)
                || string.IsNullOrWhiteSpace(message.SenderId))
            {
                Debug.WriteLine("Messenger Exception: invalid sender/recipient id");
                return false;
            }
            
            // Content
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                Debug.WriteLine("Messenger Exception: no content found to be sent");
                return false;
            }

            // Valid
            return true;
        }

        #endregion
    }
}
