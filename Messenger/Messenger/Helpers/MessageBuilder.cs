using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Helpers
{
    public class MessageBuilder
    {
        private MessengerService MessengerService => Singleton<MessengerService>.Instance;

        public async Task<MessageViewModel> Build(Message message)
        {
            MessageViewModel vm = Map(message);

            vm = await AssignReaction(vm);

            return vm;
        }

        public async Task<IEnumerable<MessageViewModel>> Build(IEnumerable<Message> messages)
        {
            var result = new List<MessageViewModel>();

            foreach (Message message in messages)
            {
                result.Add(await Build(message));
            }

            return result;
        }

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
                TeamId = message.RecipientId,
                Attachments = ParseBlobName(message.AttachmentsBlobName),
                IsReply = isReply,
                HasReacted = false
            };
        }

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
