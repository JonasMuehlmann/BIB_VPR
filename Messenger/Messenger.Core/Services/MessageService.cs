using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using System.Collections.Generic;
using System.Linq;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public class MessageService : AzureServiceBase
    {
        /// <summary>
        /// Send a message to a specified recipient and retrieve the sent messages id.
        ///</summary>
        ///<param name="recipientsId">The id of the recipient of this message</param>
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

            string correctedAttachmentBlobNames = attachmentBlobNames is null ? "NULL" : $"'{string.Join(",",attachmentBlobNames)}'";
            string correctedParentMessageId     = parentMessageId     is null ? "NULL" : $"'{parentMessageId}'";

            logger.Information($"attachmentBlobNames has been corrected to {correctedAttachmentBlobNames}");
            logger.Information($"parentMessageId has been corrected to {correctedParentMessageId}");

            string query = $"INSERT INTO Messages " +
                            $"(RecipientId, SenderId, Message, CreationDate, ParentMessageId, AttachmentsBlobNames) " +
                            $"VALUES ({recipientsId}, '{senderId}', '{message}', GETDATE(), {correctedParentMessageId}, {correctedAttachmentBlobNames}); SELECT SCOPE_IDENTITY();";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        /// <summary>
        /// Retrieve all Messages of a team/chat with user data of the sender
        /// </summary>
        /// <param name="teamId">The id of a team, whose messages should be retrieved</param>
        /// <returns>An enumerable of data rows containing the message data</returns>
        public async Task<IList<Message>> RetrieveMessages(uint teamId)
        {
            LogContext.PushProperty("Method","RetrieveMessages");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");

            string query = $"SELECT m.MessageId, m.RecipientId, m.SenderId, m.ParentMessageId, m.Message, m.CreationDate, "
                            + $"u.UserId, u.NameId, u.UserName "
                            + $"FROM Messages m "
                            + $"LEFT JOIN Users u ON m.SenderId = u.UserId "
                            + $"WHERE RecipientId = {teamId};";


            return await SqlHelpers.MapToList(Mapper.MessageFromDataRow, query);
        }

        /// <summary>
        /// Retrieve a message from a given MessageId
        /// </summary>
        /// <param name="messageId">The id of the message to retrieve</param>
        /// <returns>A complete message object</returns>
        public async Task<Message> GetMessage(uint messageId)
        {
            LogContext.PushProperty("Method","RetrieveMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $"SELECT MessageId, RecipientId, SenderId, ParentMessageId, Message, CreationDate "
                            + $"FROM Messages"
                            + $"WHERE MessageId={messageId};";

            var rows = await SqlHelpers.GetRows("Message", query);

            if (rows.Count() == 0)
            {
                logger.Information($"Return value: null");

                return null;
            }

            return rows.Select(Mapper.MessageFromDataRow).First();
        }

        /// <summary>
        /// Set the content of a specified message to a specified text
        /// </summary>
        /// <param name="messageId">The id of the message to edit</param>
        /// <param name="newContent">The new content of the message</param>
        /// <returns>True if the message got edited successfully, false otherwise</returns>
        public async Task<bool> EditMessage(uint messageId, string newContent)
        {
            LogContext.PushProperty("Method","EditMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}, newContent={newContent}");

            string query = $"UPDATE Messages SET Message='{newContent}' WHERE MessageId={messageId};";

            return await SqlHelpers.NonQueryAsync(query);

        }

        /// <summary>
        /// Delete a message
        /// </summary>
        /// <param name="messageId">The id of the message to delete</param>
        /// <returns>True if the message got deleted successfully, false otherwise</returns>
        public async Task<bool> DeleteMessage(uint messageId)
        {
            LogContext.PushProperty("Method","DeleteMessage");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $"DELETE FROM Messages WHERE MessageId={messageId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Retrieve the Blob File Names of files attached to a specified message
        /// </summary>
        /// <param name="messageId">The message to retrieve Blob File Names from</param>
        /// <returns>An enumerable of Blob File Names</returns>
        public async Task<IEnumerable<string>> GetBlobFileNamesOfAttachments(uint messageId)
        {
                LogContext.PushProperty("Method","GetBlobFileNamesOfAttachments");
                LogContext.PushProperty("SourceContext", this.GetType().Name);
                logger.Information($"Function called with parameters messageId={messageId}");

                string query = $"SELECT attachmentsBlobNames "
                             + $"FROM Messages "
                             + $"WHERE MessageId={messageId};";


                var blobFileString = await SqlHelpers.ExecuteScalarAsync(query, Convert.ToString);

                return blobFileString.Split(',');
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

                using (SqlConnection connection = GetDefaultConnection())
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

                string query = $@"EXEC RemoveOrUpdateReaction {messageId}, '{reaction}'";

                return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        ///	Retrieve The reactions of a message
        /// </summary>
        /// <param name="messageId">The id of the message to retrieve reactions from</param>
        /// <returns>A list of reaction objects</returns>
        public async Task<IEnumerable<Reaction>> RetrieveReactions(uint messageId)
        {
            LogContext.PushProperty("Method","RetrieveReactions");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $@"SELECT * FROM Reactions WHERE messageId={messageId};";

            return await SqlHelpers.MapToList(Mapper.ReactionFromDataRow, query);
        }
    }
}
