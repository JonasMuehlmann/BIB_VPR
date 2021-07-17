using Messenger.Core.Models;
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
        private UserViewModel _currentUser;

        private readonly ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>> _messagesByChannelId = new ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>>();

        public UserViewModel CurrentUser
        {
            get { return _currentUser; }
            set { Set(ref _currentUser, value); }
        }

        public MessageManager(MessageBuilder builder)
        {
            _builder = builder;
        }

        public static MessageManager CreateMessageManager()
        {
            MessageBuilder builder = new MessageBuilder();

            return new MessageManager(builder);
        }

        public async Task<MessageViewModel> AddMessage(Message messageData)
        {
            MessageViewModel viewModel = await _builder.Build(messageData, CurrentUser);

            Add(viewModel);

            return viewModel;
        }

        public async Task AddMessage(IEnumerable<Message> messageData)
        {
            foreach (Message data in messageData)
            {
                await AddMessage(data);
            }
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
            MessageViewModel viewModel = await _builder.Build(messageData, CurrentUser);

            Update(viewModel);

            return viewModel;
        }

        /// <summary>
        /// Finds and updates the message in the dictionary
        /// </summary>
        /// <param name="message">MessageViewModel to update</param>
        private void Update(MessageViewModel message)
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

        /// <summary>
        /// Finds and removes the MessageViewModel with the given Message data model
        /// </summary>
        /// <param name="data">Message data model to be searched with</param>
        public void RemoveMessage(Message data)
        {
            if (data.ParentMessageId == null)
            {
                _messagesByChannelId.AddOrUpdate(
                    data.RecipientId,
                    new ObservableCollection<MessageViewModel>(),
                    (key, list) =>
                    {
                        var updated = list
                            .Where(m => m.Id != data.Id);

                        return new ObservableCollection<MessageViewModel>(updated);
                    });
            }
            else
            {
                if (_messagesByChannelId.TryGetValue(
                    data.RecipientId,
                    out ObservableCollection<MessageViewModel> parents))
                {
                    foreach (MessageViewModel viewModel in parents)
                    {
                        if (viewModel.Id == data.ParentMessageId)
                        {
                            var updated = viewModel.Replies.Where(r => r.Id != data.Id);

                            viewModel.Replies = new ObservableCollection<MessageViewModel>(updated);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Removes entry with the given key
        /// </summary>
        /// <param name="channelId">Id of the channel</param>
        /// <returns>Count of messages deleted</returns>
        public int RemoveEntry(uint channelId)
        {
            bool entryExists = _messagesByChannelId.TryRemove(channelId, out ObservableCollection<MessageViewModel> entry);

            if (entryExists)
            {
                return entry.Count();
            }
            else
            {
                return 0;
            }
        }

        /// <summary>
        /// Adds the message to the dictionary
        /// </summary>
        /// <param name="message">MessageViewModel to add</param>
        private void Add(MessageViewModel message)
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
    }
}
