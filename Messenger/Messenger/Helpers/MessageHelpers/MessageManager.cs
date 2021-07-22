using Messenger.Core.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers.MessageHelpers
{
    /// <summary>
    /// Handles the dictionary for storing messages with the key of channel ids.
    /// </summary>
    public class MessageManager : Observable
    {
        private readonly MessageBuilder _builder;

        private readonly ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>> _messagesByChannelId = new ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>>();

        public MessageManager()
        {
            _builder = new MessageBuilder();
        }

        public async Task<MessageViewModel> AddMessage(Message messageData)
        {
            MessageViewModel viewModel = await _builder.Build(messageData);

            AddToDictionary(viewModel);

            return viewModel;
        }

        public async Task<IList<MessageViewModel>> AddMessage(IEnumerable<Message> messageData)
        {
            List<MessageViewModel> result = new List<MessageViewModel>();

            foreach (Message data in messageData)
            {
                MessageViewModel viewModel = await AddMessage(data);
                result.Add(viewModel);
            }

            return result;
        }

        public ObservableCollection<MessageViewModel> GetMessages(uint channelId)
        {
            if (TryGetMessages(channelId, out ObservableCollection<MessageViewModel> messages))
            {
                return messages;
            }
            else
            {
                return null;
            }
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

        public bool TryGetLastMessage(uint channelId, out MessageViewModel message)
        {
            if (TryGetMessages(channelId, out ObservableCollection<MessageViewModel> messages)
                && messages.Count > 0)
            {
                message = messages.LastOrDefault();
                return true;
            }
            else
            {
                message = null;
                return false;
            }
        }

        public async Task<MessageViewModel> UpdateMessage(Message messageData)
        {
            MessageViewModel viewModel = await _builder.Build(messageData);

            FindAndUpdate(viewModel);

            return viewModel;
        }

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
        /// <returns>Count of messages deleted</returns>
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

        #region Helpers

        /// <summary>
        /// Adds the message to the dictionary
        /// </summary>
        /// <param name="message">MessageViewModel to add</param>
        private void AddToDictionary(MessageViewModel message)
        {
            if (!message.IsReply)
            {
                _messagesByChannelId.AddOrUpdate(
                    (uint)message.ChannelId,
                    new ObservableCollection<MessageViewModel>() { message },
                    (key, list) =>
                    {
                        list.Add(message);
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
                                viewModel.Replies.Add(message);
                            }
                        }

                        return list;
                    });
            }
        }

        /// <summary>
        /// Finds and updates the message in the dictionary
        /// </summary>
        /// <param name="message">MessageViewModel to update</param>
        private void FindAndUpdate(MessageViewModel message)
        {
            if (!message.IsReply)
            {
                _messagesByChannelId.AddOrUpdate(
                    (uint)message.ChannelId,
                    new ObservableCollection<MessageViewModel>() { message },
                    (key, list) =>
                    {
                        var result = list.Select(m =>
                        {
                            if (m.Id == message.Id)
                            {
                                m = message;
                            }

                            return m;
                        });

                        return new ObservableCollection<MessageViewModel>(result);
                    });
            }
            else
            {
                _messagesByChannelId.AddOrUpdate(
                    (uint)message.ChannelId,
                    new ObservableCollection<MessageViewModel>() { message },
                    (key, list) =>
                    {
                        var result = list.Select(m =>
                        {
                            if (m.Id == message.ParentMessageId)
                            {
                                var updated = m.Replies.Select(r =>
                                {
                                    if (r.Id == message.Id)
                                    {
                                        r = message;
                                    }

                                    return r;
                                });

                                m.Replies = new ObservableCollection<MessageViewModel>(updated);
                            }

                            return m;
                        });

                        return new ObservableCollection<MessageViewModel>(result);
                    });
            }
        }

        #endregion
    }
}
