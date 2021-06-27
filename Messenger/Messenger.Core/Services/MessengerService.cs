using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
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
        #region Private

        private MessageService MessageService => Singleton<MessageService>.Instance;

        private ChannelService ChannelService => Singleton<ChannelService>.Instance;

        private UserService UserService => Singleton<UserService>.Instance;

        private TeamService TeamService => Singleton<TeamService>.Instance;

        private SignalRService SignalRService => Singleton<SignalRService>.Instance;

        private FileSharingService FileSharingService => Singleton<FileSharingService>.Instance;

        private PrivateChatService PrivateChatService => Singleton<PrivateChatService>.Instance;

        #endregion

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

        public void RegisterListenerForMessageUpdate(EventHandler<Message> onMessageUpdated)
        {
            SignalRService.MessageUpdated += onMessageUpdated;
        }

        public void RegisterListenerForChannelUpdate(EventHandler<Channel> onChannelUpdated)
        {
            SignalRService.ChannelUpdated += onChannelUpdated;
        }

        public void RegisterListenerForUserUpdate(EventHandler<User> onUserUpdated)
        {
            SignalRService.UserUpdated += onUserUpdated;
        }

        #endregion

        #region Message

        /// <summary>
        /// Load all messages of the team
        /// </summary>
        /// <param name="teamId">Id of the team to load messsages from</param>
        /// <returns>List of messages</returns>
        public async Task<IEnumerable<Message>> LoadMessages(uint teamId)
        {
            LogContext.PushProperty("Method", "LoadMessages");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameter teamId={teamId}");

            var messages = await MessageService.RetrieveMessages(teamId);

            logger.Information($"Return value: {string.Join(", ", messages)}");

            return messages;
        }

        /// <summary>
        /// Saves the message to the database and simultaneously broadcasts to the connected Signal-R hub
        /// </summary>
        /// <param name="message">A complete message object to send</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> SendMessage(Message message)
        {
            LogContext.PushProperty("Method","SendMessage");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameter message={message}");

            // Check the validity of the message
            if (!ValidateMessage(message))
            {
                logger.Information($"message object has been determined invalid");
                logger.Information($"Return value: false");

                return false;
            }

            // Upload attachments
            if (message.AttachmentsBlobName != null && message.AttachmentsBlobName.Count > 0)
            {
                foreach (var attachment in message.AttachmentsBlobName)
                {
                    await FileSharingService.Upload(attachment);
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
        /// Delete a Message and notify other clients
        /// </summary>
        /// <param name="messageId">The id of the message to delete</param>
        /// <returns>True if the team was successfully deleted, false otherwise</returns>
        public async Task<bool> DeleteMessage(uint messageId)
        {
            LogContext.PushProperty("Method", "DeleteMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}");

            var result = await MessageService.DeleteMessage(messageId);

            var message = await MessageService.GetMessage(messageId);

            await SignalRService.UpdateMessage(message);

            var blobFileNames = await MessageService.GetBlobFileNamesOfAttachments(messageId);

            foreach (var blobFileName in blobFileNames)
            {
                result &= await FileSharingService.Delete(blobFileName);
            }

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Change a messages content and notify other clients
        /// </summary>
        /// <param name="messageId">Id of the message to edit</param>
        /// <param name="newContent">New content of the message</param>
        /// <returns>True if the channel was successfully renamed, false otherwise</returns>
        public async Task<bool> EditMessage(uint messageId, string newContent)
        {
            LogContext.PushProperty("Method", "EditMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}, newContent={newContent}");

            var result = await MessageService.EditMessage(messageId, newContent);

            var message = await MessageService.GetMessage(messageId);

            await SignalRService.UpdateMessage(message);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	Add a reaction to a message and notify other clients
        /// </summary>
        /// <param name="messageId">The id of the message to add a reaction to</param>
        /// <param name="reaction">The reaction to add to the message</param>
        /// <returns></returns>
        public async Task<uint> AddReaction(uint messageId, string reaction)

        {
            LogContext.PushProperty("Method", "AddReaction");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}, reaction={reaction}");

            var result = await MessageService.AddReaction(messageId, reaction);

            var teamId = (await MessageService.GetMessage(messageId)).RecipientId;

            await SignalRService.UpdateMessageReactions(teamId, messageId);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	Remove a reaction from a message and notify other clients
        /// </summary>
        /// <param name="messageId">The id of the message to remove a reaction from</param>
        /// <param name="reaction">The reaction to remove from the message</param>
        /// <returns>Whetever or not to the reaction was successfully removed</returns>
        public async Task<bool> RemoveReaction(uint messageId, string reaction)
        {
            LogContext.PushProperty("Method", "RemoveReaction");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}, reaction={reaction}");

            var result = await MessageService.RemoveReaction(messageId, reaction);

            var teamId = (await MessageService.GetMessage(messageId)).RecipientId;

            await SignalRService.UpdateMessageReactions(teamId, messageId);

            logger.Information($"Return value: {result}");

            return result;
        }

        public async Task<bool> DownloadAttachment(string blobName, string path = "")
        {
            LogContext.PushProperty("Method", "DownloadAttachment");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters blobName={blobName}, path={path}");

            bool result = await FileSharingService.Download(blobName, path);

            logger.Information($"Return value: {result}");

            return result;
        }

        #endregion

        #region Team

        /// <summary>
        /// Load all teams the current user has membership of
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <returns>List of teams</returns>
        public async Task<IEnumerable<Team>> LoadTeams(string userId)
        {
            LogContext.PushProperty("Method", "LoadTeams");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters userId={userId}");

            var teams = await TeamService.GetAllTeamsByUserId(userId);

            logger.Information($"Return value: {string.Join(", ", teams)}");

            return teams != null ? teams : Enumerable.Empty<Team>();
        }

        /// <summary>
        /// Gets the team with the given team id
        /// </summary>
        /// <param name="teamId">Id of the team to retrieve</param>
        /// <returns>A complete Team object</returns>
        public async Task<Team> GetTeam(uint teamId)
        {
            LogContext.PushProperty("Method", "GetTeam");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");

            var team = await TeamService.GetTeam(teamId);

            logger.Information($"Return value: {team}");

            return team;
        }

        /// <summary>
        /// Saves new team to database, create a main channel and join the hub group of the team
        /// </summary>
        /// <param name="creatorId">Creator user id</param>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team(optional)</param>
        /// <returns>Id of the newly created team on success, null on fail (error will be handled in each service)</returns>
        public async Task<uint?> CreateTeam(string creatorId, string teamName, string teamDescription = "")
        {
            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters creatorId={creatorId}, teamName={teamName}, teamDescription={teamDescription}");

            // Create team and save to database
            uint? teamId = await TeamService.CreateTeam(teamName, teamDescription);

            if (teamId == null)
            {
                logger.Information($"could not create the team");
                logger.Information($"Return value: null");

                return null;
            }

            // Create membership for the creator and save to database, also make him the
            // admin
            await TeamService.AddMember(creatorId, teamId.Value);
            await TeamService.AddRole("admin", teamId.Value);
            await TeamService.AssignRole("admin", creatorId, teamId.Value);

            // Grant admin all permissions
            bool grantedAllPermissions = true;

            foreach (var permission in Enum.GetValues(typeof(Permissions)).Cast<Permissions>())
            {
                grantedAllPermissions &= await TeamService.GrantPermission(teamId.Value, "admin", permission);
            }

            // Create main channel
            logger.Information($"Added the user identified by {creatorId} to the team identified by {(uint)teamId}");

            uint? channelId = await ChannelService.CreateChannel("main", teamId.Value);

            if (channelId == null)
            {
                logger.Information($"could not create the team's main channel");
                logger.Information($"Return value: false");

                return null;
            }

            logger.Information($"Created a channel identified by ChannelId={channelId} in the team identified by TeamId={teamId.Value}");
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
        public async Task<bool> ChangeTeamName(string teamName, uint teamId)
        {
            LogContext.PushProperty("Method", "ChangeTeamName");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters teamName={teamName}, teamId={teamId}");

            var result = await TeamService.ChangeTeamName(teamId, teamName);

            var team = await TeamService.GetTeam(teamId);

            await SignalRService.UpdateTeam(team);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Delete a team alongside it's channels and memberships
        /// </summary>
        /// <param name="teamId">The id of the team to delete</param>
        /// <returns>True if the team was successfully deleted, false otherwise</returns>
        public async Task<bool> DeleteTeam(uint teamId)
        {
            LogContext.PushProperty("Method", "DeleteTeam");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");

            var didDeleteChannels = await ChannelService.RemoveAllChannels(teamId);
            var didDeleteTeamAndMemberships = await TeamService.DeleteTeam(teamId);

            var result = didDeleteTeamAndMemberships && didDeleteChannels;

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Change a specified team's description and notify other clients
        /// </summary>
        /// <param name="teamDescription">New description of the team</param>
        /// <param name="teamId">Id of the team to rename</param>
        /// <returns>True if the team's description was successfully changed, false otherwise</returns>
        public async Task<bool> ChangeTeamDescription(string teamDescription, uint teamId)
        {
            LogContext.PushProperty("Method", "ChangeTeamDescription");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters teamDescription={teamDescription}, teamId={teamId}");

            var result = await TeamService.ChangeTeamDescription(teamId, teamDescription);

            var team = await TeamService.GetTeam(teamId);

            await SignalRService.UpdateTeam(team);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Add a channel to a specified team
        /// </summary>
        /// <param name="teamId">Id of the team to add the channel to</param>
        /// <param name="channelName">Name of the newly created channel</param>
        /// <returns>True if the channel was successfully created, false otherwise</returns>
        public async Task<bool> CreateChannel(string channelName, uint teamId)
        {
            LogContext.PushProperty("Method", "CreateChannel");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters channelName={channelName}, teamId={teamId}");

            var channelId = await ChannelService.CreateChannel(channelName, teamId);

            if (channelId == null)
            {
                logger.Information($"could not create the channel");
                logger.Information($"Return value: false");

                return false;
            }

            var channel = await ChannelService.GetChannel(channelId.Value);

            await SignalRService.UpdateChannel(channel);

            logger.Information($"Return value: true");

            return true;
        }

        /// <summary>
        /// Remove a specified channel from it's team
        /// </summary>
        /// <param name="channelId">Id of the channel to delete</param>
        /// <returns>An awaitable task</returns>
        public async Task<bool> RemoveChannel(uint channelId)
        {
            LogContext.PushProperty("Method", "RemoveChannel");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters channelId={channelId}");

            var result = await ChannelService.RemoveChannel(channelId);

            var channel = await ChannelService.GetChannel(channelId);

            await SignalRService.UpdateChannel(channel);

            logger.Information($"Return value: {result}");

            return true;
        }

        /// <summary>
        /// Rename A channel and notify other clients
        /// </summary>
        /// <param name="channelId">Id of the channel to rename</param>
        /// <param name="channelName">The new name of the channel</param>
        /// <returns>True if the channel was successfully renamed, false otherwise</returns>
        public async Task<bool> RenameChannel(string channelName, uint channelId)
        {
            LogContext.PushProperty("Method", "RenameChannel");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters channelName={channelName}, channelId={channelId}");

            var result = await ChannelService.RenameChannel(channelName, channelId);

            var channel = await ChannelService.GetChannel(channelId);

            await SignalRService.UpdateChannel(channel);

            logger.Information($"Return value: {result}");

            return result;
        }

        #endregion

        #region Member

        /// <summary>
        /// Load all users in current Team
        /// </summary>
        /// <param name="teamId">Id of the team to load members from</param>
        /// <returns>List of teams</returns>
        public async Task<IEnumerable<User>> LoadTeamMembers(uint teamId)
        {
            LogContext.PushProperty("Method", "LoadTeamMembers");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters teamId={teamId}");

            var members = await TeamService.GetAllMembers(teamId);

            logger.Information($"Return value: {members}");

            return members != null ? members : Enumerable.Empty<User>();
        }

        /// <summary>
        /// Returns a user by username and nameId
        /// </summary>
        /// <param name="username">DisplayName of the user</param>
        /// <param name="nameId">NameId of the user</param>
        /// <returns>A complete User object on success, null if the user was not found</returns>
        public async Task<User> GetUserWithNameId(string username, uint nameId)
        {
            LogContext.PushProperty("Method", "GetUserWithNameId");
            LogContext.PushProperty("SourceContext", GetType().Name);

            logger.Information($"Function called with parameters username={username}, nameId={nameId}");

            var user = await UserService.GetUser(username, nameId);

            logger.Information($"Return value: {user}");

            return user;
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
            LogContext.PushProperty("SourceContext", GetType().Name);
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
        /// Removes a user from a specific team
        /// </summary>
        /// <param name="userId">User id to add</param>
        /// <param name="teamId">Id of the team to add the user to</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public async Task<bool> RemoveUser(string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "RemoveUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            if (string.IsNullOrWhiteSpace(userId))
            {
                logger.Information($"userId has been determined invalid");
                logger.Information($"Return value: false");

                return false;
            }

            // Create membership for the user and save to database
            await TeamService.RemoveMember(userId, teamId);
            logger.Information($"added the user identified by {userId} to the team identified by {teamId}");

            logger.Information($"Return value: true");
            return true;
        }

        /// Update A user's email
        /// </summary>
        /// <param name="userId">Id of the user whos email should be updated</param>
        /// <param name="newEmail">The new email of the user</param>
        /// <returns>True if the email was successfully updated, false otherwise</returns>
        public async Task<bool> UpdateUserEmail(string userId, string newEmail)
        {
            LogContext.PushProperty("Method", "UpdateUserEmail");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, newEmail={newEmail}");

            var result = await UserService.UpdateUserMail(userId, newEmail);

            var user = await UserService.GetUser(userId);

            await SignalRService.UpdateUser(user);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Update A user's bio
        /// </summary>
        /// <param name="userId">Id of the user whos Bio should be updated</param>
        /// <param name="newBio">The new bio of the user</param>
        /// <returns>True if the bio was successfully updated, false otherwise</returns>
        public async Task<bool> UpdateUserBio(string userId, string newBio)
        {
            LogContext.PushProperty("Method", "UpdateUserBio");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, newBio={newBio}");

            var result = await UserService.UpdateUserBio(userId, newBio);

            var user = await UserService.GetUser(userId);

            await SignalRService.UpdateUser(user);


            logger.Information($"Return value: {result}");

            return result;
        }

        /// Update A user's Photo
        /// </summary>
        /// <param name="userId">Id of the user whos photo should be updated</param>
        /// <param name="newPhotoURL">The new photo of the user</param>
        /// <returns>True if the photo was successfully updated, false otherwise</returns>
        public async Task<bool> UpdateUserPhoto(string userId, string newPhotoURL)
        {
            LogContext.PushProperty("Method", "UpdateUserPhoto");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, newPhotoURL={newPhotoURL}");

            var result = await UserService.UpdateUserPhoto(userId, newPhotoURL);

            var user = await UserService.GetUser(userId);

            await SignalRService.UpdateUser(user);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Add a role to a team with the specified teamId and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to add</param>
        /// <param name="teamId">The id of the team to add the role to</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AddRoleToTeam(string role, uint teamId)
        {
            LogContext.PushProperty("Method", "AddRoleToTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            var result = await TeamService.AddRole(role, teamId);

            await SignalRService.UpdateTeamRoles(teamId);


            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Remove a role from a team's available roles and all members roles and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to remove</param>
        /// <param name="teamId">The id of the team to remove the role from</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RemoveTeamRole(string role, uint teamId)
        {
            LogContext.PushProperty("Method", "RemoveRoleToTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            var result = await TeamService.RemoveRole(role, teamId);

            foreach (var user in await TeamService.GetUsersWithRole(teamId, role))
            {
                result &= await TeamService.UnAssignRole(role, user.Id, teamId);
            }

            await SignalRService.UpdateTeamRoles(teamId);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Assign a role to a team's member and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to assign to the user</param>
        /// <param name="userId">The id of the user to assign the role to</param>
        /// <param name="teamId">The team to assign a role to a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AssignUserRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "AssignUserRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            var result = await TeamService.AssignRole(role, userId, teamId);

            var user = await UserService.GetUser(userId);

            await SignalRService.UpdateUser(user);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Unassign a role from a team's member and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to unassign from the user</param>
        /// <param name="userId">The id of the user to unassign the role from</param>
        /// <param name="teamId">The team to unassign a role from a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UnAssignUserRloe(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "UnAssignUserRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            var result = await TeamService.UnAssignRole(role, userId, teamId);

            var user = await UserService.GetUser(userId);

            await SignalRService.UpdateUser(user);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	Add a reaction to a message and notify other clients
        /// </summary>
        /// <param name="messageId">The id of the message to add a reaction to</param>
        /// <param name="userId">The id of the user making the reaction</param>
        /// <param name="reaction">The reaction to add to the message</param>
        /// <returns></returns>
        public async Task<uint> AddReaction(uint messageId, string userId, string reaction)
        {
            LogContext.PushProperty("Method", "AddReaction");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}, userId={userId}, reaction={reaction}");

            var result = await MessageService.AddReaction(messageId, userId, reaction);

            var teamId = (await MessageService.GetMessage(messageId)).RecipientId;

            await SignalRService.UpdateMessageReactions(teamId, messageId);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	Remove a reaction from a message and notify other clients
        /// </summary>
        /// <param name="messageId">The id of the message to remove a reaction from</param>
        /// <param name="userId">The id of the user whose reaction to remove</param>
        /// <param name="reaction">The reaction to remove from the message</param>
        /// <returns>Whetever or not to the reaction was successfully removed</returns>
        public async Task<bool> RemoveReaction(uint messageId, string userId, string reaction)
        {
            LogContext.PushProperty("Method","RemoveReaction");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}, userId={userId}, reaction={reaction}");

            var result = await MessageService.RemoveReaction(messageId, userId, reaction);

            var teamId = (await MessageService.GetMessage(messageId)).RecipientId;

            await SignalRService.UpdateMessageReactions(teamId, messageId);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// Grant a team's role a specified permissions and notify other clients
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to grant a permission</param>
        /// <param name="permission">The permission to grant a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public async Task<bool> GrantPermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method", "GrantPermission");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, teamId={teamId}, permission={permission}");

            var result = await TeamService.GrantPermission(teamId, role, permission);

            await SignalRService.UpdateRolePermission(teamId);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Revoke a permission from a specified team's role and notify other clients
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to revoke a permission from</param>
        /// <param name="permission">The permission to revoke from a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public async Task<bool> RevokePermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method", "RevokePermission");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, teamId={teamId}, permission={permission}");

            var result = await TeamService.RevokePermission(teamId, role, permission);

            await SignalRService.UpdateRolePermission(teamId);

            logger.Information($"Return value: {result}");

            return result;
        }

        #endregion

        #region Chat

        public async Task<uint?> StartChat(string userId, string targetUserId)
        {
            LogContext.PushProperty("Method", "StartChat");
            LogContext.PushProperty("SourceContext", GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, targetUserNameId={targetUserId}");

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(targetUserId))
            {
                logger.Information($"Invalid 'UserId's");
                return null;
            }

            var chatId = await PrivateChatService.CreatePrivateChat(userId, targetUserId);

            if (chatId == null)
            {
                logger.Information($"Error while starting a new private chat");
                return null;
            }

            await SignalRService.JoinTeam(chatId.ToString());

            logger.Information($"Return value: {chatId}");

            return chatId;
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
