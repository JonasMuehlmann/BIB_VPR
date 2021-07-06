using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Messenger.Core.Models;

namespace Messenger.Core.Helpers
{
    /// <summary>
    ///	Contains static methods to create notification messages from minimal information
    /// </summary>
    public static class NotificationMessageBuilder
    {
        /// <summary>
        /// Encode information about the receiving of a message in a subscribed channel
        /// Sets the following fields in the JObject:
        /// notificationType,
        /// notificationSource,
        /// senderName,
        /// channelName,
        /// teamName
        /// </summary>
        /// <param name="messageId">The id of the received message</param>
        /// <returns>A JObject containing the necessary information</returns>
        public static async Task<JObject> MakeMessageInSubscribedChannelNotificationMessage(uint messageId)
        {
            var senderNameQuery = $@"
                                        SELECT
                                            UserName
                                        FROM
                                            Users
                                        LEFT JOIN Messages ON
                                            senderId = userId
                                        WHERE
                                            messageId = {messageId};
                ";

            var channelNameQuery = $@"
                                        SELECT
                                            channelName
                                        FROM
                                            Channels
                                        LEFT JOIN Messages ON
                                            channelId = recipientId
                                        WHERE
                                            messageId = {messageId};
                ";

            var teamNameQuery = $@"
                                    SELECT
                                        t.teamName
                                    FROM
                                        Teams t
                                        Channels c
                                    LEFT JOIN Messages ON
                                        c.channelId = recipientId
                                    WHERE
                                        messageId = {messageId}
                                        AND
                                        c.teamId = t.teamId;
                ";

            var senderName  = await SqlHelpers.ExecuteScalarAsync(senderNameQuery,  Convert.ToString);
            var channelName = await SqlHelpers.ExecuteScalarAsync(channelNameQuery, Convert.ToString);
            var teamName    = await SqlHelpers.ExecuteScalarAsync(teamNameQuery,    Convert.ToString);

            return new JObject
            {
                {"notificationType",   NotificationType.MessageInSubscribedChannel.ToString()},
                {"notificationSource", NotificationSource.Channel.ToString()},
                {"senderName",         senderName},
                {"channelName",        channelName},
                {"teamName",           teamName},
            };
        }

        /// <summary>
        /// Encode information about the receiving of a message in a subscribed team
        /// Sets the following fields in the JObject:
        /// notificationType,
        /// notificationSource,
        /// channelName,
        /// senderName,
        /// teamName
        /// </summary>
        /// <param name="messageId">The id of the received message</param>
        /// <returns>A JObject containing the necessary information</returns>
        public static async Task<JObject> MakeMessageInSubscribedTeamNotificationMessage(uint messageId)
        {
            return await MakeMessageInSubscribedChannelNotificationMessage(messageId);
        }

        /// <summary>
        /// Encode information about the receiving of a message in a private chat
        /// Sets the following fields in the JObject:
        /// notificationType,
        /// notificationSource,
        /// partnerName
        /// </summary>
        /// <param name="messageId">The id of the received message</param>
        /// <returns>A JObject containing the necessary information</returns>
        public static async Task<JObject> MakeMessageInPrivateChatNotificationMessage(uint messageId)
        {
            var partnerNameQuery = $@"
                                    SELECT
                                        t.teamName
                                    FROM
                                        Teams t
                                        Channels c
                                    LEFT JOIN Messages ON
                                        c.channelId = recipientId
                                    WHERE
                                        messageId = {messageId}
                                        AND
                                        c.teamId = t.teamId;
                ";

            var partnerName = await SqlHelpers.ExecuteScalarAsync(partnerNameQuery,    Convert.ToString);

            return new JObject
            {
                {"notificationType",   NotificationType.MessageInPrivateChat.ToString()},
                {"notificationSource", NotificationSource.PrivateChat.ToString()},
                {"partnerName",        partnerName},
            };
        }

