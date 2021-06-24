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
    public class ChannelTest
    {
        TeamService teamService;
        ChannelService channelService;

        [TestInitialize]
        public void Initialize()
        {
            teamService = new TeamService();
            channelService = new ChannelService();
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

                uint? channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);

                Assert.IsNotNull(channelId);

                var numChannelsBefore = (await teamService.GetAllChannelsByTeamId(teamId.Value)).Count();

                bool success = await channelService.RemoveChannel(channelId.Value);

                var numChannelsAfter = (await teamService.GetAllChannelsByTeamId(teamId.Value)).Count();

                Assert.IsTrue(success);
                Assert.IsTrue(numChannelsAfter < numChannelsBefore);


            }).GetAwaiter().GetResult();
        }
        [TestMethod]
        public void RenameChannel_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await teamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                uint? channelId = await channelService.CreateChannel(testName + "Channel", teamId.Value);

                Assert.IsNotNull(channelId);

                var channels = (await teamService.GetAllChannelsByTeamId(teamId.Value));

                Assert.AreEqual(channels.Count(), 1);

                var oldName = channels[0].ChannelName;

                bool success = await channelService.RenameChannel(testName + "ChannelRename", channelId.Value);

                Assert.IsTrue(success);

                channels = (await teamService.GetAllChannelsByTeamId(teamId.Value));

                Assert.AreEqual(channels.Count(), 1);

                var newName = channels[0].ChannelName;


                Assert.AreEqual(oldName + "Rename", newName);

            }).GetAwaiter().GetResult();
        }
    }
}
