using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Context;


namespace Messenger.Core.Services
{
    public class ChannelService: AzureServiceBase
    {
        /// <summary>
        /// Creates a Channel with the given name inside the specified team
        /// </summary>
        /// <param name="channelName">Name of the channel</param>
        /// <param name="teamId">Id of the team to create the channel in</param>
        /// <returns>The id of the created channel if it was created successfully, null otherwise</returns>
        public async Task<uint?> CreateChannel(string channelName, uint teamId)
        {
            LogContext.PushProperty("Method","CreateChannel");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters channelName={channelName}, teamId={teamId}");

            string query = $@"
                                INSERT INTO
                                    Channels
                                VALUES(
                                    '{channelName}',
                                    {teamId}
                                );

                            SELECT SCOPE_IDENTITY();";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        /// <summary>
        /// Deletes a channel with a given channelId
        /// </summary>
        /// <param name="channelId">The id of the channel to delete</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one query, false otherwise</returns>
        public async Task<bool> RemoveChannel(uint channelId)
        {
            LogContext.PushProperty("Method","RemoveChannel");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters channelId={channelId}");

            string query = $@"
                                DELETE FROM
                                    Channels
                                WHERE
                                    ChannelId={channelId};";

            return await SqlHelpers.NonQueryAsync(query);
        }
        /// <summary>
        /// Deletes all channel from a specified team
        /// </summary>
        /// <param name="teamId">The id of the team to clear the channels from</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one query, false otherwise</returns>
        public async Task<bool> RemoveAllChannels(uint teamId)
        {
            LogContext.PushProperty("Method","RemoveAllChannels");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamId={teamId}");

            string query = $@"
                                DELETE FROM
                                    Channels
                                WHERE
                                    TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Rename a specified channel
        /// </summary>
        /// <param name="channelName">the new name of the channel></param>
        /// <param name="channelId">the id of the channel to rename</param>
        /// <returns>True, if the channel got renamed successfully, false otherwise</returns>
        public async Task<bool> RenameChannel(string channelName, uint channelId)
        {
            string query = $@"
                                UPDATE
                                    Channels
                                SET
                                    ChannelName='{channelName}'
                                WHERE
                                    ChannelId={channelId};";

            return await SqlHelpers.NonQueryAsync(query);
        }
        /// <summary>
        /// Construct a Channel object from data that belongs to the channel identified by channelId.
        /// </summary>
        /// <param name="channelId">The id of the channel to retrieve</param>
        /// <returns></returns>
        public async Task<Channel> GetChannel(uint channelId)
        {
            LogContext.PushProperty("Method","GetChannel");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters channelId={channelId}");

            string selectQuery = $@"
                                    SELECT
                                        *
                                    FROM
                                        Channels
                                    WHERE
                                        ChannelId={channelId}";

            SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, GetDefaultConnection());

            var rows = await SqlHelpers.GetRows("Channel", selectQuery);

            if (rows.Count() == 0)
            {
                logger.Information($"Return value: null");

                return null;
            }

            return rows.Select(Mapper.ChannelFromDataRow).First();
        }
    }
}
