using System.Threading.Tasks;
using System.Linq;
using Messenger.Core.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.TeamService
    /// </summary>
    [TestClass]
    public class ChannelTest
    {

        [TestMethod]
        public void CreateChannel_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);

                Assert.IsNotNull(channelId);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveChannel_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);

                Assert.IsNotNull(channelId);

                var numChannelsBefore = (await TeamService.GetAllChannelsByTeamId(teamId.Value)).Count();

                bool success = await ChannelService.RemoveChannel(channelId.Value);

                var numChannelsAfter = (await TeamService.GetAllChannelsByTeamId(teamId.Value)).Count();

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
                uint? teamId = await TeamService.CreateTeam(testName + "Team");

                Assert.IsNotNull(teamId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);

                Assert.IsNotNull(channelId);

                var channels = (await TeamService.GetAllChannelsByTeamId(teamId.Value));

                Assert.AreEqual(channels.Count(), 1);

                var oldName = channels[0].ChannelName;

                bool success = await ChannelService.RenameChannel(testName + "ChannelRename", channelId.Value);

                Assert.IsTrue(success);

                channels = (await TeamService.GetAllChannelsByTeamId(teamId.Value));

                Assert.AreEqual(channels.Count(), 1);

                var newName = channels[0].ChannelName;


                Assert.AreEqual(oldName + "Rename", newName);

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }
    }
}
