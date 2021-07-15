using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public class MentionService : AzureServiceBase
    {
        /// <summary>
        /// Create a mention to the entity of type target identified by id
        /// </summary>
        /// <param name="target">The type of the entity to mention</param>
        /// <param name="id">The id of the entity to mention</param>
        /// <returns>The created mention id on success, null on failure</returns>
        public static async Task<uint?> CreateMention<T>(MentionTarget target, T id)
        {
            LogContext.PushProperty("Method","CreateMention");
            LogContext.PushProperty("SourceContext", "MentionService");
            logger.Information($"Function called with parameters target={target.ToString()}, id={id}");

            if (typeof(T) == typeof(string) || typeof(T) == typeof(uint))
            {
                var query = $@"
                                INSERT INTO
                                    mentions
                                VALUES(
                                    '{target.ToString()}',
                                    '{id}'
                                      );

                                SELECT SCOPE_IDENTITY();
                    ";

                return await SqlHelpers.ExecuteScalarAsync(query, Convert.ToUInt32);
            }
            else
            {
               throw new ArgumentException("id can only be string or uint");
            }
        }

        /// <summary>
        /// Remove a mention entry with the specified id
        /// </summary>
        /// <param name="mentionId">The id of the mention entry to remove</param>
        /// <returns>true on successfully, false otherwise</returns>
        public static async Task<bool> RemoveMention(uint mentionId)
        {
            LogContext.PushProperty("Method","RemoveMention");
            LogContext.PushProperty("SourceContext", "MentionService");
            logger.Information($"Function called with parameters mentionId={mentionId}");

            var query = $@"
                            DELETE FROM
                                Mentions
                            WHERE
                                Id = '{mentionId}';
                ";

            return await SqlHelpers.NonQueryAsync(query);
        }

        /// <summary>
        /// Retrieve a mention object
        /// </summary>
        /// <param name="mentionId">The id of the mention to retrieve</param>
        /// <returns>The mention object</returns>
        public static async Task<Mention> RetrieveMention(uint mentionId)
        {
            LogContext.PushProperty("Method","RetrieveMention");
            LogContext.PushProperty("SourceContext", "MentionService");
            logger.Information($"Function called with parameters mentionId={mentionId}");

            var query = $@"
                            SELECT
                                *
                            FROM
                                Mentions
                            WHERE
                                Id = '{mentionId}';
                ";

            return (await SqlHelpers.GetRows("Mentions", query)).Select(Mapper.MentionFromDataRow).FirstOrDefault();
        }

        /// <summary>
        /// Resolve message's a mentionId's referenced entity name
        /// </summary>
        /// <param name="message">The message containing the mentions to resolve</param>
        /// <returns>The message with all mentions resolved</returns>
        public static async Task<string> ResolveMentions(string message)
        {
            LogContext.PushProperty("Method","ResolveMentions");
            LogContext.PushProperty("SourceContext", "MentionService");
            logger.Information($"Function called with parameters message={message}");

            // Regex example:blah blah @123456 blah blub
            //                         \_____/
            // Match ---------------------'
            //
            //Regex example:@123456 blah blub
            //              \_____/
            // Match ----------'
            //
            //Regex example:blah blah @123456
            //                        \_____/
            // Match ---------------------'
            //
            //Regex example:blah blah@123456blah
            // No matc
            var splitMessage = Regex.Split(message, @"((?<= |^)@\d+(?= |$))");
            var resolvedMessage = "";
            var resolvedSubstr = "";

            foreach (var substr in splitMessage)
            {
                if (substr.StartsWith("@"))
                {
                    logger.Information($"Found mention {substr}");
                    var mention = await RetrieveMention(Convert.ToUInt32(substr.Substring(1)));

                    switch (mention.TargetType)
                    {
                        case MentionTarget.User:
                            resolvedSubstr = (await UserService.GetUser(mention.TargetId)).DisplayName;
                            break;
                        case MentionTarget.Role:
                            resolvedSubstr = (await TeamService.GetRole(Convert.ToUInt32(mention.TargetId))).Role;
                            break;
                        case MentionTarget.Channel:
                            resolvedSubstr = (await ChannelService.GetChannel(Convert.ToUInt32(mention.TargetId))).ChannelName;
                            break;
                        case MentionTarget.Message:
                            resolvedSubstr = $"#{mention.Id}";
                            break;
                        default:
                            LogContext.PushProperty("Method","ResolveMentions");
                            LogContext.PushProperty("SourceContext", "MentionService");
                            logger.Information($"Could not resolve mention {mention}");

                            resolvedSubstr = "";
                            break;
                    }

                    LogContext.PushProperty("Method","ResolveMentions");
                    LogContext.PushProperty("SourceContext", "MentionService");
                    logger.Information($"Resolved mention {substr} to {resolvedSubstr}");

                    resolvedMessage += resolvedSubstr;
                }
                else
                {
                    resolvedMessage += substr;
                }

            }

            return resolvedMessage;
        }

        /// <summary>
        /// Retrieve Mentionable objects of the top 10 user matches for the userName
        /// </summary>
        /// <param name="userName">User name to retrieve matches for</param>
        /// <param name="teamId">The id of the team to search in</param>
        /// <returns>List of top 10 matched mentionables</returns>
        private static async Task<IList<Mentionable>> SearchUsers(string userName, uint teamId)
        {
            LogContext.PushProperty("Method","SearchUser");
            LogContext.PushProperty("SourceContext", "MentionService");

            logger.Information($"Function called with parameters userName={userName}");

            string query = $@"
                                    SELECT
                                        u.UserId AS Id,
                                        CONCAT(UserName, '#', '00000' + RIGHT(NameId, 3)) AS TargetName,
                                        'User' AS TargetType
                                    FROM
                                        Users u
                                    LEFT JOIN Memberships m ON
                                        u.UserId = m.UserId
                                    WHERE
                                        LOWER(UserName) LIKE LOWER('%{userName}%')
                                        AND
                                        m.teamId = {teamId}
                                    ORDER BY
                                        LEN(UserName);";

            return await SqlHelpers.MapToList(Mapper.MentionableFromDataRow, query);
        }

        /// <summary>
        /// Retrieve Mentionable objects of the top 10 role matches for the roleName
        /// </summary>
        /// <param name="roleName">Role name to retrieve matches for</param>
        /// <param name="teamId">The id of the team to search in</param>
        /// <returns>List of top 10 matched mentionables</returns>
        private static async Task<IList<Mentionable>> SearchRoles(string roleName, uint teamId)
        {
            LogContext.PushProperty("Method","SearchRoles");
            LogContext.PushProperty("SourceContext", "MentionService");

            logger.Information($"Function called with parameters roleName={roleName}");

            string query = $@"
                                    SELECT
                                        Id AS Id,
                                        Role AS TargetName,
                                        'Role' AS TargetType
                                    FROM
                                        Team_roles
                                    WHERE
                                        LOWER(Role) LIKE LOWER('%{roleName}%')
                                        AND
                                        teamId = {teamId}
                                    ORDER BY
                                        LEN(Role);";

            return await SqlHelpers.MapToList(Mapper.MentionableFromDataRow, query);
        }

        /// <summary>
        /// Retrieve Mentionable objects of the top 10 channel matches for the
        /// channelName
        /// </summary>
        /// <param name="channelName">Channel name to retrieve matches for</param>
        /// <param name="teamId">The id of the team to search in</param>
        /// <returns>List of top 10 matched mentionables</returns>
        private static async Task<IList<Mentionable>> SearchChannels(string channelName, uint teamId)
        {
            LogContext.PushProperty("Method","SearchChannels");
            LogContext.PushProperty("SourceContext", "MentionService");

            logger.Information($"Function called with parameters channelName={channelName}");

            string query = $@"
                                    SELECT
                                        ChannelId AS Id,
                                        ChannelName AS TargetName,
                                        'Channel' AS TargetType
                                    FROM
                                        Channels
                                    WHERE
                                        LOWER(ChannelName) LIKE LOWER('%{channelName}%')
                                        AND
                                        teamId = {teamId}
                                    ORDER BY
                                        LEN(ChannelName);";

            return await SqlHelpers.MapToList(Mapper.MentionableFromDataRow, query);
        }

        /// <summary>
        /// Retrieve Mentionable objects of the top 10 channel matches for the
        /// messageId
        /// </summary>
        /// <param name="messageId">Id of the message to search for</param>
        /// <param name="teamId">The id of the team to search in</param>
        /// <returns>List of top 10 matched mentionables</returns>
        private static async Task<IList<Mentionable>> SearchMessages(string messageId, uint teamId)
        {
            LogContext.PushProperty("Method","SearchMessages");
            LogContext.PushProperty("SourceContext", "MentionService");

            logger.Information($"Function called with parameters messageId={messageId}");

            string query = $@"
                                    SELECT
                                        MessageId AS Id,
                                        SUBSTRING(Message, 0, 15) + '...' AS TargetName,
                                        'Message' AS TargetType
                                    FROM
                                        Messages m
                                    LEFT JOIN Channels c ON
                                        m.RecipientId = c.ChannelId
                                        AND
                                        teamId = {teamId}
                                    WHERE
                                        LOWER(CONVERT(VARCHAR(15), MessageId)) LIKE LOWER('%{messageId}%')
                                    ";

            return await SqlHelpers.MapToList(Mapper.MentionableFromDataRow, query);
        }

        /// <summary>
        /// Return the top 10 mentionables matching the searchString
        /// </summary>
        /// <param name="searchString">The string used to rank matched entities</param>
        /// <param name="teamId">The id of the team to search in</param>
        /// <returns>A list of Mentionables</returns>
        public static async Task<IList<Mentionable>> SearchMentionable(string searchString, uint teamId)
        {
            LogContext.PushProperty("Method","SearchMentionable");
            LogContext.PushProperty("SourceContext", "MentionService");

            logger.Information($"Function called with parameters searchString={searchString}");

            var mentionables = new List<Mentionable>();

            // Check for filters
            if (char.IsLetter(searchString[0]) && searchString[1] == ':')
            {
                switch (searchString[0])
                {
                    case 'u':
                        mentionables.AddRange(await SearchUsers(searchString.Substring(2), teamId));
                        break;
                    case 'r':
                        mentionables.AddRange(await SearchRoles(searchString.Substring(2), teamId));
                        break;
                    case 'c':
                        mentionables.AddRange(await SearchChannels(searchString.Substring(2), teamId));
                        break;
                    case 'm':
                        mentionables.AddRange(await SearchMessages(searchString.Substring(2), teamId));
                        break;
                    default:
                        // Note: If we can't parse the filter, we remove it and do an
                        // unfiltered search instead
                        return await SearchMentionable(searchString.Substring(2), teamId);
                }
            }
            else
            {
                mentionables.AddRange(await SearchUsers(searchString.Substring(2), teamId));
                mentionables.AddRange(await SearchRoles(searchString.Substring(2), teamId));
                mentionables.AddRange(await SearchChannels(searchString.Substring(2), teamId));
                mentionables.AddRange(await SearchMessages(searchString.Substring(2), teamId));
            }

            return mentionables;
        }
    }
}
