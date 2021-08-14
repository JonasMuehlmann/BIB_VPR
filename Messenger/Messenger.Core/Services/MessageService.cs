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
        public static async Task<uint?> CreateMessage(uint recipientsId,
                                               string senderId,
                                               string message,
                                               uint? parentMessageId = null,
                                               IEnumerable<string> attachmentBlobNames = null)
        {
            LogContext.PushProperty("Method","CreateMessage");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters recipientsId={recipientsId}, senderId={senderId}, parentMessageId={parentMessageId}, attachmentBlobNames={attachmentBlobNames}, message={message}");

            string correctedAttachmentBlobNames = attachmentBlobNames is null ? "NULL" : $"{string.Join(",",attachmentBlobNames)}";
            string correctedParentMessageId     = parentMessageId     is null ? "NULL" : $"{parentMessageId}";

            logger.Information($"attachmentBlobNames has been corrected to {correctedAttachmentBlobNames}");
            logger.Information($"parentMessageId has been corrected to {correctedParentMessageId}");

            string query = $@"
                                INSERT INTO
                                    Messages
                                        (
                                        SenderId,
                                        ParentMessageId,
                                        Message,
                                        CreationDate,
                                        attachmentsBlobNames,
                                        RecipientId
                                        )
                                VALUES (
                                        '{senderId}',
                                         {correctedParentMessageId},
                                        '{message}',
                                         GETDATE(),
                                        '{correctedAttachmentBlobNames}',
                                         {recipientsId}
                                );

                                SELECT SCOPE_IDENTITY();";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        /// <summary>
        /// Retrieve all Messages of a channel/chat with user data of the sender
        /// </summary>
        /// <param name="channelId">The id of a channel, whose messages should be retrieved</param>
        /// <returns>An enumerable of data rows containing the message data</returns>
        public static async Task<IList<Message>> RetrieveMessages(uint channelId)
        {
            LogContext.PushProperty("Method","RetrieveMessages");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters channelId={channelId}");

            string query = $@"
                                SELECT
                                    m.MessageId,
                                    m.RecipientId,
                                    m.SenderId,
                                    m.ParentMessageId,
                                    m.Message,
                                    m.CreationDate,
                                    u.UserId,
                                    u.NameId,
                                    u.UserName
                                FROM
                                    Messages m
                                LEFT JOIN Users u
                                    ON m.SenderId = u.UserId
                                WHERE
                                    RecipientId = {channelId};";


            return await SqlHelpers.MapToList(Mapper.MessageFromDataRow, query);
        }
        /// <summary>
        /// Retrieve all replies to a message
        /// </summary>
        /// <param name="messageId">The id of the message to retrieve replies from</param>
        /// <returns>An IList of Message objects representing the replies</returns>
        public static async Task<IList<Message>> RetrieveReplies(uint messageId)
        {
            LogContext.PushProperty("Method","RetrieveReplies");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $@"
                                SELECT
                                    m.MessageId,
                                    m.RecipientId,
                                    m.SenderId,
                                    m.ParentMessageId,
                                    m.Message,
                                    m.CreationDate,
                                    u.UserId,
                                    u.NameId,
                                    u.UserName
                                FROM
                                    Messages m
                                LEFT JOIN Users u
                                    ON m.SenderId = u.UserId
                                WHERE
                                    ParentMessageId = {messageId}";


            return await SqlHelpers.MapToList(Mapper.MessageFromDataRow, query);
        }

        /// <summary>
        /// Retrieve a message from a given MessageId
        /// </summary>
        /// <param name="messageId">The id of the message to retrieve</param>
        /// <returns>A complete message object</returns>
        public static async Task<Message> GetMessage(uint messageId)
        {
            LogContext.PushProperty("Method", "GetMessage");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters messageId={messageId}");


            // TODO: Cleanup
            string query = $"SELECT m.MessageId, m.RecipientId, m.SenderId, m.ParentMessageId, m.Message, m.CreationDate, "
                            + $"u.UserId, u.NameId, u.UserName "
                            + $"FROM Messages m "
                            + $"LEFT JOIN Users u ON m.SenderId = u.UserId "
                            + $"WHERE MessageId={messageId};";

            var rows = await SqlHelpers.GetRows("Messages", query);

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
        public static async Task<bool> EditMessage(uint messageId, string newContent)
        {
            LogContext.PushProperty("Method","EditMessage");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters messageId={messageId}, newContent={newContent}");

            string query = $@"
                                UPDATE
                                    Messages
                                SET
                                    Message='{newContent}'
                                WHERE
                                    MessageId={messageId};";

            return await SqlHelpers.NonQueryAsync(query);

        }

        /// <summary>
        /// Delete a message
        /// </summary>
        /// <param name="messageId">The id of the message to delete</param>
        /// <returns>True if the message got deleted successfully, false otherwise</returns>
        public static async Task<bool> DeleteMessage(uint messageId)
        {
            LogContext.PushProperty("Method","DeleteMessage");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters messageId={messageId}");

            // Delete replies to prevent errors
            var replies = await RetrieveReplies(messageId);

            if (replies != null)
            {
                foreach (var reply in replies)
                {
                    await DeleteMessage(reply.Id);
                }
            }

            // Delete reactions to prevent errors
            var reactions = await RetrieveReactions(messageId);

            if (reactions != null)
            {
                foreach (var reaction in reactions)
                {
                    await RemoveReaction(messageId, reaction.UserId, reaction.Symbol);
                }
            }

            // Delete actual message
            string query = $@"
                                DELETE FROM
                                    Messages
                                WHERE
                                    MessageId={messageId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Retrieve the Blob File Names of files attached to a specified message
        /// </summary>
        /// <param name="messageId">The message to retrieve Blob File Names from</param>
        /// <returns>An enumerable of Blob File Names</returns>
        public static async Task<IEnumerable<string>> GetBlobFileNamesOfAttachments(uint messageId)
        {
            LogContext.PushProperty("Method","GetBlobFileNamesOfAttachments");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $@"
                                SELECT
                                    attachmentsBlobNames
                                FROM
                                    Messages
                                WHERE
                                    MessageId={messageId};";

            var blobFileString = await SqlHelpers.ExecuteScalarAsync(query, Convert.ToString);

            if (string.IsNullOrEmpty(blobFileString))
            {
                return null;
            }

            return blobFileString.Split(',');
        }

        /// <summary>
        ///	Add a reaction to a message
        /// </summary>
        /// <param name="messageId">The id of the message to add a reaction to</param>
        /// <param name="userId">The id of the user making the reaction</param>
        /// <param name="reaction">The reaction to add to the message</param>
        /// <returns>The id of the created reaction</returns>
        public static async Task<uint> AddReaction(uint messageId, string userId, string reaction)
        {
                LogContext.PushProperty("Method","AddReaction");
                LogContext.PushProperty("SourceContext", "MessageService");
                logger.Information($"Function called with parameters messageId={messageId}, userId={userId}, reaction={reaction}");

                string query = $@"
                                    INSERT INTO
                                        Reactions
                                    VALUES(
                                        {messageId},
                                        '{reaction}',
                                        '{userId}'
                                    );

                                    SELECT SCOPE_IDENTITY();";

                return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        /// <summary>
        ///	Remove a reaction from a message
        /// </summary>
        /// <param name="messageId">The id of the message to remove a reaction from</param>
        /// <param name="userId">The id of the user whose reaction to remove</param>
        /// <param name="reaction">The reaction to remove from the message</param>
        /// <returns>Whetever or not to the reaction was successfully removed</returns>
        public static async Task<bool> RemoveReaction(uint messageId, string userId, string reaction)
        {
                LogContext.PushProperty("Method","RemoveReaction");
                LogContext.PushProperty("SourceContext", "MessageService");
                logger.Information($"Function called with parameters messageId={messageId}, userId={userId}, reaction={reaction}");

                string query = $@"
                                    DELETE FROM
                                        Reactions
                                    WHERE
                                        messageId = {messageId}
                                        AND
                                        userId = '{userId}'
                                        AND
                                        reaction = '{reaction}';";

                return await SqlHelpers.NonQueryAsync(query);
        }
        /// <summary>
        /// Check if a user made a speific reaction to a message
        /// </summary>
        /// <param name="messageId">The id of the message to check reactions for</param>
        /// <param name="userId">The id of the user to check reactions for</param>
        /// <param name="reaction">The reaction to check for</param>
        /// <returns>
        /// True if the user did not yet make the reaction to to the message,
        /// false otherwise
        /// </returns>
        public static async Task<bool> CanMakeReaction(uint messageId, string userId, string reaction)
        {
                LogContext.PushProperty("Method","CanMakeReaction");
                LogContext.PushProperty("SourceContext", "MessageService");
                logger.Information($"Function called with parameters messageId={messageId}, userId={userId}, reaction={reaction}");

                string query = $@"
                                    SELECT
                                        COUNT(*)
                                    FROM
                                        Reactions
                                    WHERE
                                        MessageId = {messageId}
                                        AND
                                        userId = '{userId}'
                                        AND
                                        Reaction = '{reaction}';";

                // NOTE: Inverting because Errors in the SqlHelpers return default
                // values for the type to be converted to(false in this case)
                return !(await SqlHelpers.ExecuteScalarAsync(query, Convert.ToBoolean));
        }

        /// <summary>
        ///	Retrieve The reactions of a message
        /// </summary>
        /// <param name="messageId">The id of the message to retrieve reactions from</param>
        /// <returns>A list of reaction objects</returns>
        public static async Task<IEnumerable<Reaction>> RetrieveReactions(uint messageId)
        {
            LogContext.PushProperty("Method","RetrieveReactions");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $@"SELECT * FROM Reactions WHERE messageId={messageId};";

            return await SqlHelpers.MapToList(Mapper.ReactionFromDataRow, query);
        }
        /// <summary>
        ///	Retrieve a reaction object from a reactionId
        /// </summary>
        /// <param name="reactionId">The id of the reaction to make an object from</param>
        /// <returns>A reaction object</returns>
        public static async Task<Reaction> GetReaction(uint reactionId)
        {
            LogContext.PushProperty("Method","GetReaction");
            LogContext.PushProperty("SourceContext", "MessageService");
            logger.Information($"Function called with parameters reactionId={reactionId}");

            string query = $@"SELECT * FROM Reactions WHERE reactionId={reactionId};";

            // TODO: Implement SqlHelper to build objects from a single row
            return (await SqlHelpers.MapToList(Mapper.ReactionFromDataRow, query)).First();
        }
    }
}
