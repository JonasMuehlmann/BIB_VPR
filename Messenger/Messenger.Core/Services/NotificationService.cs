using System;
using System.Threading.Tasks;
using Serilog.Context;
using Messenger.Core.Models;
using Messenger.Core.Helpers;

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
        public async Task<uint?> SendNotification(string recipientId, string message)
        {
            LogContext.PushProperty("Method","SendNotification");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters recipientId={recipientId}, message={message}");

            // TODO: Create json encoded message from method parameters
            string query = $@"
                                INSERT INTO
                                    Notifications
                                VALUES(
                                    '{recipientId}',
                                    '{message}',
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
        public async Task<uint?> RemoveNotification(uint notificationId)
        {
            LogContext.PushProperty("Method","RemoveNotification");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters notificationId={notificationId}");

            // TODO: Create json encoded message from method parameters
            string query = $@"
                                DELETE FROM
                                    Notifications
                                WHERE
                                    notificationId={notificationId};";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }
    }
}
