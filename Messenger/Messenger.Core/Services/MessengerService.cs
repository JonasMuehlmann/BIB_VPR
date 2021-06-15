using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

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

        private FileSharingService FileSharingService => Singleton<FileSharingService>.Instance;

        public ILogger logger => GlobalLogger.Instance;
        #region Initializers

        /// <summary>
        /// Connects the given user to the teams he is a member of
        /// </summary>
        /// <param name="userId">The user to connect to his teams</param>
        /// <param name="connectionString">(optional)connection string to initialize with</param>
        /// <returns>List of teams the user has membership of, null if none exists</returns>
        public async Task<IList<Team>> Initialize(string userId, string connectionString = null)
        {
            // Initialize with given connection string
            if (connectionString != null)
            {
                MessageService.SetTestMode(connectionString);
                TeamService.SetTestMode(connectionString);
            }

            LogContext.PushProperty("Method","Initialize");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}");

            await SignalRService.Open(userId);

            // Check the validity of user id
            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.Information($"userId has been determined invalid");
                logger.Information($"Return value: null");

                return null;
            }

            var teams = await TeamService.GetAllTeamsByUserId(userId);

            // Exit if the user has no team
            if (teams == null || teams.Count() <= 0)
            {
                logger.Information($"No teams found for the current user");
                logger.Information($"Return value: null");

                return null;
            }

            logger.Information($"Loaded the following teams for the current user: {string.Join(", ", teams)}");

            List<Team> result = new List<Team>();
            // Join the signal-r hub
            foreach (var team in teams)
            {
                await SignalRService.JoinTeam(team.Id.ToString());
                result.Add(team);

                logger.Information($"Connected the current user to the team {team.Id}");
            }

            logger.Information($"Return value: {result}");

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

        public void RegisterListenerForTeamUpdate(EventHandler<Team> onTeamUpdated)
        {
            SignalRService.TeamUpdated += onTeamUpdated;
        }

        #endregion

        #region Commands

        /// <summary>
        /// Saves the message to the database and simultaneously broadcasts to the connected Signal-R hub
        /// </summary>
        /// <param name="message">A complete message object to send</param>
        /// <param name="attachmentFilePaths">An Enumerable of paths of files to attach to the message</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> SendMessage(Message message, IEnumerable<string> attachmentFilePaths = null)
        {
            if (attachmentFilePaths == null)
            {
                attachmentFilePaths = Enumerable.Empty<string>();
            }

            LogContext.PushProperty("Method","SendMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters attachmentFilePaths={string.Join(", ", attachmentFilePaths)} , message={message}");

            // Check the validity of the message
            if (!ValidateMessage(message))
            {
                logger.Information($"message object has been determined invalid");
                logger.Information($"Return value: false");

                return false;
            }

            if (attachmentFilePaths != null)
            {
                foreach (var attachmentFilePath in attachmentFilePaths)
                {
                    message.AttachmentsBlobName.Add(await FileSharingService.Upload(attachmentFilePath));
                }
            }

            logger.Information($"added the following attachments to the message: {string.Join(",", message.AttachmentsBlobName)}");

            // Save to database
            await MessageService.CreateMessage(
                message.RecipientId,
                message.SenderId,
                message.Content,
                message.ParentMessageId,
                message.AttachmentsBlobName);

            // Broadcasts the message to the hub
            await SignalRService.SendMessage(message);

            logger.Information($"Broadcasts the following message to the hub: {message}");
            logger.Information($"Return value: true");

            return true;
        }

        /// <summary>
        /// Saves new team to database and join the hub group of the team
        /// </summary>
        /// <param name="creatorId">Creator user id</param>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team(optional)</param>
        /// <returns>Id of the newly created team on success, null on fail (error will be handled in each service)</returns>
        public async Task<uint?> CreateTeam(string creatorId, string teamName, string teamDescription = "")
        {
            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters creatorId={creatorId}, teamName={teamName}, teamDescription={teamDescription}");

            // Create team and save to database
            uint? teamId = await TeamService.CreateTeam(teamName, teamDescription);

            if (teamId == null)
            {
                logger.Information($"could not create the team");
                logger.Information($"Return value: false");
                return null;
            }

            // Create membership for the creator and save to database
            await TeamService.AddMember(creatorId, (uint)teamId);
            logger.Information($"Added the user identified by {creatorId} to the team identified by {(uint)teamId}");

            // Join the new hub group of the team
            await SignalRService.JoinTeam(teamId.ToString());
            logger.Information($"Joined the hub of the team identified by {teamId}");

            logger.Information($"Return value: true");

            return teamId;
        }

        /// <summary>
        /// Rename A team and notify other clients
        /// </summary>
        /// <param name="teamId">Id of the team to rename</param>
        /// <param name="teamName">The new team name</param>
        /// <returns>True if the team was successfully renamed, false otherwise</returns>
        public async Task<bool>ChangeTeamName(string teamName, uint teamId)
        {
            LogContext.PushProperty("Method", "RenameTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamName={teamName}, teamId={teamId}");

            var result = await TeamService.ChangeTeamName(teamId, teamName);

            var team = await TeamService.GetTeam(teamId);

            await SignalRService.UpdateTeam(team);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Change a specified team's description and notify other clients
        /// </summary>
        /// <param name="teamDescription">New description of the team</param>
        /// <param name="teamId">Id of the team to rename</param>
        /// <returns>True if the team's description was successfully changed, false otherwise</returns>
        public async Task<bool>ChangeTeamDescription(string teamDescription, uint teamId)
        {
            LogContext.PushProperty("Method", "RenameTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamDescription={teamDescription}, teamId={teamId}");

            var result = await TeamService.ChangeTeamDescription(teamId, teamDescription);

            var team = await TeamService.GetTeam(teamId);

            await SignalRService.UpdateTeam(team);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Saves new membership to database and add the user to the hub group of the team
        /// </summary>
        /// <param name="userId">User id to add</param>
        /// <param name="teamId">Id of the team to add the user to</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> InviteUser(string userId, uint teamId)
        {
            LogContext.PushProperty("Method","InviteUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.Information($"userId has been determined invalid");
                logger.Information($"Return value: false");

                return false;
            }

            // Create membership for the user and save to database
            await TeamService.AddMember(userId, teamId);
            logger.Information($"added the user identified by {userId} to the team identified by {teamId}");

            // Add user to the hub group if the user is connected (will be handled in SignalR)
            await SignalRService.AddToTeam(userId, teamId.ToString());
            logger.Information($"Joined the user identified by {userId} to the team identified by {teamId}");

            logger.Information($"Return value: true");
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
            LogContext.PushProperty("Method","ValidateMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters message={message}");

            // Sender / Recipient Id
            if (message == null || string.IsNullOrWhiteSpace(message.SenderId))
            {
                logger.Information($"message has been determined invalid");
                logger.Information($"Return value: false");

                return false;
            }

            // Content
            if (string.IsNullOrWhiteSpace(message.Content))
            {
                logger.Information($"message has been determined invalid");
                logger.Information($"Return value: false");

                return false;
            }

            // Valid
            logger.Information($"Return value: true");

            return true;
        }
        #endregion
    }
}
