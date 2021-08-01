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
        public static SignalRService SignalRService => Singleton<SignalRService>.Instance;

        public static ILogger logger => GlobalLogger.Instance;

        #region Initializers

        /// <summary>
        /// Connects the given user to the teams he is a member of
        /// </summary>
        /// <param name="userId">The user to connect to his teams</param>
        /// <param name="connectionString">(optional)connection string to initialize with</param>
        /// <returns>List of teams the user has membership of, null if none exists</returns>
        public static async Task<IList<Team>> Initialize(string userId, string connectionString = null)
        {
            LogContext.PushProperty("Method","Initialize");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters userId={userId}");

            /** REGISTER USER TO SIGNAL-R HUB **/
            SignalRService.Initialize();
            await SignalRService.OpenConnection(userId);

            /** LOAD TEAMS **/
            IEnumerable<Team> teams = await TeamService.GetAllTeamsByUserId(userId);

            /* EXIT IF NO TEAM **/
            if (teams == null || teams.Count() <= 0)
            {
                logger.Information($"No teams found for the current user");
                logger.Information($"Return value: null");

                return null;
            }

            logger.Information($"Loaded the following teams for the current user: {string.Join(", ", teams)}");

            List<Team> result = new List<Team>();

            /** CONNECT TO EACH SIGNAL-R GROUPS **/
            foreach (Team team in teams)
            {
                await SignalRService.JoinTeam(userId, team.Id.ToString());

                result.Add(team);

                logger.Information($"Connected the current user to the team {team.Id}");
            }

            logger.Information($"Return value: {result}");

            return result;
        }

        #endregion

        #region Message

        /// <summary>
        /// Gets all messages of the team
        /// </summary>
        /// <param name="teamId">Id of the team to load messsages from</param>
        /// <returns>List of messages</returns>
        public static async Task<IEnumerable<Message>> GetMessages(uint channelId)
        {
            LogContext.PushProperty("Method", "LoadMessages");
            LogContext.PushProperty("SourceContext", "MessengerService");

            logger.Information($"Function called with parameter teamId={channelId}");

            var messages = await MessageService.RetrieveMessages(channelId);

            logger.Information($"Return value: {string.Join(", ", messages)}");

            return messages;
        }

        public static async Task<Message> GetMessage(uint messageId)
        {
            LogContext.PushProperty("Method", "LoadMessages");
            LogContext.PushProperty("SourceContext", "MessengerService");

            logger.Information($"Function called with parameter messageId={messageId}");

            var message = await MessageService.GetMessage(messageId);

            logger.Information($"Return value: {message}");

            return message;
        }

        /// <summary>
        /// Saves the message to the database and simultaneously broadcasts to the connected Signal-R hub
        /// </summary>
        /// <param name="message">A complete message object to send</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public static async Task<bool> SendMessage(Message message, uint teamId)
        {
            LogContext.PushProperty("Method","SendMessage");
            LogContext.PushProperty("SourceContext", "MessengerService");

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
            uint? id = await MessageService.CreateMessage(
                message.RecipientId,
                message.SenderId,
                message.Content,
                message.ParentMessageId,
                message.AttachmentsBlobName);

            if (id == null)
            {
                return false;
            }

            message.Id = (uint)id;

            await SignalRService.SendMessage(message, teamId);

            logger.Information($"Broadcasts the following message to the hub: {message}");
            logger.Information($"Return value: true");

            return true;
        }

        /// <summary>
        /// Delete a Message and notify other clients
        /// </summary>
        /// <param name="messageId">The id of the message to delete</param>
        /// <returns>True if the team was successfully deleted, false otherwise</returns>
        public static async Task<bool> DeleteMessage(uint messageId, uint teamId)
        {
            LogContext.PushProperty("Method", "DeleteMessage");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters messageId={messageId}");

            var result = await MessageService.DeleteMessage(messageId);

            var message = await MessageService.GetMessage(messageId);

            var blobFileNames = await MessageService.GetBlobFileNamesOfAttachments(messageId);

            if (blobFileNames != null
                    && blobFileNames?.Count() > 0)
            {
                foreach (var blobFileName in blobFileNames)
                {
                    result &= await FileSharingService.Delete(blobFileName);
                }
            }

            await SignalRService.DeleteMessage(message, teamId);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Change a messages content and notify other clients
        /// </summary>
        /// <param name="messageId">Id of the message to edit</param>
        /// <param name="newContent">New content of the message</param>
        /// <returns>True if the channel was successfully renamed, false otherwise</returns>
        public static async Task<bool> UpdateMessage(uint messageId, string newContent, uint teamId)
        {
            LogContext.PushProperty("Method", "UpdateMessage");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters messageId={messageId}, newContent={newContent}");

            var result = await MessageService.EditMessage(messageId, newContent);

            var message = await MessageService.GetMessage(messageId);

            await SignalRService.UpdateMessage(message, teamId);

            logger.Information($"Return value: {result}");

            return result;
        }

        public static async Task<IEnumerable<Reaction>> GetReactions(uint messageId)
        {
            LogContext.PushProperty("Method", "GetReactions");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters messageId={messageId}");

            var result = await MessageService.RetrieveReactions(messageId);

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
        public static async Task<Reaction> CreateMessageReaction(uint messageId, string userId, uint teamId, string reaction)
        {
            LogContext.PushProperty("Method", "AddReaction");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters messageId={messageId}, userId={userId}, reaction={reaction}");

            var message = await MessageService.GetMessage(messageId);

            if (message == null)
            {
                logger.Information($"Could not retrieve the message from the database");
                return null;
            }

            uint reactionId = await MessageService.AddReaction(messageId, userId, reaction);

            Reaction result = (await MessageService.RetrieveReactions(messageId))
                .Single(r => r.Id == reactionId);

            await SignalRService.UpdateMessageReactions(message, teamId);

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
        public static async Task<Reaction> DeleteMessageReaction(uint messageId, string userId, uint teamId, string reaction)
        {
            LogContext.PushProperty("Method", "RemoveReaction");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters messageId={messageId}, userId={userId}, reaction={reaction}");

            Message message = await MessageService.GetMessage(messageId);
            Reaction userReaction = (await MessageService.RetrieveReactions(messageId))
                .Single(r => r.UserId == userId);

            if (message == null
                || userReaction == null)
            {
                logger.Information($"Could not retrieve the message from the database");
                return null;
            }

            bool isSuccess = await MessageService.RemoveReaction(message.Id, userId, reaction);

            if (isSuccess)
            {
                await SignalRService.UpdateMessageReactions(message, teamId);
            }

            logger.Information($"Return value: {userReaction}");

            return userReaction;
        }

        #endregion

        #region Team

        /// <summary>
        /// Gets all teams the current user has membership of
        /// </summary>
        /// <param name="userId">Current user id</param>
        /// <returns>List of teams</returns>
        public static async Task<IEnumerable<Team>> GetTeams(string userId)
        {
            LogContext.PushProperty("Method", "LoadTeams");
            LogContext.PushProperty("SourceContext", "MessengerService");
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
        public static async Task<Team> GetTeam(uint teamId)
        {
            LogContext.PushProperty("Method", "GetTeam");
            LogContext.PushProperty("SourceContext", "MessengerService");
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
        public static async Task<uint?> CreateTeam(string creatorId, string teamName, string teamDescription = "")
        {
            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", "MessengerService");

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
            await TeamService.AddMember(creatorId, (uint)teamId);
            await TeamService.AddRole("admin", (uint)teamId, "CD5C5C");
            await TeamService.AssignRole("admin", creatorId, (uint)teamId);

            // Grant admin all permissions
            bool grantedAllPermissions = true;

            foreach (var permission in Enum.GetValues(typeof(Permissions)).Cast<Permissions>())
            {
                uint? teamRoleId = await TeamService.GrantPermission(teamId.Value, "admin", permission);

                grantedAllPermissions &= teamRoleId != null;
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

            Team team = await TeamService.GetTeam((uint)teamId);

            if (team == null)
            {
                logger.Information($"could not retrieve the team from the server");
                logger.Information($"Return value: null");

                return null;
            }

            logger.Information($"Created a channel identified by ChannelId={channelId} in the team identified by TeamId={teamId.Value}");

            await SignalRService.CreateTeam(team);
            await SignalRService.JoinTeam(creatorId, team.Id.ToString());

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
        public static async Task<bool> UpdateTeamName(string teamName, uint teamId)
        {
            LogContext.PushProperty("Method", "ChangeTeamName");
            LogContext.PushProperty("SourceContext", "MessengerService");
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
        public static async Task<bool> DeleteTeam(uint teamId)
        {
            LogContext.PushProperty("Method", "DeleteTeam");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters teamId={teamId}");

            Team team = await TeamService.GetTeam(teamId);

            var didDeleteChannels = await ChannelService.RemoveAllChannels(teamId);
            var didDeleteTeamAndMemberships = await TeamService.DeleteTeam(teamId);

            var result = didDeleteTeamAndMemberships && didDeleteChannels;

            await SignalRService.DeleteTeam(team);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Change a specified team's description and notify other clients
        /// </summary>
        /// <param name="teamDescription">New description of the team</param>
        /// <param name="teamId">Id of the team to rename</param>
        /// <returns>True if the team's description was successfully changed, false otherwise</returns>
        public static async Task<bool> UpdateTeamDescription(string teamDescription, uint teamId)
        {
            LogContext.PushProperty("Method", "ChangeTeamDescription");
            LogContext.PushProperty("SourceContext", "MessengerService");
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
        public static async Task<uint?> CreateChannel(string channelName, uint teamId)
        {
            LogContext.PushProperty("Method", "CreateChannel");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters channelName={channelName}, teamId={teamId}");

            var channelId = await ChannelService.CreateChannel(channelName, teamId);

            if (channelId == null)
            {
                logger.Information($"could not create the channel");
                logger.Information($"Return value: false");

                return null;
            }

            var channel = await ChannelService.GetChannel(channelId.Value);

            await SignalRService.CreateChannel(channel);

            logger.Information($"Return value: true");

            return channelId;
        }

        /// <summary>
        /// Remove a specified channel from it's team
        /// </summary>
        /// <param name="channelId">Id of the channel to delete</param>
        /// <returns>An awaitable task</returns>
        public static async Task<Channel> DeleteChannel(uint channelId)
        {
            LogContext.PushProperty("Method", "RemoveChannel");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters channelId={channelId}");

            var result = await ChannelService.RemoveChannel(channelId);

            var channel = await ChannelService.GetChannel(channelId);

            await SignalRService.DeleteChannel(channel);

            logger.Information($"Return value: {result}");

            return channel;
        }

        /// <summary>
        /// Rename A channel and notify other clients
        /// </summary>
        /// <param name="channelId">Id of the channel to rename</param>
        /// <param name="channelName">The new name of the channel</param>
        /// <returns>True if the channel was successfully renamed, false otherwise</returns>
        public static async Task<bool> RenameChannel(string channelName, uint channelId)
        {
            LogContext.PushProperty("Method", "RenameChannel");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters channelName={channelName}, channelId={channelId}");

            var result = await ChannelService.RenameChannel(channelName, channelId);

            var channel = await ChannelService.GetChannel(channelId);

            await SignalRService.UpdateChannel(channel);

            logger.Information($"Return value: {result}");

            return result;
        }


        /// <summary>
        /// Gets all channels from a team
        /// </summary>
        /// <param name="teamId"></param>
        /// <returns>Returns a list with all channels</returns>
        public static async Task<IEnumerable<Channel>> GetChannelsForTeam(uint teamId) 
        {           
            LogContext.PushProperty("Method", "GetChannelsForTeam");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters teamId={teamId}");

            var channels = await TeamService.GetAllChannelsByTeamId(teamId);

            logger.Information($"Return value: {string.Join(", ", channels)}");

            return channels != null ? channels : Enumerable.Empty<Channel>();
        }

        #endregion

        #region Member

        /// <summary>
        /// Gets all users in current Team
        /// </summary>
        /// <param name="teamId">Id of the team to load members from</param>
        /// <returns>List of teams</returns>
        public static async Task<IEnumerable<User>> GetTeamMembers(uint teamId)
        {
            LogContext.PushProperty("Method", "LoadTeamMembers");
            LogContext.PushProperty("SourceContext", "MessengerService");

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
        public static async Task<User> GetUserWithNameId(string username, uint nameId)
        {
            LogContext.PushProperty("Method", "GetUserWithNameId");
            LogContext.PushProperty("SourceContext", "MessengerService");

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
        public static async Task<bool> SendInvitation(string userId, uint teamId)
        {
            LogContext.PushProperty("Method","InviteUser");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            User user = await UserService.GetUser(userId);
            Team team = await TeamService.GetTeam(teamId);

            if (user == null
                || team == null)
            {
                logger.Information($"Invalid User/Team");
                logger.Information($"Return value: false");

                return false;
            }

            // Create membership for the user and save to database
            bool isSuccess = await TeamService.AddMember(userId, teamId);

            if (!isSuccess)
            {
                logger.Information($"Could not save the user to the members list");
                logger.Information($"Return value: false");

                return false;
            }

            // Add user to the hub group if the user is connected (will be handled in SignalR)
            await SignalRService.SendInvitation(user, team);
            
            logger.Information($"Return value: true");
            
            return true;
        }

        /// <summary>
        /// Removes a user from a specific team
        /// </summary>
        /// <param name="userId">User id to add</param>
        /// <param name="teamId">Id of the team to add the user to</param>
        /// <returns>true on success, false on invalid message (error will be handled in each service)</returns>
        public static async Task<bool> RemoveMember(string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "RemoveUser");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            User user = await UserService.GetUser(userId);
            Team team = await TeamService.GetTeam(teamId);

            if (user == null
                || team == null)
            {
                logger.Information($"Invalid User/Team");
                logger.Information($"Return value: false");

                return false;
            }

            // Create membership for the user and save to database
            await TeamService.RemoveMember(userId, teamId);

            await SignalRService.RemoveMember(user, team);

            logger.Information($"Return value: true");

            return true;
        }

        /// Update A user's email
        /// </summary>
        /// <param name="userId">Id of the user whos email should be updated</param>
        /// <param name="newEmail">The new email of the user</param>
        /// <returns>True if the email was successfully updated, false otherwise</returns>
        public static async Task<bool> UpdateUserEmail(string userId, string newEmail)
        {
            LogContext.PushProperty("Method", "UpdateUserEmail");
            LogContext.PushProperty("SourceContext", "MessengerService");
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
        public static async Task<bool> UpdateUserBio(string userId, string newBio)
        {
            LogContext.PushProperty("Method", "UpdateUserBio");
            LogContext.PushProperty("SourceContext", "MessengerService");
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
        public static async Task<bool> UpdateUserPhoto(string userId, string newPhotoURL)
        {
            LogContext.PushProperty("Method", "UpdateUserPhoto");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters userId={userId}, newPhotoURL={newPhotoURL}");

            var result = await UserService.UpdateUserPhoto(userId, newPhotoURL);

            var user = await UserService.GetUser(userId);

            await SignalRService.UpdateUser(user);

            logger.Information($"Return value: {result}");

            return result;
        }

        public static async Task<IEnumerable<TeamRole>> GetRolesList(uint teamId, string userId)
        {
            IEnumerable<TeamRole> roles = await TeamService.GetUsersRoles(teamId, userId);

            if (roles != null && roles.Count() > 0)
            {
                return roles;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Add a role to a team with the specified teamId and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to add</param>
        /// <param name="teamId">The id of the team to add the role to</param>
        /// <param name="colorCode">Hex code of the color</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> CreateTeamRole(string role, uint teamId, string colorCode)
        {
            LogContext.PushProperty("Method", "AddRoleToTeam");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            uint? roleId = await TeamService.AddRole(role, teamId, colorCode);
            TeamRole teamRole = await TeamService.GetRole((uint)roleId);

            if (teamRole == null)
            {
                return false;
            }

            await SignalRService.AddOrUpdateTeamRole(teamRole);

            logger.Information($"Return value: {true}");

            return true;
        }

        public static async Task<bool> UpdateTeamRole(uint roleId, string role, string colorCode)
        {
            LogContext.PushProperty("Method", "AddRoleToTeam");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters roleId={roleId}, role={role}, colorCode={colorCode}");

            bool isSuccess = await TeamService.UpdateRole(roleId, role, colorCode);
            TeamRole teamRole = await TeamService.GetRole(roleId);

            if (!isSuccess || teamRole == null)
            {
                return false;
            }

            await SignalRService.AddOrUpdateTeamRole(teamRole);

            logger.Information($"Return value: {true}");

            return true;
        }

        /// <summary>
        /// Remove a role from a team's available roles and all members roles and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to remove</param>
        /// <param name="teamId">The id of the team to remove the role from</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> DeleteTeamRole(string role, uint teamId)
        {
            LogContext.PushProperty("Method", "RemoveRoleToTeam");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            uint? roleId = await TeamService.RemoveRole(role, teamId);
            TeamRole teamRole = await TeamService.GetRole((uint)roleId);

            if (teamRole == null)
            {
                return false;
            }

            bool isSuccess = true;

            foreach (var user in await TeamService.GetUsersWithRole(teamId, role))
            {
                isSuccess &= await TeamService.UnAssignRole(role, user.Id, teamId);
            }

            if (!isSuccess)
            {
                return false;
            }

            await SignalRService.DeleteTeamRole(teamRole);

            logger.Information($"Return value: {true}");

            return true;
        }

        /// <summary>
        /// Assign a role to a team's member and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to assign to the user</param>
        /// <param name="userId">The id of the user to assign the role to</param>
        /// <param name="teamId">The team to assign a role to a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> AssignUserRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "AssignUserRole");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            User user = await UserService.GetUser(userId);
            Team team = await TeamService.GetTeam(teamId);

            if (user == null
                || team == null)
            {
                return false;
            }

            bool isSuccess = await TeamService.AssignRole(role, userId, teamId);

            if (!isSuccess)
            {
                return false;
            }

            await SignalRService.UpdateMember(user, team);

            logger.Information($"Return value: {true}");

            return true;
        }

        /// <summary>
        /// Unassign a role from a team's member and notify other clients
        /// </summary>
        /// <param name="role">The name of the role to unassign from the user</param>
        /// <param name="userId">The id of the user to unassign the role from</param>
        /// <param name="teamId">The team to unassign a role from a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> UnassignUserRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method", "UnAssignUserRole");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            User user = await UserService.GetUser(userId);
            Team team = await TeamService.GetTeam(teamId);

            if (user == null
                || team == null)
            {
                return false;
            }

            bool isSuccess = await TeamService.UnAssignRole(role, userId, teamId);

            if (!isSuccess)
            {
                return false;
            }

            await SignalRService.UpdateMember(user, team);

            logger.Information($"Return value: {true}");

            return true;
        }

        /// Grant a team's role a specified permissions and notify other clients
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to grant a permission</param>
        /// <param name="permission">The permission to grant a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public static async Task<bool> GrantPermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method", "GrantPermission");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters role={role}, teamId={teamId}, permission={permission}");

            uint? roleId = await TeamService.GrantPermission(teamId, role, permission);

            if (roleId == null)
            {
                return false;
            }

            TeamRole teamRole = await TeamService.GetRole((uint)roleId);

            await SignalRService.AddOrUpdateTeamRole(teamRole);

            logger.Information($"Return value: {true}");

            return true;
        }

        /// <summary>
        /// Revoke a permission from a specified team's role and notify other clients
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to revoke a permission from</param>
        /// <param name="permission">The permission to revoke from a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public static async Task<bool> RevokePermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method", "RevokePermission");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters role={role}, teamId={teamId}, permission={permission}");

            uint? roleId = await TeamService.RevokePermission(teamId, role, permission);

            if (roleId == null)
            {
                return false;
            }

            TeamRole teamRole = await TeamService.GetRole((uint)roleId);

            if (teamRole == null)
            {
                return false;
            }

            await SignalRService.AddOrUpdateTeamRole(teamRole);

            logger.Information($"Return value: {true}");

            return true;
        }

        #endregion

        #region Private Chat

        public static async Task<uint?> StartChat(string userId, string targetUserId)
        {
            LogContext.PushProperty("Method", "StartChat");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters userId={userId}, targetUserNameId={targetUserId}");

            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(targetUserId))
            {
                logger.Information($"Invalid 'UserId's");
                return null;
            }

            uint? chatId = await PrivateChatService.CreatePrivateChat(userId, targetUserId);

            if (chatId == null)
            {
                logger.Information($"Error while starting a new private chat");
                return null;
            }

            await TeamService.AddRole("admin", chatId.Value, "CD5C5C");
            await TeamService.AssignRole("admin", userId, chatId.Value);
            await TeamService.AssignRole("admin", targetUserId, chatId.Value);

            // Grant admin all permissions
            bool grantedAllPermissions = true;

            foreach (var permission in Enum.GetValues(typeof(Permissions)).Cast<Permissions>())
            {
                uint? teamRoleId = await TeamService.GrantPermission(chatId.Value, "admin", permission);

                grantedAllPermissions &= teamRoleId != null;
            }

            uint? channelId = await ChannelService.CreateChannel("main",chatId.Value);

            if (channelId == null)
            {
                logger.Information($"could not create the team's main channel");
                logger.Information($"Return value: false");

                return null;
            }

            Team chat = await TeamService.GetTeam((uint)chatId);

            if (chat == null)
            {
                logger.Information($"could not retrieve the team from the server");
                logger.Information($"Return value: null");

                return null;
            }

            logger.Information($"Created a channel identified by ChannelId={channelId} in the team identified by TeamId={chatId.Value}");

            await SignalRService.CreateTeam(chat);
            await SignalRService.JoinTeam(userId, chat.Id.ToString());

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
        private static bool ValidateMessage(Message message)
        {
            LogContext.PushProperty("Method","ValidateMessage");
            LogContext.PushProperty("SourceContext", "MessengerService");
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
