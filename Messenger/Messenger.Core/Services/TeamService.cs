﻿using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Serilog.Context;

namespace Messenger.Core.Services
{
    /// <summary>
    /// Holds helpers and static methods to interact with teams on the DB,
    /// including permissions, roles and members.
    /// </summary>
    public class TeamService : AzureServiceBase
    {
        #region Teams Management

        /// <summary>
        /// Creates a team with the given name and description and retrieve the new team's id.
        /// </summary>
        /// <param name="teamName">Name of the team</param>
        /// <param name="teamDescription">Description of the team</param>
        /// <returns>The id of the created team if it was created successfully, null otherwise</returns>
        public static async Task<uint?> CreateTeam(string teamName, string teamDescription = "")
        {
            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", "TeamService");

            logger.Information($"Function called with parameters teamName={teamName}, teamDescription={teamDescription}");

            if (teamName == string.Empty)
            {
                logger.Information("Return value: null");

                return null;
            }

            string  createTeamQuery = $@"
                                            INSERT INTO
                                                Teams
                                            VALUES(
                                                    '{teamName}',
                                                    '{teamDescription}',
                                                    GETDATE()
                                            );

                                            SELECT SCOPE_IDENTITY();";

            var result = await SqlHelpers.ExecuteScalarAsync(createTeamQuery,
                                                                Convert.ToUInt32);

            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", "TeamService");

            var createEmptyRoleQuery = $@"
                                            INSERT INTO
                                                Team_roles
                                            VALUES(
                                                '',
                                                {result},
                                                'FFFFFF'
                                            );";

            logger.Information($"Result value: {await SqlHelpers.NonQueryAsync(createEmptyRoleQuery)}");

            LogContext.PushProperty("Method","CreateTeam");
            LogContext.PushProperty("SourceContext", "TeamService");

            return result;
        }

        /// <summary>
        /// Deletes a team with a given team id.
        /// </summary>
        /// <param name="teamId">The id of the team to delete</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one query, false otherwise</returns>
        public static async Task<bool> DeleteTeam(uint teamId)
        {
            LogContext.PushProperty("Method","DeleteTeam");
            LogContext.PushProperty("SourceContext", "TeamService");

            logger.Information($"Function called with parameters teamId={teamId}");

            string query = $@"
                                DELETE
                                    Role_permissions
                                FROM
                                    Role_permissions rp
                                INNER JOIN Team_roles tr
                                    ON tr.Id = rp.Team_rolesId
                                WHERE
                                    tr.TeamId={teamId};

                                DELETE FROM User_roles  WHERE TeamId={teamId};
                                DELETE FROM Team_roles  WHERE TeamId={teamId};
                                DELETE FROM Memberships WHERE TeamId={teamId};
                                DELETE FROM Teams       WHERE TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="teamName"></param>
        /// <param name="teamDescription"></param>
        /// <param name="teamId"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateTeam(string teamName, string teamDescription, uint teamId)
        {

            LogContext.PushProperty("Method", "UpdateTeam");
            LogContext.PushProperty("SourceContext", "TeamService");

            logger.Information($"Function called with parameters teamName={teamName}, teamDescription={teamDescription}, teamId={teamId}");


            string query = $@"
                                UPDATE
                                    Teams
                                SET
                                    TeamName='{teamName}',
                                    TeamDescription='{teamDescription}'
                                WHERE
                                    TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query);
        }


