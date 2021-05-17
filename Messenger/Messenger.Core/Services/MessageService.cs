using System.Threading.Tasks;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Messenger.Core.Helpers;
using Messenger.Core.Models;


namespace Messenger.Core.Services
{
    public class MessageService : AzureServiceBase
    {
        /// <summary>
        /// Send a message to a specified recipient.
        ///</summary>
        ///<param name="recipientId">The id of the recipient of this message</param>
        ///<param name="senderId">The id of the message's sender</param>
        ///<param name="message">The content of the message</param>
        ///<param name="parentMessageId">The optional id of a message this one is a reply to</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> CreateMessage(int recipientId, string senderId, string message, int? parentMessageId = null)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = $"INSERT INTO Messages(RecipientId, SenderId, ParentMessageId, Message, CreationDate) VALUES({recipientId}, '{senderId}', {parentMessageId}, '{message}', GETDATE();";


                return await SqlHelpers.NonQueryAsync(query, connection);
            }
        }

        /// <summary>
        /// Retrieve all Messages of a team/chat.
        /// </summary>
        /// <param name="teamId">The id of a team, whose messages should be retrieved</param>
        ///<returns>An enumerable of data rows containing the message data</returns>
        public List<Message> RetrieveMessages(int teamId)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = $"SELECT * FROM Messages LEFT JOIN Memberships ON (Messages.RecipientsId = Memberships.MembershipId) WHERE Memberships.TeamId = {teamId};";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                
                return SqlHelpers.GetRows("Messages", adapter).Select(row => Mapper.MessageFromDataRow(row, GetConnection())).ToList();
            }

        }
    }
}
