using Messenger.Core.Models;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers.MessageHelpers
{
    /// <summary>
    /// Manages the dictionary for storing messages with the key of channel ids, dwelling in application cache
    /// </summary>
    public class MessageManager : Observable
    {
        private readonly ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>> _messagesByChannelId = new ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>>();

        public event EventHandler<ManagerEventArgs> MessagesLoadedForChannel;

        public MessageManager()
        {

        }

        /// <summary>
        /// Gets the list of messages with the key of the given channel id
        /// </summary>
        /// <param name="channelId">Id of the team (Key)</param>
        /// <param name="messages">List of messages (Value)</param>
        /// <returns>True if the team exists, else false</returns>
        public bool TryGetMessages(uint channelId, out ObservableCollection<MessageViewModel> messages)
        {
            return _messagesByChannelId.TryGetValue(channelId, out messages);
        }

        #region Add or Update

        /// <summary>
        /// Creates an entry in dictionary with given TeamViewModel with loaded messages for each channel
        /// </summary>
        /// <param name="viewModel">TeamViewModel to create entries for</param>
        /// <returns>Task to be awaited</returns>
        public async Task CreateEntry(TeamViewModel viewModel)
        {
            if (viewModel is PrivateChatViewModel)
            {
                /* PRIVATE CHAT HAS ONLY ONE MAIN CHANNEL */
                PrivateChatViewModel chatViewModel = (PrivateChatViewModel)viewModel;

                /* LOAD FROM DATABASE */
                IEnumerable<Message> messages = await MessageBuilder.GetMessagesFromDatabase(chatViewModel.MainChannel);

                /* SKIP IF NONE EXISTS */
                if (messages == null || messages.Count() <= 0)
                {
                    return;
                }

                /* SAVE TO CACHE */
                IEnumerable<MessageViewModel> messageViewModels = await AddOrUpdateMessage(messages);

                MessagesLoadedForChannel?.Invoke(this, new ManagerEventArgs()
                {
                    Messages = messageViewModels,
                    Team = chatViewModel,
                    Channel = chatViewModel.MainChannel
                });
            }
            else
            {
                foreach (ChannelViewModel channelViewModel in viewModel.Channels)
                {
                    /* LOAD FROM DATABASE */
                    IEnumerable<Message> messages = await MessageBuilder.GetMessagesFromDatabase(channelViewModel);

                    /* SKIP IF NONE EXISTS */
                    if (messages == null || messages.Count() <= 0)
                    {
                        continue;
                    }

                    /* SAVE TO CACHE */
                    IEnumerable<MessageViewModel> messageViewModels = await AddOrUpdateMessage(messages);

                    MessagesLoadedForChannel?.Invoke(this, new ManagerEventArgs()
                    {
                        Messages = messageViewModels,
                        Team = viewModel,
                        Channel = channelViewModel
                    });
                }
            }
        }

        public async Task CreateEntry(IEnumerable<TeamViewModel> viewModels)
        {
            foreach (TeamViewModel viewModel in viewModels)
            {
                await CreateEntry(viewModel);
            }
        }

        /// <summary>
        /// Builds a MessageViewModel and adds to the dictionary, updates if exists
        /// </summary>
        /// <param name="messageData">Message data model</param>
        /// <returns>Added/updated MessageViewModel</returns>
        public async Task<MessageViewModel> AddOrUpdateMessage(Message messageData)
        {
            MessageViewModel viewModel = await MessageBuilder.Build(messageData);

            AddOrUpdateToDictionary(viewModel);

            return viewModel;
        }

        public async Task<IList<MessageViewModel>> AddOrUpdateMessage(IEnumerable<Message> messageData)
        {
            List<MessageViewModel> result = new List<MessageViewModel>();

            foreach (Message data in messageData)
            {
                MessageViewModel viewModel = await AddOrUpdateMessage(data);
                result.Add(viewModel);
            }

            return result;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Finds and removes the MessageViewModel with the given Message data model
        /// </summary>
        /// <param name="data">Message data model to be searched with</param>
        public MessageViewModel RemoveMessage(Message data)
        {
            if (data.ParentMessageId == null
                && _messagesByChannelId.TryGetValue(
                    data.RecipientId,
                    out ObservableCollection<MessageViewModel> messages))
            {
                MessageViewModel target = messages.Single(message => message.Id == data.Id);

                if (target != null)
                {
                    messages.Remove(target);

                    return target;
                }
            }

            if (data.ParentMessageId != null
                && _messagesByChannelId.TryGetValue(
                    data.RecipientId,
                    out ObservableCollection<MessageViewModel> parents))
            {
                MessageViewModel targetParent = parents.Single(p => p.Id == data.ParentMessageId);
                MessageViewModel target = targetParent.Replies.Single(r => r.Id == data.Id);

                targetParent.Replies.Remove(target);

                return target;
            }

            return null;
        }

        /// <summary>
        /// Removes entry with the given key
        /// </summary>
        /// <param name="channelId">Id of the channel</param>
        /// <returns>Removed channel object</returns>
        public ChannelViewModel RemoveEntry(uint channelId)
        {
            if (_messagesByChannelId.TryRemove(channelId, out ObservableCollection<MessageViewModel> entry))
            {
                return CacheQuery.Get<ChannelViewModel>(channelId);
            }
            else
            {
                return null;
            }
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Adds the message to the dictionary
        /// </summary>
        /// <param name="message">MessageViewModel to add</param>
        private void AddOrUpdateToDictionary(MessageViewModel message)
        {
            if (!message.IsReply)
            {
                _messagesByChannelId.AddOrUpdate(
                    (uint)message.ChannelId,
                    new ObservableCollection<MessageViewModel>() { message },
                    (key, list) =>
                    {
                        if (list.Any(m => m.Id == message.Id))
                        {
                            MessageViewModel target = list.SingleOrDefault(m => m.Id == message.Id);
                            int index = list.IndexOf(target);

                            list[index] = message;
                        }
                        else
                        {
                            list.Add(message);
                        }

                        return list;
                    });
            }
            else
            {
                _messagesByChannelId.AddOrUpdate(
                    (uint)message.ChannelId,
                    new ObservableCollection<MessageViewModel>() { message },
                    (key, list) =>
                    {
                        foreach (MessageViewModel viewModel in list)
                        {
                            if (viewModel.Id == message.ParentMessageId)
                            {
                                if (viewModel.Replies.Any(r => r.Id == message.Id))
                                {
                                    MessageViewModel target = viewModel.Replies.SingleOrDefault(r => r.Id == message.Id);
                                    int index = viewModel.Replies.IndexOf(target);

                                    viewModel.Replies[index] = message;
                                }
                                else
                                {
                                    viewModel.Replies.Add(message);
                                }
                            }
                        }

                        return list;
                    });
            }
        }

        #endregion
    }
}
