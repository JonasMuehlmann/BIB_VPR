using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers
{
    /// <summary>
    /// Converts and completes MessageViewModels to be shown on UI
    /// </summary>
    public class MessageBuilder
    {
        /// <summary>
        /// Converts the given Message data model to MessageViewModel
        /// </summary>
        /// <param name="message">Message data model to be converted</param>
        /// <returns>A complete MessageViewModel object</returns>
        public async Task<MessageViewModel> Build(Message message, UserViewModel currentUser)
        {
            MessageViewModel vm = Map(message);

            vm = await AssignReaction(vm);

            if (vm.Reactions.Count > 0)
            {
                MarkMyReaction(ref vm, currentUser.Id);
            }

            vm.IsMyMessage = currentUser.Id == vm.SenderId;

            if (vm.Sender == null)
            {
                vm = await GetSender(vm);
            }

            return vm;
        }

        /// <summary>
        /// Converts the given Message data models to MessageViewModels
        /// </summary>
        /// <param name="messages">Message data models to be converted</param>
        /// <returns>List of complete MessageViewModel objects</returns>
        public async Task<IEnumerable<MessageViewModel>> Build(IEnumerable<Message> messages, UserViewModel currentUser)
        {
            var result = new List<MessageViewModel>();

            foreach (Message message in messages)
            {
                result.Add(await Build(message, currentUser));
            }

            return result;
        }

        /// <summary>
        /// Loads reactions for the given view model
        /// </summary>
        /// <param name="viewModel">MessageViewModel to load reactions for</param>
        /// <returns>MessageViewModel with the list of reactions</returns>
        public async Task<MessageViewModel> AssignReaction(MessageViewModel viewModel)
        {
            // Loads the latest reactions made on the message
            var reactions = await MessengerService.GetReactions((uint)viewModel.Id);
            if (reactions != null && reactions.Count() > 0)
            {
                viewModel.Reactions = new ObservableCollection<Reaction>(reactions);
            }
            else
            {
                viewModel.Reactions = new ObservableCollection<Reaction>();
            }

            return viewModel;
        }

        /// <summary>
        /// Sorts parent-messages/replies and assigns replies to matching parents
        /// this should be called only after converting all loaded messages to view models
        /// </summary>
        /// <param name="viewModels">List of MessageViewModel to sort</param>
        /// <returns>List of parent messages with assigned replies</returns>
        public IList<MessageViewModel> AssignReplies(IEnumerable<MessageViewModel> viewModels)
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

        private async Task<MessageViewModel> GetSender(MessageViewModel viewModel)
        {
            User sender = await UserService.GetUser(viewModel.SenderId);

            viewModel.Sender = sender;

            return viewModel;
        }

        /// <summary>
        /// Maps the properties from the data model
        /// </summary>
        /// <param name="message">Message data model to map from</param>
        /// <returns>Mapped MessageViewModel object</returns>
        private MessageViewModel Map(Message message)
        {
            bool isReply = (message.ParentMessageId != null) ? true : false;

            return new MessageViewModel()
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ParentMessageId = message.ParentMessageId,
                Sender = message.Sender,
                Content = message.Content,
                CreationTime = message.CreationTime,
                ChannelId = message.RecipientId,
                Attachments = ParseBlobName(message.AttachmentsBlobName),
                IsReply = isReply,
                HasReacted = false
            };
        }

        private void MarkMyReaction(ref MessageViewModel viewModel, string userId)
        {
            // Mark my reaction if exists
            var myReaction = viewModel.Reactions
                .Where(r => r.UserId == userId);

            if (myReaction.Count() > 0)
            {
                viewModel.HasReacted = true;
                viewModel.MyReaction = (ReactionType)Enum.Parse(
                    typeof(ReactionType),
                    myReaction.FirstOrDefault().Symbol);
            }
        }

        /// <summary>
        /// Parses the attachments blob name of a message and maps to Attachment models
        /// </summary>
        /// <param name="blobName">Blob name to parse</param>
        /// <returns>List of Attachment objects</returns>
        private List<Attachment> ParseBlobName(IEnumerable<string> blobName)
        {
            var attachmentsList = new List<Attachment>();
            string[][] data = blobName
                .Select(b => b.Split('.'))
                .ToArray();

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
    }
}
