using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public class MessageService : AzureServiceBase
    {
        /// <summary>
        /// Send a message to a specified recipient and retrieve the sent messages id.
        ///</summary>
        ///<param name="recipientId">The id of the recipient of this message</param>
        ///<param name="senderId">The id of the message's sender</param>
        ///<param name="message">The content of the message</param>
        ///<param name="parentMessageId">The optional id of a message this one is a reply to</param>
        ///<param name="attachmentBlobNames">Enumerable of blob names of uploaded attachments</param>
        /// <returns>The id of the created message if it was created successfully, null otherwise</returns>
        public async Task<uint?> CreateMessage(uint recipientsId,
                                               string senderId,
                                               string message,
                                               uint? parentMessageId = null,
                                               IEnumerable<string> attachmentBlobNames = null)
        {
            LogContext.PushProperty("Method","CreateMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters recipientsId={recipientsId}, senderId={senderId}, parentMessageId={parentMessageId}, attachmentBlobNames={attachmentBlobNames}, message={message}");
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string correctedAttachmentBlobNames = attachmentBlobNames is null ? "NULL" : $"'{string.Join(",",attachmentBlobNames)}'";
                    string correctedParentMessageId     = parentMessageId     is null ? "NULL" : $"'{parentMessageId}'";

                    logger.Information($"attachmentBlobNames has been corrected to {correctedAttachmentBlobNames}");
                    logger.Information($"parentMessageId has been corrected to {correctedParentMessageId}");

                    string query = $"INSERT INTO Messages " +
                                   $"(RecipientId, SenderId, Message, CreationDate, ParentMessageId, AttachmentsBlobNames) " +
                                   $"VALUES ({recipientsId}, '{senderId}', '{message}', GETDATE(), {correctedParentMessageId}, {correctedAttachmentBlobNames}); SELECT SCOPE_IDENTITY();";

                    logger.Information($"Running the following query: {query}");

                    SqlCommand scalarQuery = new SqlCommand(query, connection);
                    var        result      = scalarQuery.ExecuteScalar();

                    result = SqlHelpers.TryConvertDbValue(result, Convert.ToUInt32);

                    LogContext.PushProperty("Method","CreateMessage");
                    LogContext.PushProperty("SourceContext", this.GetType().Name);

                    logger.Information($"Return value: {result}");

                    return (uint?)result;
                }
            }
            catch (SqlException e)
            {
                LogContext.PushProperty("Method","CreateMessage");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                logger.Information(e, $"Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Retrieve all Messages of a team/chat with user data of the sender
        /// </summary>
        /// <param name="teamId">The id of a team, whose messages should be retrieved</param>
        /// <returns>An enumerable of data rows containing the message data</returns>
        public async Task<IList<Message>> RetrieveMessages(uint teamId)
        {
            using (SqlConnection connection = GetConnection())
            {
                LogContext.PushProperty("Method","RetrieveMessages");
                LogContext.PushProperty("SourceContext", this.GetType().Name);
                logger.Information($"Function called with parameters teamId={teamId}");

                await connection.OpenAsync();

                string query = $"SELECT m.MessageId, m.RecipientId, m.SenderId, m.ParentMessageId, m.Message, m.CreationDate, "
                             + $"u.UserId, u.NameId, u.UserName "
                             + $"FROM Messages m "
                             + $"LEFT JOIN Users u ON m.SenderId = u.UserId "
                             + $"WHERE RecipientId = {teamId};";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                logger.Information($"Running the following query: {query}");

                var result = SqlHelpers.MapToList(Mapper.MessageFromDataRow, adapter);

                return result;
            }
        }

        /// <summary>
        /// Retrieve a message from a given MessageId
        /// </summary>
        /// <param name="messageId">The id of the message to retrieve</param>
        /// <returns>A complete message object</returns>
        public async Task<Message> GetMessage(uint messageId)
        {
            using (SqlConnection connection = GetConnection())
            {
                LogContext.PushProperty("Method","RetrieveMessage");
                LogContext.PushProperty("SourceContext", this.GetType().Name);
                logger.Information($"Function called with parameters messageId={messageId}");

                await connection.OpenAsync();

                string query = $"SELECT MessageId, RecipientId, SenderId, ParentMessageId, Message, CreationDate "
                             + $"FROM Messages"
                             + $"WHERE MessageId={messageId};";

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                var rows = SqlHelpers.GetRows("Message", adapter);

                if (rows.Count() == 0)
                {
                    logger.Information($"Return value: null");

                    return null;
                }

                var result = rows.Select(Mapper.MessageFromDataRow).First();

                logger.Information($"Return value: {result}");

                return result;
            }
        }

        /// <summary>
        /// Set the content of a specified message to a specified text
        /// <summary>
        /// <param name="messageId">The id of the message to edit</param>
        /// <param name="newContent">The new content of the message</param>
        /// <returns>True if the message got edited successfully, false otherwise</returns>
        public async Task<bool> EditMessage(uint messageId, string newContent)
        {
            Serilog.Context.LogContext.PushProperty("Method","EditMessage");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}, newContent={newContent}");


            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = $"UPDATE Messages SET Message='{newContent}' WHERE MessageId={messageId};";


                logger.Information($"Running the following query: {query}");

                try
                {
                    SqlCommand scalarQuery = new SqlCommand(query, connection);

                    var result = SqlHelpers.TryConvertDbValue(scalarQuery.ExecuteNonQuery(), Convert.ToBoolean);

                    logger.Information($"Return value: {result}");

                    return result;
                }
                catch (SqlException e)
                {
                    logger.Information(e, $"Return value: false");

                    return false;
                }
            }
        }

        /// <summary>
        /// Delete a message
        /// <summary>
        /// <param name="messageId">The id of the message to delete</param>
        /// <returns>True if the message got deleted successfully, false otherwise</returns>
        public async Task<bool> DeleteMessage(uint messageId)
        {
            Serilog.Context.LogContext.PushProperty("Method","DeleteMessage");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}");


            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = $"DELETE FROM Messages WHERE MessageId={messageId};";

                logger.Information($"Running the following query: {query}");

                try
                {
                    SqlCommand scalarQuery = new SqlCommand(query, connection);


                    var        numAffectedRows = scalarQuery.ExecuteNonQuery();
                    var result = SqlHelpers.TryConvertDbValue(numAffectedRows, Convert.ToBoolean);

                    logger.Information($"Return value: {result}");

                    return result;
                }
                catch (SqlException e)
                {
                    logger.Information(e, $"Return value: false");

                    return false;
                }
            }
        }

        /// <summary>
        /// Retrieve the Blob File Names of files attached to a specified message
        /// </summary>
        /// <param name="teamId">The message to retrieve Blob File Names from</param>
        /// <returns>An enumerable of Blob File Names</returns>
        public async Task<IEnumerable<string>> GetBlobFileNamesOfAttachments(uint messageId)
        {
            using (SqlConnection connection = GetConnection())
            {
                LogContext.PushProperty("Method","GetBlobFileNamesOfAttachments");
                LogContext.PushProperty("SourceContext", this.GetType().Name);
                logger.Information($"Function called with parameters messageId={messageId}");

                await connection.OpenAsync();

                string query = $"SELECT attachmentsBlobNames "
                             + $"FROM Messages"
                             + $"WHERE MessageId={messageId};";

                SqlCommand cmd = new SqlCommand(query, connection);

                logger.Information($"Running the following query: {query}");

                var blobFileString = SqlHelpers.TryConvertDbValue(cmd.ExecuteScalar(), Convert.ToString);

                var result = blobFileString.Split(',');

                return result;
            }
        }

        /// <summary>
        ///	Add a reaction to a message
        /// </summary>
        /// <param name="messageId">The id of the message to add a reaction to</param>
        /// <param name="reaction">The reaction to add to the message</param>
        /// <returns></returns>
        public async Task<uint> AddReaction(uint messageId, string reaction)
        {
                LogContext.PushProperty("Method","AddReaction");
                LogContext.PushProperty("SourceContext", this.GetType().Name);
                logger.Information($"Function called with parameters messageId={messageId}, reaction={reaction}");

                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = $@"EXEC AddOrUpdateReaction {messageId}, '{reaction}'";

                    SqlCommand cmd = new SqlCommand(query, connection);

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.TryConvertDbValue(cmd.ExecuteScalar(), Convert.ToUInt32);

                    LogContext.PushProperty("Method","AddReaction");
                    LogContext.PushProperty("SourceContext", this.GetType().Name);

                    logger.Information($"Return value: {result}");

                    return result;
                }
        }

        /// <summary>
        ///	Remove a reaction from a message
        /// </summary>
        /// <param name="messageId">The id of the message to remove a reaction from</param>
        /// <param name="reaction">The reaction to remove from the message</param>
        /// <returns>Whetever or not to the reaction was successfully removed</returns>
        public async Task<bool> RemoveReaction(uint messageId, string reaction)
        {
                LogContext.PushProperty("Method","RemoveReaction");
                LogContext.PushProperty("SourceContext", this.GetType().Name);
                logger.Information($"Function called with parameters messageId={messageId}, reaction={reaction}");

                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = $@"EXEC RemoveOrUpdateReaction {messageId}, '{reaction}'";

                    SqlCommand cmd = new SqlCommand(query, connection);

                    logger.Information($"Running the following query: {query}");

                    var result = await SqlHelpers.NonQueryAsync(query, connection);

                    LogContext.PushProperty("Method","RemoveReaction");
                    LogContext.PushProperty("SourceContext", this.GetType().Name);

                    logger.Information($"Return value: {result}");

                    return result;
                }
        }

        /// <summary>
        ///	Retrieve The reactions of a message
        /// </summary>
        /// <param name="messageId">The id of the message to retrieve reactions from</param>
        /// <returns>The reactions mapped to their number of occurrences</returns>
        public async Task<Dictionary<string, int>> RetrieveReactions(uint messageId)
        {
            LogContext.PushProperty("Method","RetrieveReactions");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $@"SELECT * FROM Reactions WHERE messageId={messageId};";
            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                var result = Mapper.ReactionMappingFromAdapter(new SqlDataAdapter(query, connection));

                LogContext.PushProperty("Method","RetrieveReactions");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                logger.Information($"Return value: {result}");

                return result;
            }
        }
    }
}
