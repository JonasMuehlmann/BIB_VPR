﻿using Messenger.Core.Models;
using Messenger.ViewModels.DataViewModels;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Messenger.Helpers
{
    public class MessageManager
    {
        private readonly ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>> _messagesByTeamId = new ConcurrentDictionary<uint, ObservableCollection<MessageViewModel>>();

        public void CreateEntry(uint teamId, IEnumerable<MessageViewModel> messages)
        {
            _messagesByTeamId.AddOrUpdate(
                teamId,
                (key) =>
                new ObservableCollection<MessageViewModel>(messages),
                (key, list) =>
                {
                    list.Clear();
                    foreach (var message in messages)
                    {
                        list.Add(message);
                    }

                    return list;
                });
        }

        public bool TryGetMessages(uint teamId, out ObservableCollection<MessageViewModel> messages)
        {
            return _messagesByTeamId.TryGetValue(teamId, out messages);
        }

        public void Add(MessageViewModel message)
        {
            if (!message.IsReply)
            {
                _messagesByTeamId.AddOrUpdate(
                    (uint)message.TeamId,
                    new ObservableCollection<MessageViewModel>() { message },
                    (key, list) =>
                    {
                        list.Add(message);
                        return list;
                    });
            }
            else
            {
                _messagesByTeamId.AddOrUpdate(
                    (uint)message.TeamId,
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

        public void Update(MessageViewModel message)
        {
            if (!message.IsReply)
            {
                _messagesByTeamId.AddOrUpdate(
                    (uint)message.TeamId,
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
                _messagesByTeamId.AddOrUpdate(
                    (uint)message.TeamId,
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

        public void Remove(Message data)
        {
            if (data.ParentMessageId == null)
            {
                _messagesByTeamId.AddOrUpdate(
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
                if (_messagesByTeamId.TryGetValue(
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
    }
}