        /// <summary>
        /// Change the specified teams name
        /// </summary>
        /// <param name="teamId">Id of the team which's should be changed</param>
        /// <param name="teamName">New name of the team</param>
        /// <returns>True, if the teams name was changed, false otherwise</returns>
        public static async Task<bool> ChangeTeamName(uint teamId, string teamName)
        {

            LogContext.PushProperty("Method","ChangeTeamName");
            LogContext.PushProperty("SourceContext", "TeamService");

            logger.Information($"Function called with parameters teamId={teamId}, teamName={teamName}");

            if (teamName == string.Empty)
            {
                logger.Information("Return value: false");

                return false;
            }

            string query = $@"
                                UPDATE
                                    Teams
                                SET
                                    TeamName='{teamName}'
                                WHERE
                                    TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Change the specified teams description
        /// </summary>
        /// <param name="teamId">Id of the team which's should be changed</param>
        /// <param name="description">New description of the team</param>
        /// <returns>True, if the teams name was changed, false otherwise</returns>
        public static async Task<bool> ChangeTeamDescription(uint teamId, string description)
        {

            LogContext.PushProperty("Method","ChangeTeamDescription");
            LogContext.PushProperty("SourceContext", "TeamService");

            logger.Information($"Function called with parameters teamId={teamId}, description={description}");

            string query = $@"
                                UPDATE
                                    Teams
                                SET
                                    TeamDescription='{description}'
                                WHERE
                                    TeamId={teamId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Gets the list of all existing teams.
        /// </summary>
        /// <returns>An enumerable of Team objects</returns>
        public static async Task<IEnumerable<Team>> GetAllTeams()
        {
            LogContext.PushProperty("Method","GetAllTeams");
            LogContext.PushProperty("SourceContext", "TeamService");

            logger.Information($"Function called");


            string query = @"
                            SELECT
                                *
                            FROM
                                Teams;";

            return await SqlHelpers.MapToList(Mapper.TeamFromDataRow, query);
        }

        /// <summary>
        /// Gets the team with the given team id
        /// </summary>
        /// <param name="teamId">Id of the team to retrieve</param>
        /// <returns>A complete Team object</returns>
        public static async Task<Team> GetTeam(uint teamId)
        {
            LogContext.PushProperty("Method","GetTeam");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}");

            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Teams
                                WHERE
                                    TeamId = {teamId};";

            SqlDataAdapter adapter = new SqlDataAdapter(query, GetDefaultConnection());

            return (await SqlHelpers
                    .GetRows("Teams", query))
                    .Select(Mapper.TeamFromDataRow)
                    .FirstOrDefault();
        }

        /// <summary>
        /// Gets the list of teams the user has a membership of.
        /// </summary>
        /// <param name="userId">The id of the user whose teams to list</param>
        /// <returns>An enumerable of Team objects</returns>
        public static async Task<IEnumerable<Team>> GetAllTeamsByUserId(string userId)
        {
            LogContext.PushProperty("Method","GetAllTeamsByUserId");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters userId={userId}");

            string query = $@"
                                SELECT
                                    t.TeamId, t.TeamName, t.TeamDescription, t.CreationDate
                            FROM
                                Teams t
                            LEFT JOIN Memberships m
                                ON t.TeamId = m.TeamId
                            WHERE
                                m.UserId = '{userId}';";

            return await SqlHelpers.MapToList(Mapper.TeamFromDataRow, query);

        }

        #endregion

        #region Members Management

        /// <summary>
        /// Adds a new member to the team
        /// </summary>
        /// <param name="userId">The id of the user to add to the specified team</param>
        /// <param name="teamId">The id of the team to add the specified user to</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public static async Task<bool> AddMember(string userId, uint teamId)
        {
            LogContext.PushProperty("Method","AddMember");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            var hasMembershipQuery = $@"
                                            SELECT
                                                COUNT(*)
                                            FROM
                                                Memberships
                                            WHERE
                                                UserId='{userId}'
                                                AND
                                                TeamId = {teamId};
                ";
            var hasMembership = await SqlHelpers.ExecuteScalarAsync(hasMembershipQuery, Convert.ToBoolean);

            if (hasMembership)
            {
                return false;
            }

            var Team_rolesIdQuery = $@"
                                        SELECT
                                            Id
                                        FROM
                                            Team_roles
                                        WHERE
                                            Role = ''
                                            AND
                                            TeamId = {teamId}";
            var Team_rolesId = await SqlHelpers.ExecuteScalarAsync(Team_rolesIdQuery, Convert.ToUInt32);


            var User_rolesIdQuery = $@"
                                        INSERT INTO
                                            User_roles
                                        VALUES(
                                            '{userId}',
                                             {Team_rolesId},
                                             {teamId}
                                        );";
            var User_rolesId = await SqlHelpers.ExecuteScalarAsync(User_rolesIdQuery, Convert.ToUInt32);

            string query = $@"
                                INSERT INTO
                                    Memberships
                                VALUES(
                                    '{userId}',
                                     {teamId}
                                );";
            var result = await SqlHelpers.NonQueryAsync(query);

            return result;
        }

        /// <summary>
        /// Removes a member from the team
        /// </summary>
        /// <param name="userId">The id of the user to remove from the specified team</param>
        /// <param name="teamId">The id of the team to remove the specified user from</param>
        /// <returns>True if no exceptions occured while executing the query and it affected at least one entry, false otherwise</returns>
        public static async Task<bool> RemoveMember(string userId, uint teamId)
        {
            LogContext.PushProperty("Method","RemoveMember");
            LogContext.PushProperty("SourceContext", "TeamService");
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

            var result = await SqlHelpers.NonQueryAsync(query);

            return result;
        }

        /// <summary>
        /// Gets all memberships of a user
        /// </summary>
        /// <param name="userId">User id for the current user</param>
        /// <returns>A list of membership objects</returns>
        public static async Task<IList<Membership>> GetAllMembershipByUserId(string userId)
        {
            LogContext.PushProperty("Method","GetAllMembershipByUserId");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters userId={userId}");

            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Memberships
                                WHERE
                                    UserId='{userId}'";

            return await SqlHelpers.MapToList(Mapper.MembershipFromDataRow, query);

        }

        /// <summary>
        /// Retrieve all user who are members of the specified team
        /// </summary>
        /// <param name="teamId">The id of a team to retrieve users from</param>
        ///<returns>Enumerable of User objects representing the teams members</returns>
        public static async Task<IEnumerable<User>> GetAllMembers(uint teamId)
        {
            LogContext.PushProperty("Method","GetAllMembers");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}");

            string query = $@"
                            SELECT
                                *
                            FROM
                                Users
                            WHERE
                            UserId
                                IN (
                                    SELECT
                                        UserId
                                    FROM
                                        Memberships
                                    WHERE
                                        TeamId={teamId}
                                )";

            return await SqlHelpers.MapToList(Mapper.UserFromDataRow, query);
        }

        /// <summary>
        /// Retrieve all channels of a team
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve channels from</param>
        /// <returns>A list of channel objects</returns>
        public static async Task<IList<Channel>> GetAllChannelsByTeamId(uint teamId)
        {
            LogContext.PushProperty("Method","GetAllChannelsByTeamId");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}");
            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Channels
                                WHERE
                                    TeamId={teamId};";

            return await SqlHelpers.MapToList(Mapper.ChannelFromDataRow, query);
        }

