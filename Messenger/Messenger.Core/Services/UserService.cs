using System;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Helpers;
using System.Data;
using System.Linq;
using Serilog.Context;
using System.Collections.Generic;

namespace Messenger.Core.Services
{
    public class UserService : AzureServiceBase
    {
        // TODO: Maybe use an enum of values like User.Name?
        // TODO: Prevent changing user id
        /// <summary>
        /// Update a specified column for a specified user.
        ///</summary>
        /// <param name="userId">The id of the user, whose data will be updated</param>
        /// <param name="columnToChange">The column to update for the user</param>
        /// <param name="newVal">The new value for the specifed column for the specified user</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public async Task<bool> Update(string userId, string columnToChange, string newVal)
        {
            LogContext.PushProperty("Method","Update");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

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
            LogContext.PushProperty("Method","UpdateUsername");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

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
        /// Update a specified users email
        ///</summary>
        /// <param name="userId">The id of the user, whose email will be updated</param>
        /// <param name="newMail">The new email to set</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> UpdateUserMail(string userId, string newMail)
        {
            Serilog.Context.LogContext.PushProperty("Method","UpdateUserMail");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, newMail={newMail}");

            // TODO: Validate email
            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string queryUpdate = $"UPDATE Users SET Email='{newMail}' WHERE UserId='{userId}';";

                logger.Information($"Running the following query: {queryUpdate}");

                var result = await SqlHelpers.NonQueryAsync(queryUpdate, connection);

                logger.Information($"Return value: {result}");

                return result;
            }
        }

        /// <summary>
        /// Update a specified users photo
        ///</summary>
        /// <param name="userId">The id of the user, whose photo will be updated</param>
        /// <param name="newPhoto">The new photo to set</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> UpdateUserPhoto(string userId, string newPhoto)
        {
            Serilog.Context.LogContext.PushProperty("Method","UpdateUserMail");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, newPhoto={newPhoto}");

            //TODO: Check for valid photo
            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string queryUpdate = $"UPDATE Users SET PhotoURL='{newPhoto}' WHERE UserId='{userId}';";

                logger.Information($"Running the following query: {queryUpdate}");

                var result = await SqlHelpers.NonQueryAsync(queryUpdate, connection);

                logger.Information($"Return value: {result}");

                return result;
            }
        }


        /// <summary>
        /// Update a specified users bio
        ///</summary>
        /// <param name="userId">The id of the user, whose  biowill be updated</param>
        /// <param name="newBio">The new  bioto set</param>
        /// <returns>True if no exceptions occured while executing the query, false otherwise</returns>
        public async Task<bool> UpdateUserBio(string userId, string newBio)
        {
            Serilog.Context.LogContext.PushProperty("Method","UpdateUserMail");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}, newBio={newBio}");

            //TODO: Check for valid photo
            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                string queryUpdate = $"UPDATE Users SET Bio='{newBio}' WHERE UserId='{userId}';";

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

            LogContext.PushProperty("Method","GetOrCreateApplicationUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameter userdata={userdata}");

            string selectQuery = $"SELECT UserId, NameId, UserName, Email, PhotoURL, Bio FROM Users WHERE UserId='{userdata.Id}'";

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

                    Serilog.Context.LogContext.PushProperty("Method","GetOrCreateApplicationUser");
                    Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

                    logger.Information($"newNameId has been determined as {newNameId}");

                    // Exit if name id is null
                    if (newNameId == null)
                    {
                        logger.Information("newNameId has value null, returning null");

                        return null;
                    }

                    // Create and execute query
                    string insertQuery = $"INSERT INTO Users (UserId, NameId, UserName, Email, PhotoURL, Bio) "
                                        + $"VALUES ('{userdata.Id}', {newNameId}, '{displayName}', '{userdata.Mail}', '{userdata.Photo}', '{userdata.Bio}')";

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
                logger.Information(e, $"Return value: null");

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

            LogContext.PushProperty("Method","DeleteUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

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
        /// <returns>A full User object</returns>
        public async Task<User> GetUser(string userId)
        {
            LogContext.PushProperty("Method","GetUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userId={userId}");

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string selectQuery = $"SELECT UserId, NameId, UserName, Email, PhotoURL, Bio FROM Users WHERE UserId='{userId}'";

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
                logger.Information(e, $"Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Construct a User object from a given UserName and NameId
        /// </summary>
        /// <param name="userName">The Name of the user to retrieve</param>
        /// <param name="nameId">The NameId of the user to retrieve</param>
        /// <returns>A full User object</returns>
        public async Task<IList<User>> GetUser(string userName, uint nameId)
        {
            LogContext.PushProperty("Method","GetUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userName={userName}, nameId={nameId}");

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string selectQuery = $"SELECT UserId, NameId, UserName, Email, Bio FROM Users WHERE UserName='{userName}' AND NameId={nameId}";

                    logger.Information($"Running the following query: {selectQuery}");

                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection);

                    var result = SqlHelpers.MapToList(Mapper.UserFromDataRow, adapter);

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, $"Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Retrieve the UserNames and NameIds of the top 10 matches for the given
        /// userName
        /// </summary>
        /// <param name="userName">User name to retrieve matches for</param>
        /// <returns>List of top 10 matched User names</returns>
        public async Task<IList<string>> SearchUser(string userName)
        {
            LogContext.PushProperty("Method","SearchUser");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters userName={userName}");

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string selectQuery = $"SELECT CONCAT(UserName, '#', '00000' + RIGHT(NameId, 3)) AS UserNameWithNameId FROM Users WHERE LOWER(UserName) LIKE LOWER('%{userName}%') ORDER BY LEN(UserName);";

                    logger.Information($"Running the following query: {selectQuery}");

                    SqlDataAdapter adapter = new SqlDataAdapter(selectQuery, connection);

                    var rows = SqlHelpers.GetRows("Users", adapter);

                    LogContext.PushProperty("Method","SearchUser");
                    LogContext.PushProperty("SourceContext", this.GetType().Name);

                    logger.Information($"Retrieved {rows.Count()} rows");

                    var result = rows.Select(row => Convert.ToString(row["UserNameWithNameId"])).ToList();
                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, $"Return value: null");

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
            LogContext.PushProperty("Method","DetermineNewNameId");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

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

                return (uint)result;
            }
            catch (SqlException e)
            {
                logger.Information(e, $"Return value: null");

                return null;
            }
        }

        #endregion
    }
}
