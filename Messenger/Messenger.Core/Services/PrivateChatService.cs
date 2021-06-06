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
                    var team = command.ExecuteScalar();

                    teamID = SqlHelpers.TryConvertDbValue(team, Convert.ToUInt32);
                }
                    var success1 = await AddMember(myUserId, teamID.Value);
                    var success2 = await AddMember(otherUserId, teamID.Value);

                    if (!success1 && success2)
                    {
                        return null;
                    }

                    return teamID;
           }
           catch (SqlException e)
           {
                Debug.WriteLine($"Database Exception: {e.Message}");
                return null;
           }
        }


        /// <summary>
        /// Lists all private chats starting with the last private chat in which a message was sent
        /// </summary>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllPrivateChatsFromUser(string userId)
        {
        return (await GetAllTeamsByUserId(userId)).Where(team => team.Name == "");
        }
    }
}