        /// <summary>
        /// Add a role to a team with the specified teamId
        /// </summary>
        /// <param name="role">The name of the role to add</param>
        /// <param name="teamId">The id of the team to add the role to</param>
        /// <param name="colorCode">Hex code of the color</param>
        /// <returns>Id of the added team role</returns>
        public static async Task<uint?> AddRole(string role, uint teamId, string colorCode)
        {
            // TODO: Prevent adding duplicate roles
            LogContext.PushProperty("Method","AddRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            string query = $@"
                                INSERT INTO
                                    Team_Roles
                                VALUES(
                                    '{role}',
                                     {teamId},
                                    '{colorCode}'
                                );
                                SELECT SCOPE_IDENTITY();";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        public static async Task<bool> UpdateRole(uint roleId, string role, string colorCode)
        {
            // TODO: Prevent adding duplicate roles
            LogContext.PushProperty("Method", "UpdateRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters roleId={roleId}, role={role}, colorCode={colorCode}");

            string query = $@"
                                UPDATE
                                    Team_roles
                                SET
                                    Role='{role}',
                                    Color='{colorCode}'
                                WHERE
                                    Id={roleId};";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Remove a role from a team's available roles
        /// </summary>
        /// <param name="role">The name of the role to remove</param>
        /// <param name="teamId">The id of the team to remove the role from</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<uint?> RemoveRole(string role, uint teamId)
        {
            // TODO: Prevent adding duplicate roles
            LogContext.PushProperty("Method","RemoveRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters role={role}, teamId={teamId}");

            string query = $@"
                                DELETE FROM
                                    Team_roles
                                WHERE
                                    TeamId={teamId}
                                    AND
                                    Role='{role}';
                                SELECT SCOPE_IDENTITY();";

            return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
        }

        /// <summary>
        /// Build a role object from a specified roleId
        /// </summary>
        /// <param name="roleId">The id of the role to build an object from</param>
        /// <returns>The built role object on success, null otherwise</returns>
        public static async Task<TeamRole> GetRole(uint roleId)
        {
            // TODO: Prevent adding duplicate roles
            LogContext.PushProperty("Method","GetRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters roleId={roleId}");


            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Team_roles
                                WHERE
                                    ID = {roleId};
                ";

            var rows = await SqlHelpers.GetRows("Team_roles", query);

            if (rows.Count() == 0)
            {
                logger.Information($"Return value: null");

                return null;
            }

            return rows.Select(Mapper.TeamRoleFromDataRow).First();
        }

        /// <summary>
        /// Assign a team's member a role
        /// </summary>
        /// <param name="role">The name of the role to assign to the user</param>
        /// <param name="userId">The id of the user to assign the role to</param>
        /// <param name="teamId">The team to assign a role to a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> AssignRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            // TODO: Write function to retrieve id of role in team
            var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";
            var Team_rolesId = await SqlHelpers.ExecuteScalarAsync(Team_rolesIdQuery, Convert.ToUInt32);

            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", "TeamService");

            if (Team_rolesId == 0)
            {
                logger.Information($"Could not retrieve the Team_rolesId");

                logger.Information("Return value: false");

                return false;
            }
            string query = $@"
                                INSERT INTO
                                    User_roles
                                VALUES(
                                    '{userId}',
                                     {Team_rolesId},
                                     {teamId}
                                );";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Unassign a team's member a role
        /// </summary>
        /// <param name="role">The name of the role to unassign from the user</param>
        /// <param name="userId">The id of the user to unassign the role from</param>
        /// <param name="teamId">The team to unassign a role from a member in</param>
        /// <returns>True if successful, false otherwise</returns>
        public static async Task<bool> UnAssignRole(string role, string userId, uint teamId)
        {
            LogContext.PushProperty("Method","UnassignRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters role={role}, userId={userId}, teamId={teamId}");

            var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";
            var Team_rolesId = await SqlHelpers.ExecuteScalarAsync(Team_rolesIdQuery, Convert.ToUInt32);

            LogContext.PushProperty("Method","UnassignRole");
            LogContext.PushProperty("SourceContext", "TeamService");

            if (Team_rolesId == 1)
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

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// List all roles available in a specified team
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve roles from</param>
        /// <returns>A list of available role names</returns>
        public static async Task<IList<TeamRole>> ListRoles(uint teamId)
        {
            LogContext.PushProperty("Method","AssignRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameter teamId={teamId}");

            string query = $@"
                                SELECT
                                    *
                                FROM
                                    Team_roles
                                WHERE
                                    teamId={teamId}
                                    AND
                                    Role != '';
                    ";

            return await SqlHelpers.MapToList(Mapper.TeamRoleFromDataRow, query, "Team_roles");
        }

        /// <summary>
        /// Retrieve a team's users that have a specified role
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve users from</param>
        /// <param name="role">The role of users to retrieve from the team</param>
        /// <returns>A list of user objects belonging to users with the specified role</returns>
        public static async Task<IList<User>> GetUsersWithRole(uint teamId, string role)
        {
            LogContext.PushProperty("Method","GetUsersWithRole");
            LogContext.PushProperty("SourceContext", "TeamService");
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

            return await SqlHelpers.MapToList(Mapper.UserFromDataRow, query);
        }

        /// <summary>
        /// Retrieve a users roles in a specified team
        /// </summary>
        /// <param name="teamId">The id of the team to retrieve users from</param>
        /// <param name="userId">The id of the user to retrieve roles from</param>
        /// <returns>The list of role names of the user in the specified team</returns>
        public static async Task<IList<TeamRole>> GetUsersRoles(uint teamId, string userId)
        {
            LogContext.PushProperty("Method","GetUsersRoles");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}, userId={userId}");

            string query = $@"
                                SELECT
                                    tr.Id,
                                    tr.Role,
                                    tr.TeamId,
                                    tr.Color
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


            return await SqlHelpers.MapToList(Mapper.TeamRoleFromDataRow, query, "Team_Roles");
        }

        /// <summary>
        /// Grant a team's role a specified permissions
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to grant a permission</param>
        /// <param name="permission">The permission to grant a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public static async Task<uint?> GrantPermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method","GrantPermissions");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}, role={role}, permission={permission}");


            var teamRoleIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";

            var teamRoleId = await SqlHelpers.ExecuteScalarAsync(teamRoleIdQuery, Convert.ToUInt32);

            LogContext.PushProperty("Method","GrantPermissions");
            LogContext.PushProperty("SourceContext", "TeamService");

            if (teamRoleId == 1)
            {
                logger.Information($"could not retrieve the Team_rolesId");

                logger.Information("Return value: false");

                return null;
            }

            var permissionsIdQuery = $@"
                                        SELECT
                                            Id
                                        FROM
                                            Permissions
                                        WHERE
                                            Permissions = '{Enum.GetName(typeof(Permissions),permission)}'";

            var permissionsId = await SqlHelpers.ExecuteScalarAsync(permissionsIdQuery, Convert.ToUInt32);

            LogContext.PushProperty("Method","GrantPermissions");
            LogContext.PushProperty("SourceContext", "TeamService");

            if (permissionsId== 0)
            {
                logger.Information($"could not retrieve the PermissionsId");

                logger.Information("Return value: false");

                return null;
            }

            string query = $@"
                                INSERT INTO
                                    Role_permissions
                                VALUES({permissionsId}, {teamRoleId});";

            bool isSuccess = await SqlHelpers.NonQueryAsync(query);

            if (!isSuccess)
            {
                return null;
            }

            return teamRoleId;
        }

        /// <summary>
        /// Revoke a permission from a specified team's role
        /// </summary>
        /// <param name="teamId">The id of the team to change permissions in</param>
        /// <param name="role">The role of the team to revoke a permission from</param>
        /// <param name="permission">The permission to revoke from a team's role</param>
        /// <returns>True on success, false otherwise</returns>
        public static async Task<uint?> RevokePermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method","RevokePermission");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}, role={role}, permission={permission}");

            var teamRoleIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";

            var teamRoleId = await SqlHelpers.ExecuteScalarAsync(teamRoleIdQuery, Convert.ToUInt32);

            LogContext.PushProperty("Method","RevokePermission");
            LogContext.PushProperty("SourceContext", "TeamService");

            if (teamRoleId == 1)
            {
                logger.Information($"could not retrieve the Team_rolesId");

                logger.Information("Return value: false");

                return null;
            }

            var permissionIdQuery= $@"
                                        SELECT
                                            Id
                                        FROM
                                            Permissions
                                        WHERE
                                            Permissions = '{Enum.GetName(typeof(Permissions), permission)}'";

            var permissionId = await SqlHelpers.ExecuteScalarAsync(permissionIdQuery, Convert.ToUInt32);

            LogContext.PushProperty("Method","RevokePermission");
            LogContext.PushProperty("SourceContext", "TeamService");

            if (permissionId == 0)
            {
                logger.Information($"could not retrieve the PermissionsId");

                logger.Information("Return value: false");

                return null;
            }

            string query = $@"
                                Delete FROM
                                    Role_permissions
                                WHERE
                                    PermissionsId = {permissionId}
                                    AND
                                    Team_rolesId = {teamRoleId};";

            bool isSuccess = await SqlHelpers.NonQueryAsync(query);

            if (!isSuccess)
            {
                return null;
            }

            return teamRoleId;
        }

