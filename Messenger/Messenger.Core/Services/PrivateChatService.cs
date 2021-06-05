using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Threading.Tasks;


namespace Messenger.Core.Services
{
    public class PrivateChatService : TeamService
    {
        /// <summary>
        /// Create a private chat between the users identified by myUserId and otherUserId.
        /// </summary>
        /// <param name="myUserId">user-Id of the Creator</param>
        /// <param name="otherUserId">user-Id of the other Person</param>
        /// <returns>The teamId of the created Team</returns>
        public async Task<uint?> CreatePrivateChat(string myUserId, string otherUserId)
        {
            Serilog.Context.LogContext.PushProperty("Method","CreatePrivateChat");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters myUserId={myUserId}, otherUserId={otherUserId}");

            uint teamID;
            // Add myself and other user as members
           try
           {
                string query = $"INSERT INTO Teams (CreationDate) VALUES "
                             + $"( GETDATE());"
                             + "SELECT CAST(SCOPE_IDENTITY() AS int)";


                SqlCommand command = new SqlCommand(query, GetConnection());

                logger.Information($"Running the following query: {query}");

                var team = command.ExecuteScalar();

                teamID = SqlHelpers.TryConvertDbValue(team, Convert.ToUInt32);

                logger.Information($"teamID has been determined as {teamID}");

                await AddMember(myUserId, teamID);
                await AddMember(otherUserId, teamID);

                logger.Information($"Return value: {teamID}");

                return teamID;
           }
           catch (SqlException e)
           {
                logger.Information(e, $"Return value: null");

                return null;
           }
        }


        /// <summary>
        /// Lists all private chats starting with the last private chat in which a message was sent
        /// </summary>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllPrivateChats()
        {
            Serilog.Context.LogContext.PushProperty("Method","GetAllPrivateChats");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called");

            string query = @"SELECT t.TeamId, t.CreationDate
                            FROM Teams t,  Messages m
                            WHERE TeamName IS NULL;
                            AND t.TeamId = m.RecipientId
                            ORDER BY m.CeationTime DESC;";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.MapToList(Mapper.TeamFromDataRow,
                                                      new SqlDataAdapter(query, connection));
                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, $"Return value: null");

                return null;
            }
        }


    }
}
