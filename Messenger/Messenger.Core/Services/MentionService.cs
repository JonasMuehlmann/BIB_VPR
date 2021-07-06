using System;
using System.Threading.Tasks;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public class MentionService : AzureServiceBase
    {
        /// <summary>
        /// Create a mention to the entity of type target identified by id
        /// </summary>
        /// <param name="target">The type of the entity to mention</param>
        /// <param name="id">The id of the entity to mention</param>
        /// <returns>The created mention id on success, null on failure</returns>
        public async Task<uint?> CreateMention<T>(MentionTarget target, T id)
        {
            LogContext.PushProperty("Method","CreateMention");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters target={target.ToString()}, id={id}");

            if (typeof(T) == typeof(string) || typeof(T) == typeof(uint))
            {
                var query = $@"
                                INSERT INTO
                                    mentions
                                VALUES(
                                    '{target.ToString()}',
                                    '{id}'
                                      );

                                SELECT IDENTITY_INSERT();
                    ";

                return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
            }
            else
            {
               throw new ArgumentException("id can only be string or uint");
            }
        }

        /// <summary>
        /// Remove a mention entry with the specified id
        /// </summary>
        /// <param name="mentionId">The id of the mention entry to remove</param>
        /// <returns>true on successfully, false otherwise</returns>
        public async Task<bool> RemoveMention(uint mentionId)
        {
            LogContext.PushProperty("Method","RemoveMention");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters mentionId={mentionId}");

            var query = $@"
                            DELETE FROM
                                Mentions
                            WHERE
                                Id = '{mentionId}';
                ";

            return await SqlHelpers.NonQueryAsync(query);
        }
    }
}
