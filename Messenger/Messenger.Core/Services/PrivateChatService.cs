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
        public async Task<bool> CreatePrivateChat(string myUserId, string otherUserId)
        {
            int teamID = CreatePrivateChatTeam();
            
           // Add myself and other user as members
           AddMember(myUserId, teamID);
           AddMember(otherUserId, teamID);
           
           return false;
        }
        

        public  int CreatePrivateChatTeam()
        {
            int teamID = -1;
            string query = $"INSERT INTO Teams (CreationDate) VALUES " +
                           $"( GETDATE());"
                           + "SELECT CAST(SCOPE_IDENTIDY() AS int)";
            
            //await SqlHelpers.NonQueryAsync(query, GetConnection());
            SqlCommand command = new SqlCommand(query, GetConnection());
            teamID = (int)command.ExecuteScalar();
            return teamID;
        }
    }
}