        /// <summary>
        /// Check if a role has a permission in a team
        /// </summary>
        /// <param name="teamId">The id of the team check permissions in</param>
        /// <param name="role">The role of the team to check permission</param>
        /// <param name="permission">The permission to check for a team's role</param>
        /// <returns>True if the role has the permission, false otherwise</returns>
        public static async Task<bool> HasPermission(uint teamId, string role, Permissions permission)
        {
            LogContext.PushProperty("Method","HasPermission");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}, role={role}, permission={permission}");



                var Team_rolesIdQuery = $@"SELECT Id FROM Team_roles WHERE Role='{role}' AND TeamId={teamId}";

                var Team_rolesId = await SqlHelpers.ExecuteScalarAsync(Team_rolesIdQuery, Convert.ToUInt32);

                LogContext.PushProperty("Method","HasPermission");
                LogContext.PushProperty("SourceContext", "TeamService");

                if (Team_rolesId == 0)
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

                var PermissionsId = await SqlHelpers.ExecuteScalarAsync(PermissionsIdQuery, Convert.ToUInt32);

                LogContext.PushProperty("Method","HasPermission");
                LogContext.PushProperty("SourceContext", "TeamService");


                if (PermissionsId == 0)
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

                return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToBoolean);
        }

        /// <summary>
        /// List all permissions a role has
        /// </summary>
        /// <param name="teamId">The id of the team list permissions of</param>
        /// <param name="role">The role of the team to list permission of</param>
        /// <returns>A list of Permissions the role has</returns>
        public static async Task<IList<Permissions>> GetPermissionsOfRole(uint teamId, string role)
        {
            LogContext.PushProperty("Method","GetPermissionsOfRole");
            LogContext.PushProperty("SourceContext", "TeamService");
            logger.Information($"Function called with parameters teamId={teamId}, role={role}");

            string query = $@"
                SELECT
                    Permissions
                FROM
                    Permissions p
                LEFT JOIN Role_permissions rp ON
                    rp.permissionsId = p.Id
                LEFT JOIN Team_roles tr ON
                   tr.Id = rp.Team_rolesId
                WHERE
                    teamId = {teamId}
                    AND
                    Role = '{role}';
            ";


                // (つ◕_◕)つ Gimme' non-type template parameters
                return await SqlHelpers.MapToList((val) => Mapper.EnumFromDataRow<Permissions>(val, "permissions"), query);
        }
        #endregion
    }
}
