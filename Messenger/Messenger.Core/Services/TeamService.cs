﻿using System;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

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

            Serilog.Context.LogContext.PushProperty("Method","CreateTeam");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

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


                    logger.Information($"Running the following query: {scalarQuery}");

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
        /// <returns>True if no exceptions occured while executing the query and it affected at leasat one query, false otherwise</returns>
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
            Serilog.Context.LogContext.PushProperty("Method","GetAllTeams");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);

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
                HandleException(e);

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
            string query = $"SELECT TeamId, TeamName, TeamDescription, CreationDate " +
                $"FROM Teams " +
                $"WHERE TeamId = {teamId};";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                    return SqlHelpers
                        .GetRows("Teams", adapter)
                        .Select(Mapper.TeamFromDataRow)
                        .FirstOrDefault();
                }
            }
            catch (SqlException e)
            {
                HandleException(e);
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
            Serilog.Context.LogContext.PushProperty("Method","GetAllTeamsByUserId");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
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

            Serilog.Context.LogContext.PushProperty("Method","AddMember");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            string query = $"INSERT INTO Memberships(UserId, TeamId, UserRole) VALUES('{userId}', '{teamId}', 'placeholder');";

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
            Serilog.Context.LogContext.PushProperty("Method","RemoveMember");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters userId={userId}, teamId={teamId}");

            string query = $"DELETE FROM Memberships WHERE UserId='{userId}' AND TeamId={teamId};";

            logger.Information($"Running the following query: {query}");

            var result = await SqlHelpers.NonQueryAsync(query, GetConnection());

            logger.Information($"Return value: {result}");

            return result;
        }

        /// <summary>
        /// Gets all members in the team
        /// </summary>
        /// <param name="teamId">The id of the team to get members of</param>
        /// <returns>An enumerable of User objects</returns>
        public async Task<IEnumerable<User>> GetAllUsersByTeamId(uint teamId)
        {

            Serilog.Context.LogContext.PushProperty("Method","GetAllUsersByTeamId");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");

            string subquery = $"SELECT UserId FROM Memberships WHERE TeamId={teamId}";
            string query = $"SELECT * FROM Users WHERE UserId IN ({subquery})";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    logger.Information($"Running the following query: {query}");

                    var result = SqlHelpers.MapToList(Mapper.UserFromDataRow,
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
        /// Gets all memberships of a user
        /// </summary>
        /// <param name="userId">User id for the current user</param>
        /// <returns>A list of membership objects</returns>
        public async Task<IList<Membership>> GetAllMembershipByUserId(string userId)
        {
            Serilog.Context.LogContext.PushProperty("Method","GetAllMembershipByUserId");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
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

        public async Task<IEnumerable<User>> GetAllMembers(uint teamId)
        {
            Serilog.Context.LogContext.PushProperty("Method","GetAllMembers");
            Serilog.Context.LogContext.PushProperty("SourceContext", this.GetType().Name);
            logger.Information($"Function called with parameters teamId={teamId}");

            string subquery = $"SELECT UserId FROM Memberships WHERE TeamId={teamId}";
            string query = $"SELECT * FROM Users WHERE UserId IN ({subquery})";

            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    await connection.OpenAsync();

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataSet dataSet = new DataSet();

                    logger.Information($"Running the following query: {query}");
                    adapter.Fill(dataSet, "Users");

                    var result = dataSet.Tables["Users"].Rows.Cast<DataRow>().Select(Mapper.UserFromDataRow);

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
    }
}
