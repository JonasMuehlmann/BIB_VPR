using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Core.Services
{
    public class MessengerService
    {
        private MessageService MessageService => Singleton<MessageService>.Instance;

        private TeamService TeamService => Singleton<TeamService>.Instance;

        private SignalRService SignalRService => Singleton<SignalRService>.Instance;

        /// <summary>
        /// Saves the message to the database and broadcasts simultaneously to the connected Signal-R hub
        /// </summary>
        /// <param name="message">A complete message object to send</param>
        /// <returns>true on success, false on failure (error will be handled in each service)</returns>
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
    }
}
