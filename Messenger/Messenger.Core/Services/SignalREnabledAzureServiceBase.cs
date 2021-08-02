using Messenger.Core.Helpers;
using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using Serilog.Context;

namespace Messenger.Core.Services
{
    public abstract class SignalREnabledAzureServiceBase: AzureServiceBase
    {
        public static SignalRService SignalRService => Singleton<SignalRService>.Instance;
        
        /// <summary>
        /// Connects the given user to the teams he is a member of
        /// </summary>
        /// <param name="userId">The user to connect to his teams</param>
        /// <param name="connectionString">(optional)connection string to initialize with</param>
        /// <returns>List of teams the user has membership of, null if none exists</returns>
        public static async Task<IList<Team>> Initialize(string userId, string connectionString = null)
        {
            LogContext.PushProperty("Method","Initialize");
            LogContext.PushProperty("SourceContext", "MessengerService");
            logger.Information($"Function called with parameters userId={userId}");

            /** REGISTER USER TO SIGNAL-R HUB **/
            SignalRService.Initialize();
            await SignalRService.OpenConnection(userId);

            /** LOAD TEAMS **/
            IEnumerable<Team> teams = await TeamService.GetAllTeamsByUserId(userId);

            /* EXIT IF NO TEAM **/
            if (teams == null || teams.Count() <= 0)
            {
                logger.Information($"No teams found for the current user");
                logger.Information($"Return value: null");

                return null;
            }

            logger.Information($"Loaded the following teams for the current user: {string.Join(", ", teams)}");

            List<Team> result = new List<Team>();

            /** CONNECT TO EACH SIGNAL-R GROUPS **/
            foreach (Team team in teams)
            {
                await SignalRService.JoinTeam(userId, team.Id.ToString());

                result.Add(team);

                logger.Information($"Connected the current user to the team {team.Id}");
            }

            logger.Information($"Return value: {result}");

            return result;
        }
    }
}