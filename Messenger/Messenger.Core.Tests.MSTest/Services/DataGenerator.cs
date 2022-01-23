using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Messenger.Core.Models;
using Messenger.Core.Services;


namespace Messenger.Tests.MSTest
{
    /// <summary>
    /// Holds static methods to generate objects of common entities
    /// </summary>
    public static class DataGenerator
    {
        public static async Task<User> DefaultUserGenerator(string entityNamePrefix)
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


        public static async Task<Team> DefaultTeamGenerator(string entityNamePrefix)
        {
            var teamId = await TeamService.CreateTeam(entityNamePrefix + "Team", entityNamePrefix + "Description");

            if (teamId is null)
            {
                return null;
            }
            return await TeamService.GetTeam(teamId.Value);
        }


        public static async Task<Channel> DefaultChannelGenerator(string entityNamePrefix, uint teamId)
        {
            var channelId = await ChannelService.CreateChannel(entityNamePrefix + "Channel", teamId);

            if (channelId is null)
            {
                return null;
            }
            return await ChannelService.GetChannel(channelId.Value);
        }


        public static async Task<Message> DefaultMessageGenerator(string entityNamePrefix, uint recipientId, string senderId)
        {
            var messageId = await MessageService.CreateMessage(recipientId, senderId, entityNamePrefix + "Message");

            if (messageId is null)
            {
                return null;
            }
            return await MessageService.GetMessage(messageId.Value);
        }


        public static async Task<Reaction> DefaultReactionGenerator(uint messageId, string userId)
        {
            var reactionId = await MessageService.AddReaction(messageId, userId, "+");

            return await MessageService.GetReaction(reactionId);
        }


        public static async Task<Dictionary<string, dynamic>> SetupData(
            string                                    entityNamePrefix,
            Func<string, Task<User>>                  alternateUserGenerator     = null,
            Func<string, Task<Team>>                  alternateTeamGenerator     = null,
            Func<string, uint, Task<Channel>>         alternateChannelGenerator  = null,
            Func<string, uint, string, Task<Message>> alternateMessageGenerator  = null,
            Func<uint, string, Task<Reaction>>        alternateReactionGenerator =  null
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
    }
}