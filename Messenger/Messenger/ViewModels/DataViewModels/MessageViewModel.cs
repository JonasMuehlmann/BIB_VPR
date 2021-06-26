using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Messenger.ViewModels.DataViewModels
{
    public class MessageViewModel : Observable
    {
        #region Private

        private uint? _id;
        private string _senderId;
        private string _content;
        private DateTime _creationTime;
        private uint? _teamId;
        private uint? _parentMessageId;
        private bool _isReply;
        private User _sender;
        private ObservableCollection<MessageViewModel> _replies;
        private List<Attachment> _attachments;

        #endregion

        public uint? Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        public string SenderId
        {
            get { return _senderId; }
            set { Set(ref _senderId, value); }
        }

        public string Content
        {
            get { return _content; }
            set { Set(ref _content, value); }
        }

        public DateTime CreationTime
        {
            get { return _creationTime; }
            set { Set(ref _creationTime, value); }
        }

        public uint? TeamId
        {
            get { return _teamId; }
            set { Set(ref _teamId, value); }
        }

        public uint? ParentMessageId
        {
            get { return _parentMessageId; }
            set { Set(ref _parentMessageId, value); }
        }

        public User Sender
        {
            get { return _sender; }
            set { Set(ref _sender, value); }
        }

        public ObservableCollection<MessageViewModel> Replies
        {
            get { return _replies; }
            set { Set(ref _replies, value); }
        }

        public List<Attachment> Attachments
        {
            get { return _attachments; }
            set { Set(ref _attachments, value); }
        }

        public bool IsReply
        {
            get { return _isReply; }
            set { Set(ref _isReply, value); }
        }

        public MessageViewModel()
        {
        }

        /// <summary>
        /// Builds the message view model from the DB model
        /// </summary>
        /// <param name="messages">List of messages loaded from the database</param>
        /// <returns>List of parent messages</returns>
        public static IList<MessageViewModel> FromDbModel(IEnumerable<Message> messages)
        {
            var parents = new List<MessageViewModel>();
            var replies = new List<MessageViewModel>();

            // Sorts out replies from the list
            foreach (Message message in messages)
            {
                var type = ConvertAndGetType(message, out MessageViewModel viewModel);

                switch (type)
                {
                    case MessageType.Parent:
                        parents.Add(viewModel);
                        break;
                    case MessageType.Reply:
                        replies.Add(viewModel);
                        break;
                    default:
                        break;
                }
            }

            // Assign replies to parent messages
            foreach (MessageViewModel parent in parents)
            {
                if (replies.Any(r => r.ParentMessageId == parent.Id))
                {
                    parent.Replies = new ObservableCollection<MessageViewModel>
                        (replies.Where(r => r.ParentMessageId == parent.Id));
                }
            }

            return parents;
        }

        public static MessageType ConvertAndGetType(Message message, out MessageViewModel viewModel)
        {
            var attachmentsList = new List<Attachment>();
            bool isReply = (message.ParentMessageId != null) ? true : false;

            if (message.AttachmentsBlobName.Count > 0)
            {
                string[][] data = message.AttachmentsBlobName
                    .Select(b => b.Split('.'))
                    .ToArray();

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
            }

            viewModel = new MessageViewModel()
            {
                Id = message.Id,
                SenderId = message.SenderId,
                Sender = message.Sender,
                Content = message.Content,
                CreationTime = message.CreationTime,
                TeamId = message.RecipientId,
                Replies = new ObservableCollection<MessageViewModel>(),
                Attachments = attachmentsList,
                IsReply = isReply
            };

            return isReply ? MessageType.Reply : MessageType.Parent;
        }
    }

    public enum MessageType
    {
        Parent,
        Reply
    }
}
