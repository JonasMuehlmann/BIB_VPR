using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public class TeamService : AzureServiceBase
    {
        #region Teams Management

        /// <summary>
        /// Creates a team with the given name and description and retrieve the new team's id.
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>The id of the created team if it was created successfully, null otherwise</returns>
        public async Task<uint?> CreateTeam(string teamName, string teamDescription = "")
        {
            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamName={teamName}, teamDescription={teamDescription}");

            if (teamName == string.Empty)
            {
                logger.Information("Return value: null");

                return null;
            }

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = $"INSERT INTO Teams (TeamName, TeamDescription, CreationDate) VALUES "
                                 + $"('{teamName}', '{teamDescription}', GETDATE()); SELECT SCOPE_IDENTITY();";

                    SqlCommand scalarQuery = new SqlCommand(query, connection);


                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.TryConvertDbValue(scalarQuery.ExecuteScalar(),
                                                              Convert.ToUInt32);

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, "Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Deletes a team with a given team id.
        /// </summary>
        /// <param name="teamId">The id of the team to delete</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one query, false otherwise</returns>
        public async Task<bool> DeleteTeam(uint teamId)
        {
            string query = $"DELETE FROM Memberships WHERE TeamId={teamId};"
                         + $"DELETE FROM Teams WHERE TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }

        /// <summary>
        /// Gets the list of all existing teams.
        /// </summary>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllTeams()
        {
            LogContext.PushProperty("Method","GetAllTeams");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called");


            string query = @"SELECT TeamId, TeamName, TeamDescription, CreationDate FROM Teams;";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers .MapToList(Mapper.TeamFromDataRow,
                                                       new SqlDataAdapter(query, connection));

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, "Return value: null");

                return null;
            }
        }

        /// <summary>
        /// Gets the team with the given team id
        /// </summary>
        /// <param name="teamId">Id of the team to retrieve</param>
        /// <returns>A complete Team object</returns>
        public async Task<Team> GetTeam(uint teamId)
        {
            LogContext.PushProperty("Method","GetTeam");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");

            string query = $"SELECT TeamId, TeamName, TeamDescription, CreationDate " +
                $"FROM Teams " +
                $"WHERE TeamId = {teamId};";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers
                                .GetRows("Teams", adapter)
                                .Select(Mapper.TeamFromDataRow)
                                .FirstOrDefault();

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
        /// Gets the list of teams the user has a membership of.
        /// </summary>
        /// <param name="userId">The id of the user whose teams to list</param>
        /// <returns>An enumerable of Team objects</returns>
        public async Task<IEnumerable<Team>> GetAllTeamsByUserId(string userId)
        {
            LogContext.PushProperty("Method","GetAllTeamsByUserId");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}");

            string query = $"SELECT t.TeamId, t.TeamName, t.TeamDescription, t.CreationDate " +
                $"FROM Teams t " +
                $"LEFT JOIN Memberships m ON (t.TeamId = m.TeamId) " +
                $"WHERE m.UserId = '{userId}';";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.MapToList(Mapper.TeamFromDataRow, new SqlDataAdapter(query, connection));

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

        #endregion

        #region Members Management

        /// <summary>
        /// Adds a new member to the team
        /// </summary>
        /// <param name="userId">The id of the user to add to the specified team</param>
        /// <param name="teamId">The id of the team to add the specified user to</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public async Task<bool> AddMember(string userId, uint teamId)
        {
            LogContext.PushProperty("Method","AddMember");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            string query = $"INSERT INTO Memberships(UserId, TeamId, UserRole) VALUES('{userId}', {teamId}, 'placeholder');";


            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Removes a member from the team
        /// </summary>
        /// <param name="userId">The id of the user to remove from the specified team</param>
        /// <param name="teamId">The id of the team to remove the specified user from</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public async Task<bool> RemoveMember(string userId, uint teamId)
        {
            LogContext.PushProperty("Method","RemoveMember");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            string query = $"DELETE FROM Memberships WHERE UserId='{userId}' AND TeamId={teamId};";

            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Gets all memberships of a user
        /// </summary>
        /// <param name="userId">User id for the current user</param>
        /// <returns>A list of membership objects</returns>
        public async Task<IList<Membership>> GetAllMembershipByUserId(string userId)
        {
            LogContext.PushProperty("Method","GetAllMembershipByUserId");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}");

            string query = $"SELECT * FROM Memberships WHERE UserId='{userId}'";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.MapToList(Mapper.MembershipFromDataRow,
                                                new SqlDataAdapter(query, connection));

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
        /// Retrieve all user who are members of the specified team
        /// </summary>
        /// <param name="teamId">The id of a team to retrieve users from</param>
        ///<returns>Enumerable of User objects representing the teams members</returns>
        public async Task<IEnumerable<User>> GetAllMembers(uint teamId)
        {
            LogContext.PushProperty("Method","GetAllMembers");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");

            string subquery = $"SELECT UserId FROM Memberships WHERE TeamId={teamId}";
            string query = $"SELECT * FROM Users WHERE UserId IN ({subquery})";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.MapToList(Mapper.UserFromDataRow, new SqlDataAdapter(query, connection));

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
        /// Retrieve all channels of a team
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve channels from</param>
        /// <returns>A list of channel objects</returns>
        public async Task<IList<Channel>> GetAllChannelsByTeamId(uint teamId)
        {
            Serilog.Context.LogContext.PushProperty("Method","GetAllChannelsByTeamId");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");
            string query = $"SELECT ChannelId, ChannelName, TeamId FROM Channels WHERE TeamId={teamId};";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    logger.Information($"Running the following query: {query}");
                    var result = SqlHelpers.MapToList(Mapper.ChannelFromDataRow, new SqlDataAdapter(query, connection));

                    // NOTE: This is needed for the below log line to have the correct properties
                    Serilog.Context.LogContext.PushProperty("Method","GetAllChannelsByTeamId");
                    Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

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
        ///	Add a role to a team with the specified teamId
        /// </summary>
        /// <param name="role">The name of the role to add</param>
        /// <param name="teamId">The id of the team to add the role to</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AddRole(string role, uint teamId)
        {

            LogContext.PushProperty("Method","AddRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            string query = $"UPDATE Teams SET Roles= Roles + IIF(LEN(Roles) = 0, ',{role}', '{role}');";


            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	Assign a team's member a role
        /// </summary>
        /// <param name="role">The name of the role to assign to the user</param>
        /// <param name="userId">The id of the user to assign the role to</param>
        /// <param name="teamId">The team to assign a role to a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AssignRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            string query = $"UPDATE Memberships Set Role ='{role}' WHERE teamId={teamId} AND userId='{userId}';";


            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	Unassign a team's member a role
        /// </summary>
        /// <param name="role">The name of the role to unassign from the user</param>
        /// <param name="userId">The id of the user to unassign the role from</param>
        /// <param name="teamId">The team to unassign a role from a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UnAssignRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            string query = $"UPDATE Memberships Set Role ='' WHERE teamId={teamId} AND userId='{userId}';";


            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	List all roles available in a specified team
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve roles from</param>
        /// <returns>A list of available role names</returns>
        IList<string> ListRoles(uint teamId)
        {

            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameter teamId={teamId}");

            string query = $"SELECT Role FROM Memberships WHERE teamId={teamId};";


            logger.Information($"Running the following query: {query}");

            var result = SqlHelpers.MapToList(Convert.ToString, new SqlDataAdapter(query, GetConnection()));

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        ///	Retrieve a team's users that have a specified role
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve users from</param>
        /// <param name="role">The role of users to retrieve from the team</param>
        /// <returns></returns>
        IList<User> GetUsersWithRole(uint teamId, string role)
        {

            LogContext.PushProperty("Method","GetUsersWithRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}, role={role}");

            string query = $"SELECT * FROM Users s WHERE (s.UserId IN SELECT m.UserId FROM Memberships m WHERE m.teamId={teamId});";


            logger.Information($"Running the following query: {query}");

            var result = SqlHelpers.MapToList(Mapper.UserFromDataRow, new SqlDataAdapter(query, GetConnection()));

            logger.Information($"Return value: {result}");

            return result;
        }
        #endregion
    }
}
