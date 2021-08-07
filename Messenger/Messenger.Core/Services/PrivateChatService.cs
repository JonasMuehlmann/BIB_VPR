using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Context;


namespace Messenger.Core.Services
{
    public class PrivateChatService : TeamService
    {
        /// <summary>
        /// Create a private chat between the users identified by myUserId and otherUserId.
        /// </summary>
        /// <param name="myUserId">user-Id of the Creator</param>
        /// <param name="otherUserId">user-Id of the other Person</param>
        /// <returns>The teamId of the created Team</returns>
        public static async Task<uint?> CreatePrivateChat(string myUserId, string otherUserId)
        {
            LogContext.PushProperty("Method",        "CreatePrivateChat");
            LogContext.PushProperty("SourceContext", "PrivateChatService");
            logger.Information($"Function called with parameters myUserId={myUserId}, otherUserId={otherUserId}");

            uint? teamID;

            // Add myself and other user as members
            string query = $@"
                                INSERT INTO
                                    Teams
                                VALUES(
                                    '', NULL, GETDATE()
                                );

                            SELECT CAST(SCOPE_IDENTITY() AS INT)";

            var team = await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);

            teamID = SqlHelpers.TryConvertDbValue(team, Convert.ToUInt32);

            logger.Information($"teamID has been determined as {teamID}");

            var createEmptyRoleQuery = $"INSERT INTO Team_roles VALUES('', {teamID});";
            // FIX: If AddMember() fails, we just created a dangling Team_Roles
            // entry

            logger.Information($"Result value: {await SqlHelpers.NonQueryAsync(createEmptyRoleQuery)}");

            var success1 = await AddMemberImpl(myUserId,    teamID.Value);
            var success2 = await AddMemberImpl(otherUserId, teamID.Value);

            if (!(success1 && success2))
            {
                logger.Information("Could not add one or both users(s) to the team");
                logger.Information($"Return value: null");

                return null;
            }

            logger.Information($"Return value: {teamID}");

            return teamID;
        }


        /// <summary>
        /// Lists all private chats of a specified user
        /// </summary>
        /// <param name="userId">the id of a user to retrieve private chats of</param>
        /// <returns>An enumerable of Team objects</returns>
        public static async Task<IEnumerable<Team>> GetAllPrivateChatsFromUser(string userId)
        {
            LogContext.PushProperty("Method",        "GetAllPrivateChatsFromUser");
            LogContext.PushProperty("SourceContext", "PrivateChatService");
            logger.Information($"Function called with parameters userId={userId}");

            return (await GetAllTeamsByUserId(userId)).Where(team => team.Name == "");
        }


        /// <summary>
        /// In a private chat, retrieve the conversation partner's user id
        /// </summary>
        /// <param name="teamId">the id of the team belonging to the private chat</param>
        /// <returns>The user id of the conversation partner</returns>
        public static async Task<string> GetPartner(uint teamId)
        {
            LogContext.PushProperty("Method",        "GetPartner");
            LogContext.PushProperty("SourceContext", "SqlHelpers");
            logger.Information($"Function called with parameters teamId={teamId}");

            // NOTE: Private Chats currently only support 1 Members
            string query = @"
                            SELECT
                                UserId
                            FROM
                                Memberships m LEFT JOIN Teams t
                                ON m.TeamId = t.TeamId
                            WHERE
                                t.TeamId != {teamId}
                            AND
                                t.TeamName='';";

            var otherUser = await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);

            return SqlHelpers.TryConvertDbValue(otherUser, Convert.ToString);
        }


        #region SignalREnabled
        public static async Task<uint?> StartChat(string userId, string targetUserId)
        {
            LogContext.PushProperty("Method",        "StartChat");
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
            await TeamService.AssignRole("admin", userId,       chatId.Value);
            await TeamService.AssignRole("admin", targetUserId, chatId.Value);

            // Grant admin all permissions
            bool grantedAllPermissions = true;

            foreach (var permission in Enum.GetValues(typeof(Permissions)).Cast<Permissions>())
            {
                grantedAllPermissions &= await TeamService.GrantPermission(chatId.Value, "admin", permission);
            }

            uint? channelId = await ChannelService.CreateChannel("main", chatId.Value);

            if (channelId == null)
            {
                logger.Information($"could not create the team's main channel");
                logger.Information($"Return value: false");

                return null;
            }

            Team chat = await TeamService.GetTeam((uint) chatId);

            if (chat == null)
            {
                logger.Information($"could not retrieve the team from the server");
                logger.Information($"Return value: null");

                return null;
            }

            logger.Information($"Created a channel identified by ChannelId={channelId} in the team identified by TeamId={chatId.Value}"
                              );

            await SignalRService.CreateTeam(chat);
            await SignalRService.JoinTeam(userId, chat.Id.ToString());

            logger.Information($"Return value: {chatId}");

            return chatId;
        }

        #endregion SignalREnabled
    }
}
