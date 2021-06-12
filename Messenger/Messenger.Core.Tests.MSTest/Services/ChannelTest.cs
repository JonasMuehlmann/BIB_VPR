using System.Threading.Tasks;
using System.Linq;
using System;
using System.Data.SqlClient;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Core.Helpers;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.TeamService
    /// </summary>
    [TestClass]
    public class ChannelTest : SqlServiceTestBase
    {
        TeamService teamService;
        ChannelService channelService;

        [TestInitialize]
        public void Initialize()
        {
            teamService = InitializeTestMode<TeamService>();
            channelService = InitializeTestMode<ChannelService>();
        }

        [TestMethod]
        public void CreateChannel_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;
            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                uint? channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);

                Assert.IsNotNull(channelId);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveChannel_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                var numChannelsBefore = (await teamService.GetAllChannelsByTeamId(teamId.Value)).Count();

                uint? channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);

                Assert.IsNotNull(channelId);

                bool success = await channelService.RemoveChannel(channelId.Value);

                var numChannelsAfter = (await teamService.GetAllChannelsByTeamId(teamId.Value)).Count();

                teamService.logger.Information($"{numChannelsBefore}");
                teamService.logger.Information($"{numChannelsAfter}");

                Assert.IsTrue(success);
                Assert.IsTrue(numChannelsAfter < numChannelsBefore);


            }).GetAwaiter().GetResult();
        }

    }
}
