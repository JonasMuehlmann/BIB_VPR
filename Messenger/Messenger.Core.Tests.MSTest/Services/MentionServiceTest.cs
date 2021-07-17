using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
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

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId, userId);
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

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId, userId);
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

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId, userId);
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

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId, userId);
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

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId, userId);
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

                uint? mentionId = await MentionService.CreateMention(MentionTarget.User, userId, userId);
                Assert.IsNotNull(mentionId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, $"{testName} @{mentionId.Value} Message");
                Assert.IsNotNull(messageId);

                var messageOriginal = (await MessageService.GetMessage(messageId.Value)).Content;
                Assert.AreEqual($"{testName} @{mentionId.Value} Message", messageOriginal);

                var messageResolved = await MentionService.ResolveMentions(messageOriginal);
                Assert.AreEqual($"{testName} {testName}UserName Message", messageResolved);
            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void SearchMentionableUser_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                List<User> usersWantedTeam = new List<User>{
                                       new User(){Id="Id1",  NameId=1, DisplayName=$"{testName}1"}
                                     , new User(){Id="Id2",  NameId=2, DisplayName=$"{testName}1"}
                                     , new User(){Id="Id3",  NameId=1, DisplayName=$"The{testName}2"}
                                     , new User(){Id="Id4",  NameId=1, DisplayName=$"Another{testName}"}
                                     , new User(){Id="Id5",  NameId=1, DisplayName=$"YetAnother{testName}"}
                                     , new User(){Id="Id6",  NameId=1, DisplayName=$"ThisIsA{testName}"}
                                     , new User(){Id="Id7",  NameId=1, DisplayName=$"A{testName}ThisBe"}
                                     };

                List<User> usersOtherTeam = new List<User>{
                                       new User(){Id="Id8",  NameId=1, DisplayName=$"SomeText"}
                                     , new User(){Id="Id9",  NameId=1, DisplayName=$"Yeet"}
                                     , new User(){Id="Id10", NameId=1, DisplayName=$"Oi mate"}
                                     , new User(){Id="Id11", NameId=1, DisplayName=$"Deez Nuts {testName}"}
                                     , new User(){Id="Id12", NameId=1, DisplayName=$"  "}
                                     , new User(){Id="Id13", NameId=1, DisplayName=$"jdhsjdhjdhj{testName}dksdskdjkdjsk"}
                                     , new User(){Id="Id14", NameId=1, DisplayName=$"ksjdksjdahdj"}
                                     , new User(){Id="Id15", NameId=1, DisplayName=$"jdhsjdhjdhj {testName} dksdskdjkdjsk"}
                                 };

                var userMatchString = $"{testName}1#000001,{testName}1#000002,The{testName}2#000001,Another{testName}#000001,ThisIsA{testName}#000001,A{testName}ThisBe#000001,YetAnother{testName}#000001";

                uint? teamIdWanted = await TeamService.CreateTeam(testName + "TeamWanted");
                Assert.IsNotNull(teamIdWanted);

                foreach (var user in usersWantedTeam)
                {
                    Assert.IsNotNull(await UserService.GetOrCreateApplicationUser(user));
                    Assert.IsTrue(await TeamService.AddMember(user.Id, teamIdWanted.Value));
                }

                uint? teamIdOther = await TeamService.CreateTeam(testName + "TeamOther");
                Assert.IsNotNull(teamIdOther);

                foreach (var user in usersOtherTeam)
                {
                    Assert.IsNotNull(await UserService.GetOrCreateApplicationUser(user));
                    Assert.IsTrue(await TeamService.AddMember(user.Id, teamIdOther.Value));
                }

                var matchedMentionables = await MentionService.SearchMentionable($"u:{testName}", teamIdWanted.Value);
                Assert.IsNotNull(matchedMentionables);

                Assert.AreEqual(userMatchString, string.Join(",", matchedMentionables.Select((mentionable) => mentionable.TargetName)));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void SearchMentionableRole_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamIdWanted = await TeamService.CreateTeam(testName + "TeamWanted");
                Assert.IsNotNull(teamIdWanted);

                uint? teamIdOther = await TeamService.CreateTeam(testName + "TeamOther");
                Assert.IsNotNull(teamIdOther);

                List<string> rolesWantedTeam = new List<string>{
                                      $"{testName}1"
                                     ,$"{testName}1"
                                     ,$"The{testName}2"
                                     ,$"Another{testName}"
                                     ,$"YetAnother{testName}"
                                     ,$"ThisIsA{testName}"
                                     ,$"A{testName}ThisBe"
                                     };

                List<string> rolesOtherTeam = new List<string>{
                                      $"SomeText"
                                     ,$"Yeet"
                                     ,$"Oi mate"
                                     ,$"Deez Nuts {testName}"
                                     ,$"  "
                                     ,$"jdhsjdhjdhj{testName}dksdskdjkdjsk"
                                     ,$"ksjdksjdahdj"
                                     ,$"jdhsjdhjdhj {testName} dksdskdjkdjsk"
                                 };

                foreach (var role in rolesWantedTeam)
                {
                    Assert.IsNotNull(await TeamService.AddRole(role, teamIdWanted.Value));
                }

                foreach (var role in rolesOtherTeam)
                {
                    Assert.IsNotNull(await TeamService.AddRole(role, teamIdOther.Value));
                }

                var roleMatchString = $"{testName}1,{testName}1,The{testName}2,Another{testName},ThisIsA{testName},A{testName}ThisBe,YetAnother{testName}";

                var matchedMentionables = await MentionService.SearchMentionable($"r:{testName}", teamIdWanted.Value);
                Assert.IsNotNull(matchedMentionables);

                Assert.AreEqual(roleMatchString, string.Join(",", matchedMentionables.Select((mentionable) => mentionable.TargetName)));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void SearchMentionableChannel_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamIdWanted = await TeamService.CreateTeam(testName + "TeamWanted");
                Assert.IsNotNull(teamIdWanted);

                uint? teamIdOther = await TeamService.CreateTeam(testName + "TeamOther");
                Assert.IsNotNull(teamIdOther);

                List<string> channelWantedTeam = new List<string>{
                                      $"{testName}1"
                                     ,$"{testName}1"
                                     ,$"The{testName}2"
                                     ,$"Another{testName}"
                                     ,$"YetAnother{testName}"
                                     ,$"ThisIsA{testName}"
                                     ,$"A{testName}ThisBe"
                                     };

                List<string> channelOtherTeam = new List<string>{
                                      $"SomeText"
                                     ,$"Yeet"
                                     ,$"Oi mate"
                                     ,$"Deez Nuts {testName}"
                                     ,$"  "
                                     ,$"jdhsjdhjdhj{testName}dksdskdjkdjsk"
                                     ,$"ksjdksjdahdj"
                                     ,$"jdhsjdhjdhj {testName} dksdskdjkdjsk"
                                 };

                foreach (var channel in channelWantedTeam)
                {
                    Assert.IsNotNull(await ChannelService.CreateChannel(channel, teamIdWanted.Value));
                }

                foreach (var channel in channelOtherTeam)
                {
                    Assert.IsNotNull(await ChannelService.CreateChannel(channel, teamIdOther.Value));
                }

                var roleMatchString = $"{testName}1,{testName}1,The{testName}2,Another{testName},ThisIsA{testName},A{testName}ThisBe,YetAnother{testName}";

                var matchedMentionables = await MentionService.SearchMentionable($"c:{testName}", teamIdWanted.Value);
                Assert.IsNotNull(matchedMentionables);

                Assert.AreEqual(roleMatchString, string.Join(",", matchedMentionables.Select((mentionable) => mentionable.TargetName)));

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void SearchMentionableMessage_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? teamIdWanted = await TeamService.CreateTeam(testName + "TeamWanted");
                Assert.IsNotNull(teamIdWanted);

                uint? channelIdWanted = await ChannelService.CreateChannel(testName + "ChannelWanted", teamIdWanted.Value);
                Assert.IsNotNull(channelIdWanted);

                uint? teamIdOther = await TeamService.CreateTeam(testName + "TeamOther");
                Assert.IsNotNull(teamIdOther);

                uint? channelIdOther = await ChannelService.CreateChannel(testName + "ChannelWanted", teamIdWanted.Value);
                Assert.IsNotNull(channelIdOther);

                List<string> messageWantedTeam = new List<string>{
                                      $"{testName}1"
                                     ,$"{testName}1"
                                     ,$"The{testName}2"
                                     ,$"Another{testName}"
                                     ,$"YetAnother{testName}"
                                     ,$"ThisIsA{testName}"
                                     ,$"A{testName}ThisBe"
                                     };

                List<string> messageOtherTeam = new List<string>{
                                      $"SomeText"
                                     ,$"Yeet"
                                     ,$"Oi mate"
                                     ,$"Deez Nuts {testName}"
                                     ,$"  "
                                     ,$"jdhsjdhjdhj{testName}dksdskdjkdjsk"
                                     ,$"ksjdksjdahdj"
                                     ,$"jdhsjdhjdhj {testName} dksdskdjkdjsk"
                                 };

                List<uint> messageIdsWanted = new List<uint>();

                foreach (var message in messageWantedTeam)
                {
                    var messageId = await MessageService.CreateMessage(channelIdWanted.Value,userId, message);
                    Assert.IsNotNull(messageId);

                    messageIdsWanted.Add(messageId.Value);
                }

                foreach (var message in messageOtherTeam)
                {
                    Assert.IsNotNull(await MessageService.CreateMessage(channelIdOther.Value, userId, message));
                }

                var matchedMentionables = await MentionService.SearchMentionable($"m:{messageIdsWanted[0]}", teamIdWanted.Value);
                Assert.IsNotNull(matchedMentionables);

                Assert.AreEqual(messageIdsWanted[0], Convert.ToUInt32(matchedMentionables[0].TargetId));

            }).GetAwaiter().GetResult();
        }
    }
}
