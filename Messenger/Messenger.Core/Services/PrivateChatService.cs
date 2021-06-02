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
            uint? teamID = null;
            // Add myself and other user as members
           try
           {
               string query = $"INSERT INTO Teams (CreationDate) VALUES " +
                              $"( GETDATE());"
                              + "SELECT CAST(SCOPE_IDENTITY() AS int)";
            
            
               SqlCommand command = new SqlCommand(query, GetConnection());
               var team = command.ExecuteScalar();

               teamID = SqlHelpers.TryConvertDbValue(team, Convert.ToUInt32);              
               await AddMember(myUserId, Convert.ToUInt32(teamID));
               await AddMember(otherUserId, Convert.ToUInt32(teamID));
           }catch (SqlException e)
           {
               Debug.WriteLine($"Database Exception: {e.Message}");
           }
           return teamID;
        }
        
        
    }
}