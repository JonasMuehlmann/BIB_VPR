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
        public async Task<int> CreatePrivateChat(string myUserId, string otherUserId)
        {
            int teamID = -1;
            // Add myself and other user as members
           try
           {
               string query = $"INSERT INTO Teams (CreationDate) VALUES " +
                              $"( GETDATE());"
                              + "SELECT CAST(SCOPE_IDENTITY() AS int)";
            
            
               SqlCommand command = new SqlCommand(query, GetConnection());
               teamID = (int)command.ExecuteScalar();
               
               AddMember(myUserId, teamID);
               AddMember(otherUserId, teamID);
           }catch (SqlException e)
           {
               Debug.WriteLine($"Database Exception: {e.Message}");
           }
           return teamID;
        }
        
        
    }
}