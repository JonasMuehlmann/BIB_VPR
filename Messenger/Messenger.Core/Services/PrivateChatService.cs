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
        /// Calls CreatePrivateChat Method an adds the creator and one other user to the chat
        /// </summary>
        /// <param name="myUserId">user-Id of the Creator</param>
        /// <param name="otherUserId">user-Id of the other Person</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> CreatePrivateChat(string myUserId, string otherUserId)
        {
            int teamID = CreatePrivateChatTeam();
            
           // Add myself and other user as members
           try
           {
               AddMember(myUserId, teamID);
               AddMember(otherUserId, teamID);
           }catch (Exception e)
           {
               Debug.WriteLine($"Database Exception: {e.Message}");

               return false;
           }
           return true;
        }
        

        // <summary>
        /// Creates the PrivateChat
        /// </summary>
        /// <returns>The teamId of the created Team</returns>
        public  int CreatePrivateChatTeam()
        {
            int teamID = -1;
            string query = $"INSERT INTO Teams (CreationDate) VALUES " +
                           $"( GETDATE());"
                           + "SELECT CAST(SCOPE_IDENTIDY() AS int)";
            
            
            SqlCommand command = new SqlCommand(query, GetConnection());
            teamID = (int)command.ExecuteScalar();
            return teamID;
        }
    }
}