using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Context;


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
            LogContext.PushProperty("Method","CreatePrivateChat");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters myUserId={myUserId}, otherUserId={otherUserId}");

            uint? teamID;

            // Add myself and other user as members
           try
           {
               string query = $"INSERT INTO Teams (TeamName, CreationDate) VALUES " +
                              $"('', GETDATE());"
                              + "SELECT CAST(SCOPE_IDENTITY() AS INT)";


                using(SqlConnection connection = GetConnection())
                {

                    await connection.OpenAsync();

                    SqlCommand command = new SqlCommand(query, connection);

                    logger.Information($"Running the following query: {query}");

                    var team = command.ExecuteScalar();

                    teamID = SqlHelpers.TryConvertDbValue(team, Convert.ToUInt32);

                    logger.Information($"teamID has been determined as {teamID}");

                    var createEmptyRoleQuery = $"INSERT INTO Team_roles VALUES('', {teamID});";
                    // FIX: If AddMember() fails, we just created a dangling Team_Roles
                    // entry
                    logger.Information($"Running the following query: {createEmptyRoleQuery}");

                    logger.Information($"Result value: {await SqlHelpers.NonQueryAsync(createEmptyRoleQuery, connection)}");

                    var success1 = await AddMember(myUserId, teamID.Value);
                    var success2 = await AddMember(otherUserId, teamID.Value);

                    if (!(success1 && success2))
                    {
                        logger.Information("Could not add one or both users(s) to the team");
                        logger.Information($"Return value: null");

                        return null;
                    }


                    logger.Information($"Return value: {teamID}");

                    return teamID;
                }
           }
           catch (SqlException e)
           {
                logger.Information(e, $"Return value: null");
                return null;
           }
        }


        /// <summary>
        /// Lists all private chats of a specified user
        /// </summary>
        /// <param name="userId">the id of a user to retrieve private chats of</param>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllPrivateChatsFromUser(string userId)
        {
            LogContext.PushProperty("Method","GetAllPrivateChatsFromUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}");

            return (await GetAllTeamsByUserId(userId)).Where(team => team.Name == "");
        }


        /// <summary>
        /// In a private chat, retrieve the conversation partner's user id
        /// </summary>
        /// <param name="teamId">the id of the team belonging to the private chat</param>
        /// <returns>The user id of the conversation partner</returns>
        public string GetPartner(uint teamId)
        {
            LogContext.PushProperty("Method","GetPartner");
            LogContext.PushProperty("SourceContext", "SqlHelpers");
            logger.Information($"Function called with parameters teamId={teamId}");

            // NOTE: Private Chats currently only support 1 Members
            string query = "SELECT UserId  FROM Memberships m LEFT JOIN Teams t ON m.TeamId = t.TeamId "
                         + $"WHERE t.TeamId != {teamId} AND t.TeamName='';";

            try
            {

                using(SqlConnection connection = GetConnection())
                {
                    SqlCommand scalarQuery = new SqlCommand(query, connection);
                    var        otherUser   = scalarQuery.ExecuteScalar();

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.TryConvertDbValue(otherUser, Convert.ToString);

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e,"Return value: null");

                return null;
            }
        }
    }
}
