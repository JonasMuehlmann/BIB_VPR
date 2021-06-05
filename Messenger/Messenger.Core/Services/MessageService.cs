using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data.SqlClient;
using Messenger.Core.Helpers;
using Messenger.Core.Models;

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
            Serilog.Context.LogContext.PushProperty("Method","CreateMessage");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters recipientsId={recipientsId}, senderId={senderId}, parentMessageId={parentMessageId}, attachmentBlobNames={attachmentBlobNames}, message={message}");
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string correctedAttachmentBlobNames = attachmentBlobNames is null ? "NULL" : $"'{string.Join(",",attachmentBlobNames)}'";
                    string correctedParentMessageId     = parentMessageId     is null ? "NULL" : $"'{parentMessageId}'";

                    logger.Information($"attachmentBlobNames has been corrected to {attachmentBlobNames}");
                    logger.Information($"parentMessageId has been corrected to {correctedParentMessageId}");

                    string query = $"INSERT INTO Messages " +
                                   $"(RecipientId, SenderId, Message, CreationDate, ParentMessageId, AttachmentsBlobNames) " +
                                   $"VALUES ({recipientsId}, '{senderId}', '{message}', GETDATE(), {correctedParentMessageId}, {correctedAttachmentBlobNames}); SELECT SCOPE_IDENTITY();";

                    logger.Information($"Running the following query: {query}");

                    SqlCommand scalarQuery = new SqlCommand(query, connection);
                    var        result      = scalarQuery.ExecuteScalar();

                    result = SqlHelpers.TryConvertDbValue(result, Convert.ToUInt32);

                    logger.Information($"Return value: {result}");

                    return (uint?)result;
                }
            }
            catch (SqlException e)
            {
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
                Serilog.Context.LogContext.PushProperty("Method","RetrieveMessages");
                Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
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
    }
}
