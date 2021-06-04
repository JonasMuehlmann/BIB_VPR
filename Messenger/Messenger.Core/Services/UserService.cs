using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using System.Data;
using System.Linq;

namespace Messenger.Core.Services
{
    public class UserService : AzureServiceBase
    {
        /// <summary>
        /// Update a specified column for a specified user.
        ///</summary>
        /// <param name="userId">The id of the user, whose data will be updated</param>
        /// <param name="columnToChange">The column to update for the user</param>
        /// <param name="newVal">The new value for the specifed column for the specified user</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public async Task<bool> Update(string userId, string columnToChange, string newVal)
        {
            Serilog.Context.LogContext.PushProperty("Method","Update");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, columnToChange={columnToChange}, newVal={newVal}");

            if (columnToChange == "Username")
            {
                logger.Information("Forwarding arguments to UpdateUsername()");
                return await UpdateUsername(userId, newVal);
            }

            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                if (SqlHelpers.GetColumnType("Users", columnToChange, connection) == "nvarchar")
                {
                    logger.Information($"columnToChange is of type nvarchar, now newVal={newVal}");
                    newVal = "'" + newVal + "'";
                }

                string queryUpdateOther = $"UPDATE Users SET {columnToChange}={newVal} WHERE UserId='{userId}';";

                logger.Information($"Running the following query: {queryUpdateOther}");

                var result = await SqlHelpers.NonQueryAsync(queryUpdateOther, connection);

                logger.Information($"Return value: {result}");

                return result;
            }
        }

        /// <summary>
        /// Update a specified users name
        ///</summary>
        /// <param name="userId">The id of the user, whose name will be updated</param>
        /// <param name="newUsername">The new username to set</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> UpdateUsername(string userId, string newUsername)
        {
            Serilog.Context.LogContext.PushProperty("Method","UpdateUsername");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, newUsername={newUsername}");

            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                uint? newNameId = DetermineNewNameId(newUsername, connection);

                logger.Information($"newNameId has been determined as {newNameId}");

                if (newNameId == null)
                {
                    logger.Information($"Return value: false");
                    return false;
                }

                string queryUpdate = $"UPDATE Users SET NameId={newNameId} WHERE UserId='{userId}';"
                                   + $"UPDATE Users SET UserName='{newUsername}' WHERE UserId='{userId}';";

                logger.Information($"Running the following query: {queryUpdate}");

                var result = await SqlHelpers.NonQueryAsync(queryUpdate, connection);

                logger.Information($"Return value: {result}");

                return result;
            }
        }


        /// <summary>
        /// Create or retrieve an application user from a specified user object holding a GraphService Id.
        /// </summary>
        /// <param name="userdata">A user object holding a GraphService id which will be used to retrieve or create a user</param>
        /// <returns>The existing or newly created User object</returns>
        public async Task<User> GetOrCreateApplicationUser(User userdata)
        {

            Serilog.Context.LogContext.PushProperty("Method","GetOrCreateApplicationUser");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter userdata={userdata}");

            string selectQuery = $"SELECT UserId, NameId, UserName, Email, Bio FROM Users WHERE UserId='{userdata.Id}'";

            logger.Information($"Running the following query: {selectQuery}");

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
                        logger.Information("Returning existing user");
                        // Exists: return existing application user object from database
                        return rows
                              .Select(Mapper.UserFromDataRow)
                              .FirstOrDefault();
                    }

                    // Not exists: create Application user based on MicrosoftGraphService user
                    // Get a new id for the display name
                    string displayName = userdata.DisplayName.Split('/')[0].Trim();

                    logger.Information($"displayName has been determined as {displayName}");

                    uint? newNameId = DetermineNewNameId(displayName, connection);

                    logger.Information($"newNameId has been determined as {newNameId}");

                    // Exit if name id is null
                    if (newNameId == null) return null;

                    // Create and execute query
                    string insertQuery = $"INSERT INTO Users (UserId, NameId, UserName, Email) "
                                        + $"VALUES ('{userdata.Id}', {newNameId}, '{displayName}', '{userdata.Mail}')";

                    logger.Information($"Running the following query: {insertQuery}");

                    await SqlHelpers.NonQueryAsync(insertQuery, connection);

                    // Return the new application user, mapped directly from MSGraph

                    userdata.NameId = Convert.ToUInt32(newNameId);

                    var result = Mapper.UserFromMSGraph(userdata);

                    logger.Information("Returning new user");
                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}");

                logger.Information($"Return value: null");

                return null;
            }
        }


        /// <summary>
        /// Delete the user with the specified userId.
        /// </summary>
        /// <param name="userId">The id of the user, whose data will be updated</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> DeleteUser(string userId)
        {

            Serilog.Context.LogContext.PushProperty("Method","DeleteUser");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}");

            string query = $"DELETE FROM Users WHERE UserId='{userId}';";

            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }

        #region Helpers

        /// <summary>
        /// Construct a User object from data that belongs to the user identified by userId.
        /// </summary>
        /// <param name="userid">The id of the user to retrieve</param>
        /// <returns></returns>
        public async Task<User> GetUser(string userId)
        {
            Serilog.Context.LogContext.PushProperty("Method","GetUser");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}");

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string selectQuery = $"SELECT UserId, NameId, UserName, Email, Bio FROM Users WHERE UserId='{userId}'";

                    logger.Information($"Running the following query: {selectQuery}");

                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection);

                    var rows = SqlHelpers.GetRows("User", adapter);

                    if (rows.Count() == 0)
                    {
                        logger.Information($"Return value: null");

                        return null;
                    }

                    var result = rows.Select(Mapper.UserFromDataRow).First();

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}");

                logger.Information($"Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Determine a usernames new NameId.
        /// </summary>
        /// <param name="username">A username whose nameid is the be determined</param>
        /// <param name="connection">A connection to the sql database</param>
        ///<returns>Null on database errors, the appropriate NameId otherwise</returns>
        private uint? DetermineNewNameId(string username, SqlConnection connection)
        {
            Serilog.Context.LogContext.PushProperty("Method","DetermineNewNameId");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters username={username},connection={connection}");


            string query = $"SELECT MAX(NameId) FROM USERS WHERE UserName='{username}'";
            try
            {
                logger.Information($"Running the following query: {query}");

                SqlCommand scalarQuery = new SqlCommand(query, connection);

                // Will be System.DBNull if there is no other user with the same name
                var result = scalarQuery.ExecuteScalar();
                result = result is DBNull ? 0 : Convert.ToUInt32(result) + 1;

                logger.Information($"Return value: {result}");

                return (uint?)result;
            }
            catch (SqlException e)
            {
                Debug.WriteLine($"Database Exception: {e.Message}");

                logger.Information($"Return value: null");

                return null;
            }
        }

        #endregion
    }
}
