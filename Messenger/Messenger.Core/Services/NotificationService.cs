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
        /// <returns>The id of the Notification mute on success, null otherwise</returns>
        public static async Task<uint?> AddMute(NotificationType notificationType, NotificationSource notificationSourceType, string notificationSourceValue, string userId)
        {
            LogContext.PushProperty("Method","AddMute");
            LogContext.PushProperty("SourceContext", "NotificationService");

            logger.Information($"Function called with parameters notificationType={notificationType.ToString()}, notificationSourceType={notificationSourceType.ToString()}, notificationSourceValue={notificationSourceValue}, userId={userId}");

            string query = $@"
                                INSERT INTO
                                    NotificationMutes
                                VALUES(
                                        NotificationType = '{notificationType.ToString()}',
                                        NotificationSourceType = '{notificationSourceType.ToString()}',
                                        NotificationSourceValue = '{notificationSourceValue.ToString()}',
                                        UserId = '{userId}'
                                      );

                                SELECT SCOPE_IDENTITY();
                ";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }
    }
}
