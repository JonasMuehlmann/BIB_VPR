using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Serilog.Context;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Messenger.Core.Services
{
    public class NotificationService : AzureServiceBase
    {
        /// <summary>
        /// Send a notification to a specified user
        /// </summary>
        /// <param name="recipientId">
        /// The id of the user who is to recieve the notification
        /// </param>
        /// <param name="message">The message the notification holds</param>
        /// <returns>The id of the notification on success, null otherwise</returns>
        public static async Task<uint?> SendNotification(string recipientId, JObject message)
        {
            LogContext.PushProperty("Method","SendNotification");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters recipientId={recipientId}, message={message}");

            string query = $@"
                                INSERT INTO
                                    Notifications
                                VALUES(
                                    '{recipientId}',
                                    '{JsonConvert.SerializeObject(message)}',
                                    GETDATE()
                                );

                                SELECT SCOPE_IDENTITY();
                            ";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        /// <summary>
        /// Remove a notification from the db
        /// </summary>
        /// <param name="notificationId">The id of the notification to remove</param>
        /// <returns>True on success, false on failure</returns>
        public static async Task<bool> RemoveNotification(uint notificationId)
        {
            LogContext.PushProperty("Method","RemoveNotification");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters notificationId={notificationId}");

            string query = $@"
                                DELETE FROM
                                    Notifications
                                WHERE
                                    Id={notificationId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Retrieve all notifications a user currently has
        /// </summary>
        /// <param name="userId">The id of the user to retrieve notifications from</param>
        /// <returns>An enumerable of notification objects</returns>
        public static async Task<IEnumerable<Notification>> RetrieveNotifications(string userId)
        {
            LogContext.PushProperty("Method","RetrieveNotifications");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters userId={userId}");

            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Notifications
                                WHERE
                                    recipientId='{userId}'";

            return await SqlHelpers.MapToList(Mapper.NotificationFromDataRow, query);
        }

        /// <summary>
        /// Add a notification mute for a specified user
        /// </summary>
        /// <param name="notificationType">The type of notification event to mute</param>
        /// <param name="notificationSourceType">
        /// The type of the notification source to muted
        /// </param>
        /// <param name="notificationSourceValue">
        /// The name of the notification source to muted
        /// </param>
        /// <param name="userId">The id of the user muting the notification</param>
        /// <param name="senderId">
        /// The id of the sender of notifications to be muted
        /// </param>
        /// <returns>The id of the Notification mute on success, null otherwise</returns>
        public static async Task<uint?> AddMute(NotificationType? notificationType, NotificationSource? notificationSourceType, string notificationSourceValue, string userId, string senderId = null)
        {
            LogContext.PushProperty("Method","AddMute");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters notificationType={notificationType.ToString()}, notificationSourceType={notificationSourceType.ToString()}, notificationSourceValue={notificationSourceValue}, userId={userId}");

            var senderIdQueryFragment                = senderId                is null ? "NULL" : $"'{senderId}'";
            var notificationTypeQueryFragment        = notificationType        is null ? "NULL" : $"'{notificationType}'";
            var notificationSourceTypeQueryFragment  = notificationSourceType  is null ? "NULL" : $"'{notificationSourceType}'";
            var notificationSourceValueQueryFragment = notificationSourceValue is null ? "NULL" : $"'{notificationSourceValue}'";

            string query = $@"
                                INSERT INTO
                                    NotificationMutes
                                VALUES(
                                         {notificationTypeQueryFragment.ToString()},
                                         {notificationSourceTypeQueryFragment.ToString()},
                                         {notificationSourceValueQueryFragment.ToString()},
                                        '{userId}',
                                         {senderIdQueryFragment}
                                      );

                                SELECT SCOPE_IDENTITY();
                ";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        /// <summary>
        /// Remove a notification mute
        /// </summary>
        /// <param name="muteId">The id of the mute to remove</param>
        /// </param>
        /// <returns>True on success, false otherwise</returns>
        public static async Task<bool> RemoveMute(uint muteId)
        {
            LogContext.PushProperty("Method","RemoveMute");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters muteId={muteId}");

            string query = $@"
                                DELETE FROM
                                    NotificationMutes
                                WHERE
                                    Id = {muteId};
                ";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Get A user's notification mutes
        /// </summary>
        /// <param name="userId">The id of the user to get mutes for</param>
        /// </param>
        /// <returns>An IList of NotificationMute objects representing the usera's mutes</returns>
        public static async Task<IList<NotificationMute>> GetUsersMutes(string userId)
        {
            LogContext.PushProperty("Method","GetUsersMutes");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters userId={userId}");

            string query = $@"
                                SELECT
                                    *
                                FROM
                                    NotificationMutes
                                WHERE
                                    UserId = '{userId}';
                ";

            return await SqlHelpers.MapToList(Mapper.NotificationMuteFromDataRow, query);
        }

        /// <summary>
        /// Check whether a notification can be sent to a user
        /// </summary>
        /// <param name="userId">The id of the notification receiver</param>
        /// </param>
        /// <returns>True if the notification can be sent, false otherwise</returns>
        public static async Task<bool> CanSendNotification(JObject message, string userId)
        {
            LogContext.PushProperty("Method","CanSendNotification");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters message={message}, userId={userId}");

            NotificationType notificationType         = message["notificationType"].ToObject<NotificationType>();
            NotificationSource notificationSourceType = message["notificationSource"].ToObject<NotificationSource>();

            string notificationSourceValue = null;
            string senderId = null;
            uint messageId;

            switch (notificationType)
            {
                case NotificationType.UserMentioned:
                    var mentionId           = message["mentionId"].ToObject<uint>();
                    senderId                = (await MentionService.RetrieveMention(mentionId)).MentionerId;
                    notificationSourceValue = message["channelId"].ToObject<string>();
                    break;

                case NotificationType.MessageInSubscribedChannel:
                    messageId               = message["messageId"].ToObject<uint>();
                    senderId                = (await MessageService.GetMessage(messageId)).SenderId;
                    notificationSourceValue = message["channelId"].ToObject<string>();
                    break;

                case NotificationType.MessageInSubscribedTeam:
                    messageId               = message["messageId"].ToObject<uint>();
                    senderId                = (await MessageService.GetMessage(messageId)).SenderId;
                    notificationSourceValue = message["teamId"].ToObject<string>();
                    break;

                case NotificationType.MessageInPrivateChat:
                    messageId               = message["messageId"].ToObject<uint>();
                    senderId                = (await MessageService.GetMessage(messageId)).SenderId;
                    notificationSourceValue = message["channelId"].ToObject<string>();
                    break;

                case NotificationType.ReactionToMessage:
                    var reactionId = message["reactionId"].ToObject<uint>();
                    senderId       = (await MessageService.GetReaction(reactionId)).UserId;
                    break;

                case NotificationType.InvitedToTeam:
                    notificationSourceValue = message["teamId"].ToObject<string>();
                    break;

                case NotificationType.RemovedFromTeam:
                    notificationSourceValue = message["teamId"].ToObject<string>();
                    break;
            }

            var senderIdQueryFragment = senderId is null ? "NULL" : $"'{senderId}'";
            var notificationSourceValueQueryFragment = notificationSourceValue is null ? "NULL" : $"'{notificationSourceValue}'";

            var query = $@"
                            SELECT
                                COUNT(*)
                            FROM
                                NotificationMutes
                            WHERE
                                NotificationType = '{notificationType}'
                                AND
                                NotificationSourceType = '{notificationSourceType}'
                                AND
                                NotificationSourceValue = '{notificationSourceValue}'
                                AND
                                UserId = '{userId}'
                                AND
                                SenderId = {senderIdQueryFragment};
                ";

            return !(await SqlHelpers.ExecuteScalarAsync(query, Convert.ToBoolean));
        }
    }
}
