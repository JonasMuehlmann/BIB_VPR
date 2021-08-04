using System;
using System.Collections.Generic;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Context;


namespace Messenger.Core.Services
{
    public class ChannelService: SignalREnabledAzureServiceBase
    {
        /// <summary>
        /// Creates a Channel with the given name inside the specified team
        /// </summary>
        /// <param name="channelName">Name of the channel</param>
        /// <param name="teamId">Id of the team to create the channel in</param>
        /// <returns>The id of the created channel if it was created successfully, null otherwise</returns>
        private static async Task<uint?> CreateChannelImpl(string channelName, uint teamId)
        {
            LogContext.PushProperty("Method",        "CreateChannelImpl");
            LogContext.PushProperty("SourceContext", "ChannelService");

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
        private static async Task<bool> RemoveChannelImpl(uint channelId)
        {
            LogContext.PushProperty("Method",        "RemoveChannelImpl");
            LogContext.PushProperty("SourceContext", "ChannelService");

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
        private static async Task<bool> RemoveAllChannelsImpl(uint teamId)
        {
            LogContext.PushProperty("Method",        "RemoveAllChannelsImpl");
            LogContext.PushProperty("SourceContext", "ChannelService");

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
        public static async Task<bool> RenameChannelImpl(string channelName, uint channelId)
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
        public static async Task<Channel> GetChannel(uint channelId)
        {
            LogContext.PushProperty("Method",        "GetChannel");
            LogContext.PushProperty("SourceContext", "ChannelService");

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


        /// <summary>
        /// Pin a specified message in the specified channel
        /// </summary>
        /// <param name="messageId">The id of the message to pin</param>
        /// <param name="channelId">The id of the channel to pin the message in</param>
        /// <returns>True on success, false on failure</returns>
        public static async Task<bool> PinMessage(uint messageId, uint channelId)
        {
            LogContext.PushProperty("Method",        "PinMessage");
            LogContext.PushProperty("SourceContext", "ChannelService");

            logger.Information($"Function called with parameters messageId={messageId}, channelId={channelId}");

            string query = $@"
                                INSERT INTO
                                    PinnedMessages
                                VALUES(
                                        {channelId},
                                        {messageId}
                                      );
                ";

            return await SqlHelpers.NonQueryAsync(query);
        }


        /// <summary>
        /// Unpin a specified message in the specified channel
        /// </summary>
        /// <param name="messageId">The id of the message to Unpin</param>
        /// <param name="channelId">The id of the channel to Unpin the message in</param>
        /// <returns>True on success, false on failure</returns>
        public static async Task<bool> UnPinMessage(uint messageId, uint channelId)
        {
            LogContext.PushProperty("Method",        "UnpinMessage");
            LogContext.PushProperty("SourceContext", "ChannelService");

            logger.Information($"Function called with parameters messageId={messageId}, channelId={channelId}");

            string query = $@"
                                DELETE FROM
                                    PinnedMessages
                                WHERE
                                    channelId = {channelId}
                                    AND
                                    messageId = {messageId};
                ";

            return await SqlHelpers.NonQueryAsync(query);
        }


        /// <summary>
        /// Retrieve the pinned messages of a channel
        /// </summary>
        /// <param name="channelId">The channel to retrieve pinned messages from</param>
        /// <returns>An enumerable of message objects representing the pinned messages</returns>
        public static async Task<IEnumerable<Message>> RetrievePinnedMessages(uint channelId)
        {
            LogContext.PushProperty("Method",        "RetrievePinnedMessages");
            LogContext.PushProperty("SourceContext", "ChannelService");

            logger.Information($"Function called with parameters channelId={channelId}");

            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Messages m
                                LEFT JOIN PinnedMessages p ON
                                    p.MessageId = m.MessageId
                                LEFT JOIN Users u ON
                                    u.userId = m.senderId
                                WHERE
                                    p.ChannelId = {channelId};
                ";

            return await SqlHelpers.MapToList(Mapper.MessageFromDataRow, query);
        }


        #region SignalREnabled

        /// <summary>
        /// Add a channel to a specified team
        /// </summary>
        /// <param name="teamId">Id of the team to add the channel to</param>
        /// <param name="channelName">Name of the newly created channel</param>
        /// <returns>True if the channel was successfully created, false otherwise</returns>
        public static async Task<uint?> CreateChannel(string channelName, uint teamId)
        {
            LogContext.PushProperty("Method",        "CreateChannel");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters channelName={channelName}, teamId={teamId}");

            var channelId = await CreateChannelImpl(channelName, teamId);

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
        public static async Task<bool> DeleteChannel(uint channelId)
        {
            LogContext.PushProperty("Method",        "RemoveChannel");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters channelId={channelId}");

            var result = await RemoveChannelImpl(channelId);

            var channel = await GetChannel(channelId);

            await SignalRService.DeleteChannel(channel);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Remove all channels from a team
        /// </summary>
        /// <param name="teamId">Id of the team to remove channels from</param>
        /// <returns>An awaitable task</returns>
        public static async Task<bool> DeleteAllChannels(uint teamId)
        {
            LogContext.PushProperty("Method",        "RemoveAllChannels");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters teamId={teamId}");

            var result = await RemoveAllChannelsImpl(teamId);

            var channels = await TeamService.GetAllChannelsByTeamId(teamId);

            foreach (var channel in channels)
            {
                await SignalRService.DeleteChannel(channel);
            }

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Rename A channel and notify other clients
        /// </summary>
        /// <param name="channelId">Id of the channel to rename</param>
        /// <param name="channelName">The new name of the channel</param>
        /// <returns>True if the channel was successfully renamed, false otherwise</returns>
        public static async Task<bool> RenameChannel(string channelName, uint channelId)
        {
            LogContext.PushProperty("Method",        "RenameChannel");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters channelName={channelName}, channelId={channelId}");

            var result = await RenameChannelImpl(channelName, channelId);

            var channel = await GetChannel(channelId);

            await SignalRService.UpdateChannel(channel);

            logger.Information($"Return value: {result}");

            return result;
        }

        #endregion SignalREnabled
    }
}
