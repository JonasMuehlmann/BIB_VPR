using System;
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
        /// <returns>The id of the created message if it was created successfully, null otherwise</returns>
        public async Task<uint?> CreateMessage(uint recipientsId, string senderId, string message, uint? parentMessageId = null)
        {
            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string query = $"INSERT INTO Messages " +
                        $"(RecipientId, SenderId, Message, CreationDate, ParentMessageId) " +
                        $"VALUES ({recipientsId}, '{senderId}', '{message}', GETDATE()";

                if (parentMessageId != null)
                {
                    query += $", {parentMessageId}) ";
                }    
                else
                {
                    query += ", NULL); ";
                }
                query += "SELECT SCOPE_IDENTITY();";

                SqlCommand scalarQuery = new SqlCommand(query, connection);
                var result = scalarQuery.ExecuteScalar();

                return SqlHelpers.TryConvertDbValue(result, Convert.ToUInt32);
            }
        }

        /// <summary>
        /// Retrieve all Messages of a team/chat.
        /// </summary>
        /// <param name="teamId">The id of a team, whose messages should be retrieved</param>
        ///<returns>An enumerable of data rows containing the message data</returns>
        public async Task<IList<Message>> RetrieveMessages(uint teamId)
        {
            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();
                string query = $"SELECT m.MessageId, m.RecipientId, m.SenderId, m.ParentMessageId, m.Message, m.CreationDate, " +
                    $"u.UserId, u.NameId, u.UserName " +
                    $"FROM Messages m " +
                    $"LEFT JOIN Users u ON m.SenderId = u.UserId " +
                    $"WHERE RecipientId = {teamId};";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                return SqlHelpers.MapToList(Mapper.MessageFromDataRow, adapter);
            }
        }
    }
}
