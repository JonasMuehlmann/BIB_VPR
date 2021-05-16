using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using System.Data;
using System.Linq;
using System.Collections.Generic;

namespace Messenger.Core.Services
{
    public class UserService : AzureServiceBase
    {
        /// <summary>
        /// Update a specified users name
        ///</summary>
        /// <param name="userId">The id of the user, whose name will be updated</param>
        /// <param name="newUsername">The new username to set</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        private async Task<bool> UpdateUsername(string userId, string newUsername)
        {
            using (SqlConnection connection = GetConnection())
            {
                int? newNameId = DetermineNewNameId(newUsername, connection);

                if (newNameId == null)
                {
                    return false;
                }

                string queryUpdate = $"UPDATE Users SET NameId={newNameId} WHERE UserId='{userId}';"
                                   + $"UPDATE Users SET UserName='{newUsername}' WHERE UserId='{userId}';";

                return await SqlHelpers.NonQueryAsync(queryUpdate, connection);
            }
        }

        /// <summary>
        /// Update a specified column for a specified user.
        ///</summary>
        /// <param name="userId">The id of the user, whose data will be updated</param>
        /// <param name="columnToChange">The column to update for the user</param>
        /// <param name="newVal">The new value for the specifed column for the specified user</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> Update(string userId, string columnToChange, string newVal)
        {
            if (columnToChange == "Username")
            {
                return await UpdateUsername(userId, newVal);
            }
            else
            {
                if (SqlHelpers.GetColumnType("Users", columnToChange, GetConnection()) == "nvarchar")
                {
                    newVal = "'" + newVal + "'";
                }

                string queryUpdateOther = $"UPDATE Users SET {columnToChange}={newVal} WHERE UserId='{userId}';";

                return await SqlHelpers.NonQueryAsync(queryUpdateOther, GetConnection());
            }
        }

        /// <summary>
        /// Delete the user with the specified userId.
        /// </summary>
        /// <param name="userId">The id of the user, whose data will be updated</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> DeleteUser(string userId)
        {
            string query = $"DELETE FROM Users WHERE UserId='{userId}';";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Create or retrieve an application user from a specified user object holding a GraphService Id.
        /// </summary>
        /// <param name="userdata">A user object holding a GraphService id which will be used to retrieve or create a user</param>
        /// <returns>The existing or newly created User object</returns>
        public async Task<User> GetOrCreateApplicationUser(User userdata)
        {
            string selectQuery = $"SELECT UserId, NameId, UserName, Email, Bio FROM Users WHERE UserId='{userdata.Id}'";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    // Get application user from database
                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection);
                    DataRow[] rows = SqlHelpers.GetRows("Users", adapter).ToArray();

                    // Check if application user is already in database
                    if (rows.Length > 0)
                    {
                        // Exists: return existing application user object from database
                        return rows
                            .Select(Mapper.UserFromDataRow)
                            .FirstOrDefault();
                    }
                    else
                    {
                        // Not exists: create Application user based on MicrosoftGraphService user
                        // Get a new id for the display name
                        string displayName = userdata.DisplayName.Split('/')[0].Trim();
                        int? newNameId = DetermineNewNameId(displayName, connection);

                        // Exit if name id is null
                        if (newNameId == null) return null;

                        // Create and execute query
                        string insertQuery = $"INSERT INTO Users (UserId, NameId, UserName, Email) " +
                            $"VALUES ('{userdata.Id}', {newNameId}, '{displayName}', '{userdata.Mail}')";

                        await SqlHelpers.NonQueryAsync(insertQuery, connection);

                        // Return the new application user, mapped directly from MSGraph
                        userdata.NameId = (int)newNameId;
                        return Mapper.UserFromMSGraph(userdata);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}");

                return null;
            }
        }

        /// <summary>
        /// Determine a usernames new NameId.
        /// </summarry>
        /// <param = "username">A username whose nameid is the be determined</param>
        ///<returns>Null on database errors, the appropriate NameId otherwise</returns>
        private int? DetermineNewNameId(string username, SqlConnection connection)
        {
            string query = $"SELECT MAX(NameId) FROM USERS WHERE UserName='{username}'";
            try
            {
                SqlCommand scalarQuery = new SqlCommand(query, connection);

                // Will be System.DBNull if there is no other user with the same name
                var result = scalarQuery.ExecuteScalar();

                return result.GetType() == typeof(DBNull) ? 0 : Convert.ToInt32(result) + 1;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}");

                return null;
            }

        }
    }
}
