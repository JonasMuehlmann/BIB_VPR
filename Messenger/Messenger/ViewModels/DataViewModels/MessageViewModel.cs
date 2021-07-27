using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.ViewModels.DataViewModels
{
    public class MessageViewModel : DataViewModel
    {
        #region Private

        private uint? _id;
        private string _senderId;
        private string _content;
        private DateTime _creationTime;
        private uint? _channelId;
        private uint? _parentMessageId;
        private bool _isReply;
        private bool _hasReacted;
        private ReactionType _myReaction;
        private UserViewModel _sender;
        private ObservableCollection<MessageViewModel> _replies;
        private ObservableCollection<Reaction> _reactions;
        private List<Attachment> _attachments;
        private bool _isMyMessage;

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

        public uint? ChannelId
        {
            get { return _channelId; }
            set { Set(ref _channelId, value); }
        }

        public uint? ParentMessageId
        {
            get { return _parentMessageId; }
            set { Set(ref _parentMessageId, value); }
        }

        public UserViewModel Sender
        {
            get { return _sender; }
            set { Set(ref _sender, value); }
        }

        public ObservableCollection<MessageViewModel> Replies
        {
            get { return _replies; }
            set { Set(ref _replies, value); }
        }

        public ObservableCollection<Reaction> Reactions
        {
            get { return _reactions; }
            set { Set(ref _reactions, value); }
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

        public bool HasReacted
        {
            get { return _hasReacted; }
            set { Set(ref _hasReacted, value); }
        }

        public bool IsMyMessage
        {
            get { return _isMyMessage; }
            set { Set(ref _isMyMessage, value); }
        }

        public ReactionType MyReaction
        {
            get
            {
                if (!HasReacted)
                {
                    return ReactionType.None;
                }
                else
                {
                    return _myReaction;
                }
            }
            set
            {
                Set(ref _myReaction, value);
            }
        }

        public MessageViewModel()
        {
            HasReacted = false;
            Replies = new ObservableCollection<MessageViewModel>();
            Reactions = new ObservableCollection<Reaction>();
            Attachments = new List<Attachment>();
        }
    }
}
