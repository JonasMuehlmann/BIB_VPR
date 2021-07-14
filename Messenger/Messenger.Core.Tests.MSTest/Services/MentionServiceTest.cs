
using System.Threading.Tasks;
using System.Linq;
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
    public class MentionServiceTest
    {

        [TestMethod]
        public void CreateMention_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMention_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

                bool didRemove = await MentionService.RemoveMention(mentionId.Value);
                Assert.IsTrue(didRemove);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ResolveMentionNoMention_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

                var messageOriginal = (await MessageService.GetMessage(messageId.Value)).Content;
                Assert.AreEqual(testName + "Message", messageOriginal);

                var messageResolved = await MentionService.ResolveMentions(messageOriginal);
                Assert.AreEqual(messageOriginal, messageResolved);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ResolveMentionMentionAtEnd_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, $"{testName}Message @{mentionId.Value}");
                Assert.IsNotNull(messageId);

                var messageOriginal = (await MessageService.GetMessage(messageId.Value)).Content;
                Assert.AreEqual($"{testName}Message @{mentionId.Value}", messageOriginal);

                var messageResolved = await MentionService.ResolveMentions(messageOriginal);
                Assert.AreEqual($"{testName}Message {testName}UserName", messageResolved);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ResolveMentionMentionAtBeginning_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, $"@{mentionId.Value} {testName}Message");
                Assert.IsNotNull(messageId);

                var messageOriginal = (await MessageService.GetMessage(messageId.Value)).Content;
                Assert.AreEqual($"@{mentionId.Value} {testName}Message", messageOriginal);

                var messageResolved = await MentionService.ResolveMentions(messageOriginal);
                Assert.AreEqual($"{testName}UserName {testName}Message", messageResolved);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void ResolveMentionMentionInMiddle_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId);
                Assert.IsNotNull(mentionId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, $"{testName} @{mentionId.Value} Message");
                Assert.IsNotNull(messageId);

                var messageOriginal = (await MessageService.GetMessage(messageId.Value)).Content;
                Assert.AreEqual($"{testName} @{mentionId.Value} Message", messageOriginal);

                var messageResolved = await MentionService.ResolveMentions(messageOriginal);
                Assert.AreEqual($"{testName} {testName}UserName Message", messageResolved);
            }).GetAwaiter().GetResult();
        }
    }
}
