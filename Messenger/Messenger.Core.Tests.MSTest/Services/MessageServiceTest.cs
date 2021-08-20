using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Messenger.Core.Models;
using Messenger.Core.Services;

using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// MSTests for Messenger.Core.Services.TeamService
    /// </summary>
    [TestClass]
    public class MessageServiceTest
    {
        public async Task<User> DefaultUserGenerator(string entityNamePrefix)
        {
            return await UserService.GetOrCreateApplicationUser(new User(){
                    Id          = entityNamePrefix + "User",
                    DisplayName = entityNamePrefix + "DisplayName",
                    NameId      = 0,
                    Photo       = entityNamePrefix + "Photo",
                    Mail        = entityNamePrefix + "Mail",
                    Bio         = entityNamePrefix + "Bio"
                    });


        }
        
        public async Task<Team> DefaultTeamGenerator(string entityNamePrefix)
        {
            var teamId = await TeamService.CreateTeam(entityNamePrefix + "Team", entityNamePrefix + "Description");

            if (teamId is null)
            {
                return null;
            }
            return await TeamService.GetTeam(teamId.Value);
        }
        
        public async Task<Channel> DefaultChannelGenerator(string entityNamePrefix, uint teamId)
        {
            var channelId = await ChannelService.CreateChannel(entityNamePrefix + "Channel", teamId);

            if (channelId is null)
            {
                return null;
            }
            return await ChannelService.GetChannel(channelId.Value);
        }
        
        public async Task<Message> DefaultMessageGenerator(string entityNamePrefix, uint recipientId, string senderId)
        {
            var messageId = await MessageService.CreateMessage(recipientId, senderId, entityNamePrefix + "Message");

            if (messageId is null)
            {
                return null;
            }
            return await MessageService.GetMessage(messageId.Value);
        }
        
        public async Task<Reaction> DefaultReactionGenerator(uint messageId, string userId)
        {
            var reactionId = await MessageService.AddReaction(messageId, userId, "+");

            return await MessageService.GetReaction(reactionId);
        }

        public async Task<Dictionary<string, dynamic>> SetupData(
               string entityNamePrefix,
               Func<string, Task<User>> alternateUserGenerator     = null,
               Func<string, Task<Team>> alternateTeamGenerator     = null,
               Func<string, uint, Task<Channel>> alternateChannelGenerator  = null,
               Func<string, uint, string, Task<Message>> alternateMessageGenerator  = null,
               Func<uint, string, Task<Reaction>> alternateReactionGenerator =  null
            )
        {
            Dictionary<string, dynamic> data = new Dictionary<string, dynamic>();

            if (alternateUserGenerator is null)
            {
                data["User"] = await DefaultUserGenerator(entityNamePrefix);
            }
            else
            {
                data["User"] = await  alternateUserGenerator(entityNamePrefix);
            }
            if (alternateTeamGenerator is null)
            {
                data["Team"] = await  DefaultTeamGenerator(entityNamePrefix);
            }
            else
            {
                data["Team"] = await  alternateTeamGenerator(entityNamePrefix);
            }
            if (alternateChannelGenerator is null)
            {
                data["Channel"] = await  DefaultChannelGenerator(entityNamePrefix, data["Team"].Id);
            }
            else
            {
                data["Channel"] = await  alternateChannelGenerator(entityNamePrefix, data["Team"].Id);
            }
            if (alternateMessageGenerator is null)
            {
                data["Message"] = await  DefaultMessageGenerator(entityNamePrefix, data["Channel"].ChannelId, data["User"].Id);
            }
            else
            {
                data["Message"] = await  alternateMessageGenerator(entityNamePrefix, data["Channel"].ChannelId, data["User"].Id);
            }
            if (alternateReactionGenerator is null)
            {
                data["Reaction"] = await  DefaultReactionGenerator(data["Message"].Id, data["User"].Id);
            }
            else
            {
                data["Reaction"] = await  alternateReactionGenerator(data["Message"].Id, data["User"].Id);
            }
            
            return data;
        }
        
        [TestMethod]
        public void CreateMessage_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var data = await SetupData(testName);
                
                var result = await MessageService.CreateMessage(data["Channel"].ChannelId, data["User"].Id, data["Message"].Content);
                Assert.IsNotNull(result);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessages_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"});

                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var result = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message");

                var messages = await MessageService.RetrieveMessages(teamId.Value);
                Assert.IsTrue(messages.Count > 0);
                Assert.IsNotNull(messages[0]);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveMessagesNoneExist_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"});

                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var messages = await MessageService.RetrieveMessages(channelId.Value);

                Assert.IsTrue(messages.Count == 0);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RenameMessage_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                var messages = (await MessageService.RetrieveMessages(channelId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var oldContent = messages[0].Content;

                bool success = await MessageService.EditMessage(messageId.Value, testName + "MessageNewContent");

                Assert.IsTrue(success);

                messages = (await MessageService.RetrieveMessages(channelId.Value));

                Assert.AreEqual(messages.Count(), 1);

                var newContent = messages[0].Content;


                Assert.AreEqual(oldContent + "NewContent", newContent);

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMessage_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);


                var  numMessagesBefore = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                var success = await MessageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                Assert.IsTrue(numMessagesAfter < numMessagesBefore);


            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveMessageWithReplies_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? replyMessageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message", messageId);
                Assert.IsNotNull(replyMessageId);

                var  numMessagesBefore = (await MessageService.RetrieveMessages(channelId.Value)).Count();
                Assert.AreEqual(2, numMessagesBefore);

                var success = await MessageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(channelId.Value)).Count();
                Assert.AreEqual(0, numMessagesAfter);


            }).GetAwaiter().GetResult();
        }
        [TestMethod]
        public void RemoveMessageWithReactions_Test()
        {
            string testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var  numMessagesBefore = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                var success = await MessageService.DeleteMessage(messageId.Value);
                Assert.IsTrue(success);

                var numMessagesAfter = (await MessageService.RetrieveMessages(channelId.Value)).Count();

                Assert.IsTrue(numMessagesAfter < numMessagesBefore);


            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void AddReaction_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var reactions = await MessageService.RetrieveReactions(messageId.Value);

                Assert.AreEqual(reactions.Count(), 1);
                Assert.AreEqual(reactions.ToList()[0].Symbol, "🈚");

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RemoveReaction_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                uint? teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                string userId = (await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "UserId" ,DisplayName = testName + "UserName"})).Id;
                Assert.IsNotNull(userId);

                uint? messageId = await MessageService.CreateMessage(channelId.Value, userId, testName + "Message");
                Assert.IsNotNull(messageId);

                uint? reactionId = await MessageService.AddReaction(messageId.Value, userId, "🈚");
                Assert.IsNotNull(reactionId);

                var didRemoveReaction = await MessageService.RemoveReaction(messageId.Value, userId, "🈚");
                Assert.IsTrue(didRemoveReaction);

                var reactions = await MessageService.RetrieveReactions(messageId.Value);

                Assert.AreEqual(0, reactions.Count());

            }).GetAwaiter().GetResult();
        }

        [TestMethod]
        public void RetrieveReplies_Test()
        {
            var testName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            Task.Run(async () =>
            {
                var _ = await UserService.GetOrCreateApplicationUser(new User(){Id= testName + "User"});

                var teamId = await TeamService.CreateTeam(testName + "Team");
                Assert.IsNotNull(teamId);

                var channelId = await ChannelService.CreateChannel(testName + "Channel", teamId.Value);
                Assert.IsNotNull(channelId);

                var parentMessageId = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message");
                Assert.IsNotNull(parentMessageId);

                var childMessageId1 = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message", parentMessageId.Value);
                Assert.IsNotNull(childMessageId1);

                var childMessageId2 = await MessageService.CreateMessage(channelId.Value, testName + "User", testName + "Message", parentMessageId.Value);
                Assert.IsNotNull(childMessageId2);

                var repliesToParentMessage = await MessageService.RetrieveReplies(parentMessageId.Value);

                Assert.AreEqual(repliesToParentMessage.Count, 2);

            }).GetAwaiter().GetResult();
        }

        [TestCleanup]
        public void Cleanup()
        {
            ServiceCleanup.Cleanup();
        }
    }
}
