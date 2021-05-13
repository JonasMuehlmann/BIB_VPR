using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;


namespace Messenger.Core.Services
{
    public class UserService : AzureServiceBase
    {
        public async Task<bool> Update(string userId, string columnToChange, string newVal)
        {
            string query = $"UPDATE Users SET {columnToChange}={newVal} WHERE UserId={userId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }


        public async Task<bool> CreateUser(User newUser)
        {
            string query =
                $"INSERT INTO Users(UserId, UserName, Email, Bio) VALUES ({newUser.Id}, {newUser.Email}, {newUser.Bio});";


            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }


        public async Task<bool> DeleteUser(string userId)
        {
            string query = $"DELETE FROM Users WHERE UserId={userId};";


            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }
    }
}