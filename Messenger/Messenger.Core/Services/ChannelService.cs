using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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
            Serilog.Context.LogContext.PushProperty("Method","CreateChannel");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters channelName={channelName}, teamId={teamId}");

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = $"INSERT INTO Channels (ChannelName,ChannelId) VALUES "
                                 + $"('{channelName}', {teamId}); SELECT SCOPE_IDENTITY();";

                    SqlCommand scalarQuery = new SqlCommand(query, connection);


                    logger.Information($"Running the following query: {scalarQuery}");

                    var result = SqlHelpers.TryConvertDbValue(scalarQuery.ExecuteScalar(),
                                                          Convert.ToUInt32);

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, "Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Deletes a channel with a given channelId in a specified team
        /// </summary>
        /// <param name="channelId">The id of the channel to delete</param>
        /// <param name="teamId">The id of the team to delete which contains the channel to delete</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at leasat one query, false otherwise</returns>
        public async Task<bool> RemoveChannel(uint channelId, uint teamId)
        {

            Serilog.Context.LogContext.PushProperty("Method","RemoveChannel");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters channelId={channelId}, teamId={teamId}");

            string query = $"DELETE FROM Channels WHERE ChannelId={channelId} AND TeamId={teamId};";

            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());
            logger.Information($"Return value: {result}");

            return result;

        }

        /// <summary>
        /// Rename a specified channe;
        /// </summary>
        /// <param name="channelName">the new name of the channel></param>
        /// <param name="channelId">the id of the channel to rename</param>
        /// <returns>True, if the channel got renamed successfully, false otherwise</returns>
        public async Task<bool> RenameChannel(string channelName, uint channelId)
        {
            string query = $"UPDATE Channels SET ChannelName='{channelName}' WHERE ChannelId={channelId};";

            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }
    }
}
