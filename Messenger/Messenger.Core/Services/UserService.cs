using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using System.Data;

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
                $"INSERT INTO Users(UserId, UserName, Email, Bio) VALUES ({newUser.Id}, {newUser.Mail}, {newUser.Bio});";


            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }


        public async Task<bool> DeleteUser(string userId)
        {
            string query = $"DELETE FROM Users WHERE UserId={userId};";


            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Gets the user from application database, or create one if not exists.
        /// </summary>
        /// <param name="user">User data from MicrosoftGraphService</param>
        /// <returns>User from the application database</returns>
        public async Task<User> GetOrCreateApplicationUser(User user)
        {
            string selectQuery = $"SELECT UserId, UserName, Email, Bio FROM Users WHERE UserId='{user.Id}'";
            string insertQuery = $"INSERT INTO Users (UserId, UserName, Email) VALUES ('{user.Id}', '{user.DisplayName.Split('/')[0].Trim()}', '{user.Mail}')";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    // Get user from database
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection);
                    DataSet dataSet = new DataSet();
                    adapter.Fill(dataSet, "Users");

                    // Check if user is already in database
                    if (dataSet.Tables["Users"].Rows.Count > 0)
                    {
                        // Return user from database
                        DataRow row = dataSet.Tables["Users"].Rows[0];
                        return new User()
                        {
                            Id = row["UserId"].ToString(),
                            DisplayName = row["UserName"].ToString(),
                            Mail = row["Email"].ToString(),
                            Bio = row["Bio"].ToString()
                        };
                    }
                    else
                    {
                        // Create user based on MicrosoftGraphService
                        User newUser = new User()
                        {
                            Id = user.Id,
                            DisplayName = user.DisplayName,
                            Mail = user.Mail
                        };

                        SqlCommand insertCommand = new SqlCommand(insertQuery, connection);
                        insertCommand.ExecuteNonQuery();
                        
                        // Return new user 
                        return newUser;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}");

                return null;
            }
        }
    }
}