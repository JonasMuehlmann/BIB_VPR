using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers.TeamHelpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.Helpers.MessageHelpers
{
    /// <summary>
    /// Loads and creates complete MessageViewModels to be saved in MessageManager
    /// </summary>
    public static class MessageBuilder
    {

        /// <summary>
        /// loads all messages in channel from the db
        /// </summary>
        /// <param name="channel"></param>
        /// <returns>Task<IEnumerable<Message>> all messages from channel</returns>
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
            MessageViewModel withSender = await Map(message).WithSender();
            MessageViewModel withReactions = await withSender.WithReactions();
            MessageViewModel withReplies = await withReactions.WithReplies();
            MessageViewModel withAttachments = await withReplies.WithAttachments();

            return await withAttachments.WithParsedMentions();
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
                    .Where(r => r.UserId == App.StateProvider.CurrentUser.Id).FirstOrDefault();

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


        /// <summary>
        /// Loads replies for the given view model
        /// </summary>
        /// <param name="viewModel">MessageViewModel to load replies for</param>
        /// <returns>MessageViewModel with the list of replies</returns>
        public static async Task<MessageViewModel> WithReplies(this MessageViewModel viewModel)
        {
            /* LOAD REACTIONS */
            IEnumerable<Message> replies = await MessageService.RetrieveReplies((uint)viewModel.Id);

            if (replies != null && replies.Count() > 0)
            {
                List<MessageViewModel> replyViewModels = new List<MessageViewModel>();

                foreach (var reply in replies) {
                    MessageViewModel withSender = await WithSender(Map(reply));
                    withSender = await withSender.WithReactions();
                    replyViewModels.Add(withSender);
                }
                //replyViewModels = SortReplies(replyViewModels).ToList();

                viewModel.Replies = new ObservableCollection<MessageViewModel>(replyViewModels);
            }
            else
            {
                viewModel.Replies = new ObservableCollection<MessageViewModel>();
            }

            return viewModel;
        }

        /// <summary>
        /// loads the sender to the ViewModel
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>Task<MessageViewModel> with senders</returns>
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
        /// loads the attachments to the viewmodel
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns>Task<MessageViewModel> with attachments</returns>
        public static async Task<MessageViewModel> WithAttachments(this MessageViewModel viewModel)
        {
            IEnumerable<string> blobNames = await MessageService.GetBlobFileNamesOfAttachments((uint)viewModel.Id);

            if (blobNames != null && blobNames.Count() > 0)
            {
                List<Attachment> attachements = blobNames.Parse();
                List<MemoryStream> imageStreams = new List<MemoryStream>();
                foreach (var item in attachements)
                {
                    if(item.FileType == "jpg" || item.FileType == "jpeg" || item.FileType == "gif" || item.FileType == "png" || item.FileType == "gif")
                    {
                        imageStreams.Add(await FileSharingService.Download(item.ToBlobName()));
                    }
                    else
                    {
                        viewModel.Attachments.Add(item);
                    }
                }
                if (imageStreams.Count > 0) {
                    viewModel.AddImages(imageStreams);
                }
            }

            return viewModel;
        }

        public static async Task<MessageViewModel> WithParsedMentions(this MessageViewModel viewModel)
        {
            viewModel.Content = await MentionService.ResolveMentions(viewModel.Content);

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
            bool hasAttachments = (message.AttachmentsBlobName != null && message.AttachmentsBlobName.Count() > 0);

            return new MessageViewModel()
            {
                Id = message.Id,
                SenderId = message.SenderId,
                ParentMessageId = message.ParentMessageId,
                Content = message.Content,
                CreationTime = message.CreationTime,
                ChannelId = message.RecipientId,
                Attachments = hasAttachments ? message.AttachmentsBlobName.Parse() : new List<Attachment>(),
                IsReply = isReply,
                IsMyMessage = App.StateProvider.CurrentUser.Id == message.SenderId,
                HasReacted = false
            };
        }

        /// <summary>
        /// converts the user to an UserViewModel
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
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
                if (blobData.Length >= 3)
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
            }

            return attachmentsList;
        }

        /// <summary>
        /// converts the attachment to the download name
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public static string ToBlobName(this Attachment attachment)
        {
            if (attachment == null)
            {
                return string.Empty;
            }

            string blobName = string.Join('.', attachment.FileName, attachment.FileType, attachment.UploaderId);

            return blobName;
        }

        /// <summary>
        /// converts a list of attachments to donwloadnames
        /// </summary>
        /// <param name="attachments"></param>
        /// <returns></returns>
        public static IEnumerable<string> ToBlobNames(this List<Attachment> attachments)
        {
            if (attachments == null || attachments.Count() <= 0)
            {
                return Enumerable.Empty<string>();
            }

            List<string> blobNamesList = new List<string>();

            foreach (Attachment attachment in attachments)
            {
                string blobName = string.Join('.', attachment.FileName, attachment.FileType, attachment.UploaderId);
                blobNamesList.Add(blobName);
            }

            return blobNamesList;
        }

        #endregion
    }
}
