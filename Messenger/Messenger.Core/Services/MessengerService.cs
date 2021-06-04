using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
        /// <returns>List of teams the user has membership of, null if none exists</returns>
        public async Task<IList<Team>> Initialize(string userId)
        {
            await SignalRService.Open(userId);

            // Check the validity of user id
            if (string.IsNullOrWhiteSpace(userId))
            {
                HandleException(nameof(this.Initialize), "invalid user id");
                return null;
            }

            var teams = await TeamService.GetAllTeamsByUserId(userId);

            // Exit if the user has no team
            if (teams == null || teams.Count() <= 0)
            {
                return null;
            }

            List<Team> result = new List<Team>();
            // Join the signal-r hub
            foreach (var team in teams)
            {
                await SignalRService.JoinTeam(team.Id.ToString());
                result.Add(team);
            }

            return result;
        }

        /// <summary>
        /// Registers the action from the view model to signal-r event
        /// </summary>
        /// <param name="onMessageReceived">Action to run upon receiving a message</param>
        public void RegisterListenerForMessages(EventHandler<Message> onMessageReceived)
        {
            SignalRService.MessageReceived += onMessageReceived;
        }

        public void RegisterListenerForInvites(EventHandler<uint> onInviteReceived)
        {
            SignalRService.InviteReceived += onInviteReceived;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Saves the message to the database and simultaneously broadcasts to the connected Signal-R hub
        /// </summary>
        /// <param name="message">A complete message object to send</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> SendMessage(Message message)
        {
            // Check the validity of the message
            if (!ValidateMessage(message))
            {
                HandleException(nameof(this.SendMessage), "invalid message object");
                return false;
            }

            // Save to database
            await MessageService.CreateMessage(
                message.RecipientId,
                message.SenderId,
                message.Content,
                message.ParentMessageId);

            // Broadcasts the message to the hub
            await SignalRService.SendMessage(message);

            return true;
        }

        /// <summary>
        /// Saves new team to database and join the hub group of the team
        /// </summary>
        /// <param name="creatorId">Creator user id</param>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team(optional)</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> CreateTeam(string creatorId, string teamName, string teamDescription = "")
        {
            // Create team and save to database
            uint? teamId = await TeamService.CreateTeam(teamName, teamDescription);

            if (teamId == null)
            {
                HandleException(nameof(this.CreateTeam), "invalid team id");
                return false;
            }
            
            // Create membership for the creator and save to database
            await TeamService.AddMember(creatorId, (uint)teamId);

            // Join the new hub group of the team
            await SignalRService.JoinTeam(teamId.ToString());

            return true;
        }

        /// <summary>
        /// Saves new membership to database and add the user to the hub group of the team
        /// </summary>
        /// <param name="userId">User id to add</param>
        /// <param name="connectionId">Connection id of the user to add</param>
        /// <param name="teamId">Id of the team to add the user to</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> InviteUser(string userId, uint teamId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                HandleException(nameof(this.InviteUser), "invalid member id");
                return false;
            }

            // Create membership for the user and save to database
            await TeamService.AddMember(userId, teamId);

            // Add user to the hub group if the user is connected (will be handled in SignalR)
            await SignalRService.AddToTeam(userId, teamId.ToString());

            return true;
        }

        /// <summary>
        /// Load all teams the current user has membership of
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <returns>List of teams</returns>
        public async Task<IEnumerable<Team>> LoadTeams(string userId)
        {
            return await TeamService.GetAllTeamsByUserId(userId);
        }

        /// <summary>
        /// Gets the team with the given team id
        /// </summary>
        /// <param name="teamId">Id of the team to retrieve</param>
        /// <returns>A complete Team object</returns>
        public async Task<Team> GetTeam(uint teamId)
        {
            return await TeamService.GetTeam(teamId);
        }

        /// <summary>
        /// Load all messages of the team
        /// </summary>
        /// <param name="teamId">Id of the team to load messsages from</param>
        /// <returns>List of messages</returns>
        public async Task<IEnumerable<Message>> LoadMessages(uint teamId)
        {
            return await MessageService.RetrieveMessages(teamId);
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
            if (message == null || string.IsNullOrWhiteSpace(message.SenderId))
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

        /// <summary>
        /// Handles MessengerService specific exception
        /// </summary>
        /// <param name="methodName">Name of the method that raised exception</param>
        /// <param name="reason">Reason for the exception</param>
        private void HandleException(string methodName, string reason)
        {
            Debug.Write($"Core.MessengerService::{methodName} failed. ");
            Debug.WriteLine($"(Reason: {reason})");
        }

        #endregion
    }
}
