using Messenger.Commands.Messenger;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Messenger.ViewModels.Controls
{
    public class MessagesListControlViewModel : Observable
    {
        private ObservableCollection<MessageViewModel> _messages;

        /// <summary>
        /// Loaded messages of the current team/chat
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages
        {
            get { return _messages; }
            set { Set(ref _messages, value); }
        }

        public ChatViewModel ParentViewModel { get; private set; }

        public ICommand ToggleReactionCommand { get => new ToggleReactionCommand(); }

        public ICommand DeleteMessageCommand { get => new DeleteMessageCommand(); }

        public ICommand UpdateMessageCommand { get => new UpdateMessageCommand(); }

        public ICommand ReplyToCommand { get => new ReplyMessageCommand(ParentViewModel); }

        public MessagesListControlViewModel(ChatViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
            Initialize();
        }

        private void Initialize()
        {
            Messages = new ObservableCollection<MessageViewModel>();

            /** LOAD FROM CACHE **/
            if (App.StateProvider != null)
            {
                if (CacheQuery.TryGetMessages(
                        ParentViewModel.SelectedChannel.ChannelId,
                        out ObservableCollection<MessageViewModel> messages))
                {
                    Messages.Clear();

                    foreach (MessageViewModel message in messages)
                    {
                        Messages.Add(message);
                    }
                }
            }
        }

        public void OnMessageUpdated(object sender, BroadcastArgs e)
        {
            MessageViewModel message = e.Payload as MessageViewModel;

            if (message == null) return;

            if (e.Reason == BroadcastReasons.Created)
            {
                if (!message.IsReply)
                {
                    Messages.Add(message);
                }
            }
            else if (e.Reason == BroadcastReasons.Updated)
            {
                MessageViewModel target = Messages.FirstOrDefault(m => m.Id == message.Id);

                if (target != null)
                {
                    int index = Messages.IndexOf(target);

                    Messages[index] = message;
                }
            }
            else if (e.Reason == BroadcastReasons.Deleted)
            {
                if (!message.IsReply )
                {
                    MessageViewModel target = Messages.Single(m => m.Id == message.Id);

                    if (target != null)
                    {
                        Messages.Remove(target);
                    }
                }
                //else
                //{
                //    MessageViewModel target = Messages.Single(m => m.Id == message.ParentMessageId);


                //    if ( target != null)
                //    {
                //        MessageViewModel targetReply = target.Replies.Single(m => m.Id == message.Id);
                //        if (targetReply != null)
                //        {
                //            target.Replies.Remove(targetReply);
                //        }
                //    }
                //}
            }
        }

        public void OnMessagesSwitched(object sender, BroadcastArgs e)
        {
            IEnumerable<MessageViewModel> messages = e.Payload as IEnumerable<MessageViewModel>;

            if (messages != null && messages.Count() > 0)
            {
                Messages.Clear();

                foreach (MessageViewModel message in messages)
                {
                    Messages.Add(message);
                }
            }
        }
    }
}