        /// <summary>
        /// Encode information about the receiving of an invitation to a team
        /// Sets the following fields in the JObject:
        /// notificationType,
        /// notificationSource,
        /// teamName
        /// </summary>
        /// <param name="teamId">The id of the team the user got invited to</param>
        /// <returns>A JObject containing the necessary information</returns>
        public static async Task<JObject> MakeInvitedToTeamNotificationMessage(uint teamId)
        {
            var teamNameQuery = $@"
                                    SELECT
                                        teamName
                                    FROM
                                        Teams
                                    WHERE
                                        teamId = {teamId};
                ";

            var teamName    = await SqlHelpers.ExecuteScalarAsync(teamNameQuery,    Convert.ToString);

            return new JObject
            {
                {"notificationType",   NotificationType.InvitedToTeam.ToString()},
                {"notificationSource", NotificationSource.Team.ToString()},
                {"teamName",           teamName},
            };
        }
        /// <summary>
        /// Encode information about the removal from a team
        /// Sets the following fields in the JObject:
        /// notificationType,
        /// notificationSource,
        /// teamName
        /// </summary>
        /// <param name="teamId">The id of the team the user got removed from</param>
        /// <returns>A JObject containing the necessary information</returns>
        public static async Task<JObject> MakeRemovedFromTeamNotificationMessage(uint teamId)
        {
            var teamNameQuery = $@"
                                    SELECT
                                        teamName
                                    FROM
                                        Teams
                                    WHERE
                                        teamId = {teamId};
                ";

            var teamName    = await SqlHelpers.ExecuteScalarAsync(teamNameQuery,    Convert.ToString);

            return new JObject
            {
                {"notificationType",   NotificationType.RemovedFromTeam.ToString()},
                {"notificationSource", NotificationSource.Team.ToString()},
                {"teamName",           teamName},
            };
        }
        /// <summary>
        /// Encode information about the receiving of a reaction to a user's message
        /// Sets the following fields in the JObject:
        /// notificationType,
        /// notificationSource,
        /// channelName,
        /// teamName,
        /// reactorName,
        /// reaction
        /// </summary>
        /// <param name="teamId">The id of the team the user got removed from</param>
        /// <returns>A JObject containing the necessary information</returns>
        public static async Task<JObject> MakeReactionToMessageNotificationMessage(uint reactionId)
        {
            var channelNameQuery = $@"
                                        SELECT
                                            channelName
                                        FROM
                                            Channels
                                        LEFT JOIN Messages m ON
                                            channelId = recipientId
                                        LEFT JOIN Reactions r ON
                                            r.messageId = m.messageId
                                        WHERE
                                            reactionId = {reactionId};
                ";

            var teamNameQuery = $@"
                                    SELECT
                                        t.teamName
                                    FROM
                                        Teams t
                                        Channels c
                                    LEFT JOIN Messages m ON
                                        c.channelId = recipientId
                                    LEFT JOIN Reactions r ON
                                        r.messageId = m.messageId
                                    WHERE
                                        reactionId = {reactionId};
                                        AND
                                        c.teamId = t.teamId;
                ";

            var reactorNameQuery = $@"
                                    SELECT
                                        reaction
                                    FROM
                                        Reactions
                                    WHERE reactionId = {reactionId};
                ";

            var reactionQuery = $@"
                                    SELECT
                                        userName
                                    FROM
                                        Users u
                                    LEFT JOIN Reactions r ON
                                        r.userId = u.userId
                                    WHERE reactionId = {reactionId};
                ";

            var channelName = await SqlHelpers.ExecuteScalarAsync(channelNameQuery, Convert.ToString);
            var teamName    = await SqlHelpers.ExecuteScalarAsync(teamNameQuery,    Convert.ToString);
            var reactorName = await SqlHelpers.ExecuteScalarAsync(reactorNameQuery, Convert.ToString);
            var reaction    = await SqlHelpers.ExecuteScalarAsync(reactionQuery,    Convert.ToString);

            return new JObject
            {
                {"notificationType",   NotificationType.ReactionToMessage.ToString()},
                {"notificationSource", NotificationSource.Channel.ToString()},
                {"teamName",           teamName},
                {"channelName",        channelName},
                {"reactorName",        reactorName},
                {"reaction",           reaction},
            };
        }

        /// <summary>
        /// Encode information about the (in)direct mentioning of a  user
        /// Sets the following fields in the JObject:
        /// notificationType,
        /// notificationSource,
        /// channelName,
        /// teamName,
        /// mentioneerName,
        /// mentionTarget
        /// </summary>
        /// <param name="teamId">The id of the team the user got removed from</param>
        /// <returns>A JObject containing the necessary information</returns>
        public static async Task<JObject> MakeUserMentionedNotificationMessage(uint mentionId)
        {
            var channelNameQuery = $@"
                                        SELECT
                                            channelName
                                        FROM
                                            Channels
                                        LEFT JOIN Messages m ON
                                            channelId = recipientId
                                        LEFT JOIN Reactions r ON
                                            r.messageId = m.messageId
                                        WHERE
                                            reactionId = {reactionId};
                ";

            var teamNameQuery = $@"
                                    SELECT
                                        t.teamName
                                    FROM
                                        Teams t
                                        Channels c
                                    LEFT JOIN Messages m ON
                                        c.channelId = recipientId
                                    LEFT JOIN Reactions r ON
                                        r.messageId = m.messageId
                                    WHERE
                                        reactionId = {reactionId};
                                        AND
                                        c.teamId = t.teamId;
                ";

            var channelName    = await SqlHelpers.ExecuteScalarAsync(channelNameQuery,    Convert.ToString);
            var teamName       = await SqlHelpers.ExecuteScalarAsync(teamNameQuery,       Convert.ToString);
            var mentioneerName = await SqlHelpers.ExecuteScalarAsync(mentioneerNameQuery, Convert.ToString);
            var mentionTarget  = await SqlHelpers.ExecuteScalarAsync(mentionTargetName,   Convert.ToString);

            return new JObject
            {
                {"notificationType",   NotificationType.UserMentioned.ToString()},
                {"notificationSource", NotificationSource.Channel.ToString()},
                {"teamName",           teamName},
                {"channelName",        channelName},
                {"mentioneerName",     mentioneerName},
                {"mentionTarget",      mentionTarget},
            };
        }
    }
}
