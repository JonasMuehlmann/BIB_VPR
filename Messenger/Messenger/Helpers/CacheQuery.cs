using Messenger.Core.Models;
using Messenger.Helpers.MessageHelpers;
using Messenger.Helpers.TeamHelpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers
{
    public static class CacheQuery
    {
        public static async Task Reload()
        {
            await App.StateProvider.TeamManager.LoadTeamsFromDatabase(App.StateProvider.CurrentUser);

            App.EventProvider.Broadcast(
                BroadcastOptions.TeamsLoaded,
                BroadcastReasons.Loaded,
                GetMyTeams());

            App.EventProvider.Broadcast(
                BroadcastOptions.ChatsLoaded,
                BroadcastReasons.Loaded,
                GetMyChats());
        }

        public static bool IsChannelOf<T>(ChannelViewModel viewModel)
        {
            Type type = typeof(T);

            if (IsTypeOf<TeamViewModel>(type))
            {
                IEnumerable<ChannelViewModel> channels = GetMyTeams().SelectMany(t => t.Channels);

                return channels.Any(channel => channel.ChannelId == viewModel.ChannelId);
            }
            else if (IsTypeOf<PrivateChatViewModel>(type))
            {
                IEnumerable<ChannelViewModel> mainChannels = GetMyChats().Select(c => c.MainChannel);

                return mainChannels.Any(mainChannel => mainChannel.ChannelId == viewModel.ChannelId);
            }

            return false;
        }

        public static IReadOnlyCollection<TeamViewModel> GetMyTeams() => App.StateProvider.TeamManager.MyTeams;

        public static IReadOnlyCollection<PrivateChatViewModel> GetMyChats() => App.StateProvider.TeamManager.MyChats;

        public static bool TryGetMessages(uint channelId, out ObservableCollection<MessageViewModel> messages) => App.StateProvider.MessageManager.TryGetMessages(channelId, out messages);

        /// <summary>
        /// Gets given model from the cache,
        /// following parameter(s) is/are required for each model:
        /// 'TeamViewModel' => 'uint teamId',
        /// 'PrivateChatViewModel' => 'uint chatId',
        /// 'ChannelViewModel' => 'uint channelId',
        /// </summary>
        /// <typeparam name="T">Type of view model to get from the cache</typeparam>
        /// <param name="parameters">Required parameter(s)</param>
        /// <returns>Requested view model</returns>
        public static T Get<T>(params object[] parameters) where T : DataViewModel
        {
            TeamManager teamManager = App.StateProvider.TeamManager;
            MessageManager messageManager = App.StateProvider.MessageManager;
            Type type = typeof(T);
            dynamic target = default(T);

            if (parameters == null
                || parameters.Length <= 0)
            {
                return target;
            }
            else if (IsTypeOf<TeamViewModel>(type))
            {
                target = teamManager.MyTeams.SingleOrDefault(team => team.Id == (uint)parameters.First());
            }
            else if (IsTypeOf<PrivateChatViewModel>(type))
            {
                target = teamManager.MyChats.SingleOrDefault(chat => chat.Id == (uint)parameters.First());
            }
            else if (IsTypeOf<ChannelViewModel>(type))
            {
                uint channelId = (uint)parameters.First();

                IEnumerable<ChannelViewModel> allChannels = teamManager.MyTeams.SelectMany(t => t.Channels);

                if (allChannels.Any(c => c.ChannelId == channelId))
                {
                    target = allChannels.Single(channel => channel.ChannelId == (uint)parameters.First());
                }
            }
            else if (IsTypeOf<MemberViewModel>(type))
            {
                if (parameters.Length == 2)
                {
                    uint teamId = (uint)parameters[0];
                    string userId = parameters[1].ToString();

                    TeamViewModel targetTeam = teamManager.MyTeams.SingleOrDefault(team => team.Id == teamId);

                    target = targetTeam.Members.SingleOrDefault(m => m.Id == userId);
                }
            }

            return (T)Convert.ChangeType(target, type);
        }

        /// <summary>
        /// Adds or updates given model to the cache,
        /// following parameter(s) is/are required for each model:
        /// 'MessageViewModel' => 'Message message',
        /// 'TeamViewModel' => 'Team team',
        /// 'TeamRoleViewModel' => 'TeamRole teamRole',
        /// 'ChannelViewModel' => 'Channel channel',
        /// 'MemberViewModel' => 'uint teamId, User user'
        /// </summary>
        /// <typeparam name="T">Type of view model to be added/updated to the cache</typeparam>
        /// <param name="parameters">Required parameter(s)</param>
        /// <returns>Added/Updated view model</returns>
        public static async Task<T> AddOrUpdate<T>(params object[] parameters)
        {
            TeamManager teamManager = App.StateProvider.TeamManager;
            MessageManager messageManager = App.StateProvider.MessageManager;
            Type type = typeof(T);
            dynamic target = default(T);

            if (parameters == null
                || parameters.Length <= 0)
            {
                return target;
            }
            else if (IsTypeOf<MessageViewModel>(type))
            {
                target = await messageManager.AddOrUpdateMessage((Message)parameters.First());
            }
            else if (IsTypeOf<IEnumerable<MessageViewModel>>(type))
            {
                target = await messageManager.AddOrUpdateMessage((IEnumerable<Message>)parameters.First());
            }
            else if (IsTypeOf<TeamViewModel>(type) || IsTypeOf<PrivateChatViewModel>(type))
            {
                target = await teamManager.AddOrUpdateTeam((Team)parameters.First());
            }
            else if (IsTypeOf<IEnumerable<TeamViewModel>>(type) || IsTypeOf<IEnumerable<PrivateChatViewModel>>(type))
            {
                target = await teamManager.AddOrUpdateTeam((IEnumerable<Team>)parameters.First());
            }
            else if (IsTypeOf<TeamRoleViewModel>(type))
            {
                target = await teamManager.AddOrUpdateTeamRole((TeamRole)parameters.First());
            }
            else if (IsTypeOf<ChannelViewModel>(type))
            {
                target = teamManager.AddOrUpdateChannel((Channel)parameters.First());
            }
            else if (IsTypeOf<IEnumerable<ChannelViewModel>>(type))
            {
                target = teamManager.AddOrUpdateChannel((IEnumerable<Channel>)parameters.First());
            }
            else if (IsTypeOf<MemberViewModel>(type))
            {
                target = await teamManager.AddOrUpdateMember((uint)parameters[0], (User)parameters[1]);
            }
            else if (IsTypeOf<IEnumerable<MemberViewModel>>(type))
            {
                target = await teamManager.AddOrUpdateMember((uint)parameters[0], (IEnumerable<User>)parameters[1]);
            }

            return target;
        }

        /// <summary>
        /// Removes given model from the cache,
        /// following parameter(s) is/are required for each model:
        /// 'MessageViewModel' => 'Message message',
        /// 'TeamViewModel' => 'uint teamId',
        /// 'TeamRoleViewModel' => 'uint teamRoleId',
        /// 'ChannelViewModel' => 'uint channelId',
        /// 'MemberViewModel' => 'uint teamId, string userId'
        /// </summary>
        /// <typeparam name="T">Type of view model to be removed from the cache</typeparam>
        /// <param name="parameters">Required parameter(s)</param>
        /// <returns>Removed view model</returns>
        public static T Remove<T>(params object[] parameters)
        {
            TeamManager teamManager = App.StateProvider.TeamManager;
            MessageManager messageManager = App.StateProvider.MessageManager;
            Type type = typeof(T);
            dynamic target = default(T);

            if (parameters == null
                || parameters.Length <= 0)
            {
                return target;
            }
            else if (IsTypeOf<MessageViewModel>(type))
            {
                target = messageManager.RemoveMessage((Message)parameters.First());
            }
            else if (IsTypeOf<TeamViewModel>(type))
            {
                target = teamManager.RemoveTeam((uint)parameters.First());
            }
            else if (IsTypeOf<TeamRoleViewModel>(type))
            {
                target = teamManager.RemoveTeamRole((uint)parameters.First());
            }
            else if (IsTypeOf<ChannelViewModel>(type))
            {
                target = teamManager.RemoveChannel((uint)parameters.First());
            }
            else if (IsTypeOf<MemberViewModel>(type))
            {
                target = teamManager.RemoveMember((uint)parameters[0], (string)parameters[1]);
            }

            return target;
        }

        public static bool IsTypeOf<T>(Type type)
        {
            return typeof(T) == type;
        }

    }
}
