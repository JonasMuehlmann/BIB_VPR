using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers.TeamHelpers;
using Messenger.Models;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers.MessageHelpers
{
    /// <summary>
    /// Converts and completes MessageViewModels to be shown on UI
    /// </summary>
    public static class MessageBuilder
    {
        public static async Task<IEnumerable<Message>> GetMessagesFromDatabase(ChannelViewModel channel)
        {
            return await MessengerService.GetMessages(channel.ChannelId);
        }

        /// <summary>
        /// Converts the given Message data model to MessageViewModel
        /// </summary>
        /// <param name="message">Message data model to be converted</param>
        /// <returns>A complete MessageViewModel object</returns>
        public static async Task<MessageViewModel> Build(this Message message)
        {
            MessageViewModel withReactions = await Map(message).WithReactions();

            if (withReactions.Sender == null)
            {
                return await withReactions.WithSender();
            }
            else
            {
                return withReactions;
            }
        }

        /// <summary>
        /// Converts the given Message data models to MessageViewModels
        /// </summary>
        /// <param name="messages">Message data models to be converted</param>
        /// <returns>List of complete MessageViewModel objects</returns>
        public static async Task<IEnumerable<MessageViewModel>> Build(this IEnumerable<Message> messages)
        {
            var result = new List<MessageViewModel>();

            foreach (Message message in messages)
            {
                result.Add(await Build(message));
            }

            return result;
        }

        /// <summary>
        /// Loads reactions for the given view model
        /// </summary>
        /// <param name="viewModel">MessageViewModel to load reactions for</param>
        /// <returns>MessageViewModel with the list of reactions</returns>
        public static async Task<MessageViewModel> WithReactions(this MessageViewModel viewModel)
        {
            /* LOAD REACTIONS */
            IEnumerable<Reaction> reactions = await MessengerService.GetReactions((uint)viewModel.Id);

            if (reactions != null && reactions.Count() > 0)
            {
                viewModel.Reactions = new ObservableCollection<Reaction>(reactions);

                /* MARK MY REACTION IF EXISTS */
                Reaction myReaction = viewModel.Reactions
                    .Where(r => r.UserId == App.StateProvider.CurrentUser.Id)
                    .SingleOrDefault();

                if (myReaction != null)
                {
                    viewModel.HasReacted = true;
                    viewModel.MyReaction = (ReactionType)Enum.Parse(
                        typeof(ReactionType),
                        myReaction.Symbol);
                }
            }
            else
            {
                viewModel.Reactions = new ObservableCollection<Reaction>();
            }

            return viewModel;
        }

        public static async Task<MessageViewModel> WithSender(this MessageViewModel viewModel)
        {
            ChannelViewModel recipient = TeamBuilder.Map(await ChannelService.GetChannel((uint)viewModel.ChannelId));
            User sender = await UserService.GetUser(viewModel.SenderId);

            if (recipient == null || sender == null)
            {
                return viewModel;
            }

            viewModel.Sender = Map(sender).ToMemberViewModel(recipient.TeamId);

            return viewModel;
        }

        /// <summary>
        /// Sorts parent-messages/replies and assigns replies to matching parents
        /// this should be called only after converting all loaded messages to view models
        /// </summary>
        /// <param name="viewModels">List of MessageViewModel to sort</param>
        /// <returns>List of parent messages with assigned replies</returns>
        public static IList<MessageViewModel> SortReplies(this IEnumerable<MessageViewModel> viewModels)
        {
            List<MessageViewModel> parents = new List<MessageViewModel>();
            List<MessageViewModel> replies = new List<MessageViewModel>();

            foreach (MessageViewModel vm in viewModels)
            {
                if (vm.IsReply)
                {
                    replies.Add(vm);
                }
                else
                {
                    parents.Add(vm);
                }
            }

            foreach (MessageViewModel reply in replies)
            {
                MessageViewModel parent = parents
                    .Where(p => p.Id == reply.ParentMessageId)
                    .FirstOrDefault();

                if (parent != null)
                {
                    parent.Replies.Add(reply);
                }
            }

            return parents;
        }

        /// <summary>
        /// Maps the properties from the data model
        /// </summary>
        /// <param name="message">Message data model to map from</param>
        /// <returns>Mapped MessageViewModel object</returns>
        public static MessageViewModel Map(Message message)
        {
            bool isReply = (message.ParentMessageId != null) ? true : false;

            return new MessageViewModel()
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ParentMessageId = message.ParentMessageId,
                Content = message.Content,
                CreationTime = message.CreationTime,
                ChannelId = message.RecipientId,
                Attachments = message.AttachmentsBlobName.Parse(),
                IsReply = isReply,
                IsMyMessage = App.StateProvider.CurrentUser.Id == message.SenderId,
                HasReacted = false
            };
        }

        public static UserViewModel Map(User user)
        {
            if (user == null)
            {
                return null;
            }

            return new UserViewModel()
            {
                Id = user.Id,
                Name = user.DisplayName,
                NameId = user.NameId,
                Bio = user.Bio,
                Mail = user.Mail
            };
        }

        #region Helpers

        /// <summary>
        /// Parses the attachments blob name of a message and maps to Attachment models
        /// </summary>
        /// <param name="blobName">Blob name to parse</param>
        /// <returns>List of Attachment objects</returns>
        public static List<Attachment> Parse(this IEnumerable<string> blobName)
        {
            List<Attachment> attachmentsList = new List<Attachment>();
            string[][] data = blobName.Select(b => b.Split('.')).ToArray();

            if (data.GetLength(0) <= 0)
            {
                return attachmentsList;
            }

            foreach (string[] blobData in data)
            {
                string fileName = blobData[0];
                string fileType = blobData[1];
                string uploaderId = blobData[2];

                attachmentsList.Add(new Attachment()
                {
                    FileName = fileName,
                    FileType = fileType,
                    UploaderId = uploaderId
                });
            }

            return attachmentsList;
        }

        #endregion
    }
}
