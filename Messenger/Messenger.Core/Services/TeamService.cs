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

                    string query = $@"
                                        INSERT INTO Teams (TeamName, TeamDescription, CreationDate)
                                        VALUES ('{teamName}', '{teamDescription}', GETDATE());
                                        SELECT SCOPE_IDENTITY();";

                    SqlCommand scalarQuery = new SqlCommand(query, connection);


                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.TryConvertDbValue(scalarQuery.ExecuteScalar(),
                                                              Convert.ToUInt32);

                    LogContext.PushProperty("Method","CreateTeam");
                    LogContext.PushProperty("SourceContext", this.GetType().Name);

                    var createEmptyRoleQuery = $"INSERT INTO Team_roles VALUES('', {result});";

                    logger.Information($"Running the following query: {createEmptyRoleQuery}");
                    logger.Information($"Result value: {await SqlHelpers.NonQueryAsync(createEmptyRoleQuery, connection)}");

                    LogContext.PushProperty("Method","CreateTeam");
                    LogContext.PushProperty("SourceContext", this.GetType().Name);

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
            string query = $@"
                                DELETE Role_permissions
                                    FROM Role_permissions rp
                                INNER JOIN Team_roles tr
                                    ON tr.Id = rp.Team_rolesId
                                WHERE tr.TeamId={teamId};

                                DELETE FROM User_roles       WHERE TeamId={teamId};
                                DELETE FROM Team_roles       WHERE TeamId={teamId};
                                DELETE FROM Memberships      WHERE TeamId={teamId};
                                DELETE FROM Teams            WHERE TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query, GetConnection());
        }


        /// <summary>
        /// Change the specified teams name
        /// </summary>
        /// <param name="teamId">Id of the team which's should be changed</param>
        /// <param name="teamName">New name of the team</param>
        /// <returns>True, if the teams name was changed, false otherwise</returns>
        public async Task<bool> ChangeTeamName(uint teamId, string teamName)
        {

            Serilog.Context.LogContext.PushProperty("Method","ChangeTeamName");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamId={teamId}, teamName={teamName}");

            if (teamName == string.Empty)
            {
                logger.Information("Return value: false");

                return false;
            }

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = $"UPDATE Teams SET TeamName='{teamName}' WHERE TeamId={teamId};";

                    SqlCommand scalarQuery = new SqlCommand(query, connection);


                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.TryConvertDbValue(scalarQuery.ExecuteNonQuery(),
                                                          Convert.ToBoolean);

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, "Return value: false");

                return false;
            }
        }

        /// <summary>
        /// Change the specified teams description
        /// </summary>
        /// <param name="teamId">Id of the team which's should be changed</param>
        /// <param name="description">New description of the team</param>
        /// <returns>True, if the teams name was changed, false otherwise</returns>
        public async Task<bool> ChangeTeamDescription(uint teamId, string description)
        {

            Serilog.Context.LogContext.PushProperty("Method","ChangeTeamDescription");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Function called with parameters teamId={teamId}, description={description}");

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    string query = $"UPDATE Teams SET TeamDescription='{description}' WHERE TeamId={teamId};";

                    SqlCommand scalarQuery = new SqlCommand(query, connection);


                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.TryConvertDbValue(scalarQuery.ExecuteNonQuery(),
                                                              Convert.ToBoolean);

                    logger.Information($"Return value: {result}");

                    return result;
                }
            }
            catch (SqlException e)
            {
                logger.Information(e, "Return value: false");

                return false;
            }
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

            string query = $@"
                                SELECT
                                    TeamId, TeamName, TeamDescription, CreationDate
                                FROM Teams
                                WHERE
                                    TeamId = {teamId};";

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

            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='' AND TeamId={teamId}";
                var Team_rolesIdCmd = new SqlCommand(Team_rolesIdQuery, connection);

                logger.Information($"Running the following query: {Team_rolesIdQuery}");
                var Team_rolesId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                var User_rolesIdQuery = $@"
                                            INSERT INTO User_roles(UserId, Team_rolesId, teamId)
                                                VALUES('{userId}', {Team_rolesId}, {teamId});";

                var User_rolesIdCmd = new SqlCommand(User_rolesIdQuery, connection);
                logger.Information($"Running the following query: {User_rolesIdQuery}");
                var User_rolesId = SqlHelpers.TryConvertDbValue(User_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                string query = $@"  INSERT INTO Memberships(UserId, TeamId)
                                        VALUES('{userId}', {teamId});";


                logger.Information($"Running the following query: {query}");

                var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

                logger.Information($"Return value: {result}");

                return result;
            }
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

            string query = $@"
                                DELETE FROM
                                    Memberships
                                WHERE
                                    UserId='{userId}'
                                    AND
                                    TeamId={teamId};

                                DELETE FROM
                                    User_roles
                                WHERE
                                    UserId='{userId}'
                                    AND
                                    TeamId={teamId};";

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
        /// Add a role to a team with the specified teamId
        /// </summary>
        /// <param name="role">The name of the role to add</param>
        /// <param name="teamId">The id of the team to add the role to</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> AddRole(string role, uint teamId)
        {
            // TODO: Prevent adding duplicate roles
            LogContext.PushProperty("Method","AddRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            string query = $@"
                                INSERT INTO Team_Roles
                                    VALUES('{role}', {teamId});
                                ";


            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());
            LogContext.PushProperty("Method","AddRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Remove a role from a team's available roles
        /// </summary>
        /// <param name="role">The name of the role to remove</param>
        /// <param name="teamId">The id of the team to remove the role from</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RemoveRole(string role, uint teamId)
        {
            // TODO: Prevent adding duplicate roles
            LogContext.PushProperty("Method","RemoveRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            string query = $@"
                                DELETE FROM Team_roles
                                WHERE
                                    TeamId={teamId}
                                    AND
                                    Role='{role}'";

            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());
            LogContext.PushProperty("Method","RemoveRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Assign a team's member a role
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

            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                // TODO: Write function to retrieve id of role in team
                var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";
                var Team_rolesIdCmd = new SqlCommand(Team_rolesIdQuery, connection);
                var Team_rolesId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","AssignRole");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                if (Team_rolesId == null)
                {
                    logger.Information($"could not retrieve the Team_rolesId");

                    logger.Information("Return value: false");

                    return false;
                }
                string query = $@"
                                    INSERT INTO User_roles
                                    VALUES('{userId}', {Team_rolesId}, {teamId});";

                logger.Information($"Running the following query: {query}");

                var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

                LogContext.PushProperty("Method","AssignRole");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                logger.Information($"Return value: {result}");

                return result;
            }
        }

        /// <summary>
        /// Unassign a team's member a role
        /// </summary>
        /// <param name="role">The name of the role to unassign from the user</param>
        /// <param name="userId">The id of the user to unassign the role from</param>
        /// <param name="teamId">The team to unassign a role from a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> UnAssignRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method","UnassignRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";
                var Team_rolesIdCmd = new SqlCommand(Team_rolesIdQuery, connection);
                var Team_rolesId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","UnassignRole");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                if (Team_rolesId == null)
                {
                    logger.Information($"could not retrieve the Team_rolesId");

                    logger.Information("Return value: false");

                    return false;
                }
                string query = $@"
                                    DELETE FROM
                                        User_roles
                                    WHERE
                                        Team_rolesId = {Team_rolesId}
                                        AND
                                        TeamId = {teamId};";

                logger.Information($"Running the following query: {query}");

                var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

                LogContext.PushProperty("Method","UnassignRole");
                LogContext.PushProperty("SourceContext", this.GetType().Name);


                logger.Information($"Return value: {result}");

                return result;
            }
        }

        /// <summary>
        /// List all roles available in a specified team
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve roles from</param>
        /// <returns>A list of available role names</returns>
        public IList<string> ListRoles(uint teamId)
        {
            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameter teamId={teamId}");

            string query = $"SELECT Role FROM Team_roles WHERE teamId={teamId} AND Role != '';";


            logger.Information($"Running the following query: {query}");

            var result = SqlHelpers.MapToList(Mapper.StringFromDataRow, new SqlDataAdapter(query, GetConnection()), "Team_roles", "Role");

            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Retrieve a team's users that have a specified role
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve users from</param>
        /// <param name="role">The role of users to retrieve from the team</param>
        /// <returns>A list of user objects belonging to users with the specified role</returns>
        public IList<User> GetUsersWithRole(uint teamId, string role)
        {
            LogContext.PushProperty("Method","GetUsersWithRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}, role={role}");

            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Users s
                                INNER JOIN User_roles ur
                                    ON ur.UserId = s.UserId
                                INNER JOIN Team_roles tr
                                    ON tr.Id = ur.Team_rolesId
                                WHERE
                                    ur.TeamId = {teamId}
                                    AND
                                    tr.Role = '{role}';";


            logger.Information($"Running the following query: {query}");

            var result = SqlHelpers.MapToList(Mapper.UserFromDataRow, new SqlDataAdapter(query, GetConnection()));

            LogContext.PushProperty("Method","GetUsersWithRole");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Retrieve a users roles in a specified team
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve users from</param>
        /// <param name="userId">The id of the user to retrieve roles from</param>
        /// <returns>The list of role names of the user in the specified team</returns>
        public IList<string> GetUsersRoles(uint teamId, string userId)
        {
            LogContext.PushProperty("Method","GetUsersRoles");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}, userId={userId}");

            string query = $@"
                                SELECT
                                    Role
                                FROM
                                    Team_roles tr
                                INNER JOIN
                                    User_roles ur
                                    ON ur.Team_rolesId = tr.Id
                                WHERE
                                    ur.UserId = '{userId}'
                                    AND
                                    ur.TeamId = {teamId}
                                    AND tr.Role != '';";


            logger.Information($"Running the following query: {query}");

            var result = SqlHelpers.MapToList(Mapper.StringFromDataRow, new SqlDataAdapter(query, GetConnection()), "Team_Roles", "Role");

            LogContext.PushProperty("Method","GetUsersRoles");
            LogContext.PushProperty("SourceContext", this.GetType().Name);

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Grant a team's role a specified permissions
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to grant a permission</param>
        /// <param name="permissions">The permission to grant a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public async Task<bool> GrantPermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method","GrantPermissions");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}, role={role}, permission={permission}");


            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";
                var Team_rolesIdCmd = new SqlCommand(Team_rolesIdQuery, connection);

                logger.Information($"Running the following query: {Team_rolesIdQuery}");
                var Team_rolesId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","GrantPermissions");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                if (Team_rolesId == null)
                {
                    logger.Information($"could not retrieve the Team_rolesId");

                    logger.Information("Return value: false");

                    return false;
                }

                var PermissionsIdQuery= $@"
                                            SELECT
                                                Id
                                            FROM
                                                Permissions
                                            WHERE
                                                Permissions = '{Enum.GetName(typeof(Permissions),permission)}'";

                var PermissionsIdCmd= new SqlCommand(PermissionsIdQuery, connection);

                logger.Information($"Running the following query: {PermissionsIdCmd}");
                var PermissionsId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","GrantPermissions");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                if (PermissionsId== null)
                {
                    logger.Information($"could not retrieve the PermissionsId");

                    logger.Information("Return value: false");

                    return false;
                }

                string query = $@"
                                    INSERT INTO
                                        Role_permissions
                                    VALUES({PermissionsId}, {Team_rolesId});";

                logger.Information($"Running the following query: {query}");
                var result = await SqlHelpers.NonQueryAsync(query, connection);

                LogContext.PushProperty("Method","GrantPermissions");
                LogContext.PushProperty("SourceContext", this.GetType().Name);


                logger.Information($"Return value: {result}");

                return result;
            }
        }

        /// <summary>
        /// Revoke a permission from a specified team's role
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to revoke a permission from</param>
        /// <param name="permissions">The permission to revoke from a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public async Task<bool> RevokePermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method","RevokePermission");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}, role={role}, permission={permission}");


            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='' AND TeamId={teamId}";
                var Team_rolesIdCmd = new SqlCommand(Team_rolesIdQuery, connection);

                logger.Information($"Running the following query: {Team_rolesIdQuery}");
                var Team_rolesId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","RevokePermission");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                if (Team_rolesId == null)
                {
                    logger.Information($"could not retrieve the Team_rolesId");

                    logger.Information("Return value: false");

                    return false;
                }
                var PermissionsIdQuery= $@"
                                            SELECT
                                                Id
                                            FROM
                                                Permissions
                                            WHERE
                                                Permissions = '{Enum.GetName(typeof(Permissions), permission)}'";

                var PermissionsIdCmd = new SqlCommand(PermissionsIdQuery, connection);

                logger.Information($"Running the following query: {PermissionsIdCmd}");
                var PermissionsId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","RevokePermission");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                if (PermissionsId == null)
                {
                    logger.Information($"could not retrieve the PermissionsId");

                    logger.Information("Return value: false");

                    return false;
                }
                string query = $@"
                                    Delete FROM
                                        Role_permissions
                                    WHERE
                                        PermissionsId = {PermissionsId}
                                        AND
                                        Team_rolesId = {Team_rolesId};";

                logger.Information($"Running the following query: {query}");
                var result = await SqlHelpers.NonQueryAsync(query, connection);

                LogContext.PushProperty("Method","RevokePermission");
                LogContext.PushProperty("SourceContext", this.GetType().Name);


                logger.Information($"Return value: {result}");

                return result;
            }
        }

        /// <summary>
        /// Check if a role has a permission in a team
        /// </summary>
        /// <param name="teamId">The id of the team check permissions in</param>
        /// <param name="role">The role of the team to check permission</param>
        /// <param name="permissions">The permission to check for a team's role</param>
        /// <returns>True if the role has the permission, false otherwise</returns>
        public async Task<bool> HasPermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method","HasPermission");
            LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}, role={role}, permission={permission}");


            using (SqlConnection connection = GetConnection())
            {
                await connection.OpenAsync();

                var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='' AND TeamId={teamId}";
                var Team_rolesIdCmd = new SqlCommand(Team_rolesIdQuery, connection);

                logger.Information($"Running the following query: {Team_rolesIdQuery}");
                var Team_rolesId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","HasPermission");
                LogContext.PushProperty("SourceContext", this.GetType().Name);

                if (Team_rolesId == null)
                {
                    logger.Information($"could not retrieve the Team_rolesId");

                    logger.Information("Return value: false");

                    return false;
                }

                var PermissionsIdQuery= $@"
                                            SELECT
                                                Id
                                            FROM
                                                Permissions
                                            WHERE
                                                Permissions = '{Enum.GetName(typeof(Permissions),permission)}'";

                var PermissionsIdCmd= new SqlCommand(PermissionsIdQuery, connection);

                logger.Information($"Running the following query: {PermissionsIdCmd}");
                var PermissionsId = SqlHelpers.TryConvertDbValue(Team_rolesIdCmd.ExecuteScalar(), Convert.ToUInt32);

                LogContext.PushProperty("Method","HasPermission");
                LogContext.PushProperty("SourceContext", this.GetType().Name);


                if (PermissionsId == null)
                {
                    logger.Information($"could not retrieve the PermissionsId");

                    logger.Information("Return value: false");

                    return false;
                }

                string query = $@"
                                    SELECT
                                        COUNT(*)
                                    FROM
                                        Role_permissions
                                    WHERE
                                        PermissionsId = {PermissionsId}
                                        AND
                                        Team_rolesId = {Team_rolesId};";

                var cmd = new SqlCommand(query, connection);

                logger.Information($"Running the following query: {query}");
                var result = SqlHelpers.TryConvertDbValue(cmd.ExecuteScalar(), Convert.ToBoolean);

                LogContext.PushProperty("Method","HasPermission");
                LogContext.PushProperty("SourceContext", this.GetType().Name);


                logger.Information($"Return value: {result}");

                return result;
            }
        }

        #endregion
    }
}
