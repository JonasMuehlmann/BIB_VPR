﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.Messenger;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using Messenger.Views.DialogBoxes;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ChatViewModel : Observable
    {
        #region Private

        private ObservableCollection<MessageViewModel> _messages;
        private IReadOnlyList<StorageFile> _selectedFiles;
        private ChatHubService Hub => Singleton<ChatHubService>.Instance;
        private MessageViewModel _replyMessage;
        private Message _messageToSend;
        private ChannelViewModel _currentChannel;
        private TeamViewModel _currentTeam;

        #endregion

        #region Properties

        /// <summary>
        /// Loaded messages of the current team/chat
        /// </summary>
        public ObservableCollection<MessageViewModel> Messages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
                Set(ref _messages, value);
            }
        }

        /// <summary>
        /// Attachments list to upload with the message
        /// </summary>
        public IReadOnlyList<StorageFile> SelectedFiles {
            get
            {
                return _selectedFiles;
            }
            set
            {
                Set(ref _selectedFiles, value);
            }
        }

        /// <summary>
        /// Message that the user is replying to
        /// </summary>
        public MessageViewModel ReplyMessage
        {
            get
            {
                return _replyMessage;
            }
            set
            {
                Set(ref _replyMessage, value);
            }
        }

        /// <summary>
        /// Message object to be sent
        /// </summary>
        public Message MessageToSend
        {
            get
            {
                return _messageToSend;
            }
            set
            {
                Set(ref _messageToSend, value);
            }
        }

        public TeamViewModel CurrentTeam
        {
            get { return _currentTeam; }
            set { Set(ref _currentTeam, value); }
        }

        public ChannelViewModel CurrentChannel
        {
            get { return _currentChannel; }
            set { Set(ref _currentChannel, value); }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Sends the current MessageToSend model
        /// </summary>
        public ICommand SendMessageCommand => new SendMessageCommand(this, Hub);

        /// <summary>
        /// Open file open picker for attachments on the message
        /// </summary>
        public ICommand OpenFilesCommand => new OpenFilesCommand(this);

        /// <summary>
        /// Marks the current MessageToSend model as a reply
        /// </summary>
        public ICommand ReplyToCommand => new ReplyToCommand(this);

        public ICommand EditMessageCommand => new EditMessageCommand(Hub);

        public ICommand DeleteMessageCommand => new DeleteMessageCommand(Hub);

        public ICommand ToggleReactionCommand => new ToggleReactionCommand(Hub);

        public ICommand OpenTeamManagerCommand => new RelayCommand(() => NavigationService.Open<TeamManagePage>());

        public ICommand OpenSettingsCommand => new RelayCommand(() => NavigationService.Open<SettingsPage>());

        public ICommand EditTeamDetailsCommand => new RelayCommand(EditTeamDetails);

        #endregion

        public ChatViewModel()
        {
            // Initialize models
            Messages = new ObservableCollection<MessageViewModel>();
            MessageToSend = new Message();

            // Register events
            Hub.TeamSwitched += OnTeamSwitched;
            Hub.MessageReceived += OnMessageReceived;
            Hub.MessageUpdated += OnMessagesUpdated;
            Hub.MessageDeleted += OnMessagesUpdated;

            LoadAsync();
        }
         
        /// <summary>
        /// Loads messages from the hub
        /// </summary>
        private async void LoadAsync()
        {
            var messages = await Hub.GetMessages();

            CurrentTeam = Hub.CurrentTeam;
            CurrentChannel = Hub.CurrentChannel;

            UpdateView(messages);
        }

        private async void EditTeamDetails()
        {
            if (Hub.CurrentUser == null)
            {
                return;
            }

            // Opens the dialog box for the input
            var dialog = new ChangeTeamDialog()
            {
                TeamName = CurrentTeam.TeamName,
                TeamDescription = CurrentTeam.Description
            };

            // Create team on confirm
            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await Hub.UpdateTeam(dialog.TeamName, dialog.TeamDescription);
            }
        }

        #region Helper

        /// <summary>
        /// Updates the view with the given messages
        /// </summary>
        /// <param name="messages">Messages from the hub</param>
        private void UpdateView(IEnumerable<MessageViewModel> messages)
        {
            Messages.Clear();

            if (messages == null)
            {
                return;
            }

            foreach (var message in messages)
            {
                var myReaction = GetMyReaction(message);

                if (myReaction != ReactionType.None)
                {
                    message.HasReacted = true;
                    message.MyReaction = myReaction;
                }

                Messages.Add(message);
            }
        }

        private ReactionType GetMyReaction(MessageViewModel message)
        {
            if (message.Reactions.Any(r => r.UserId == Hub.CurrentUser.Id))
            {
                var reaction = (ReactionType)message
                    .Reactions
                    .Where(r => r.UserId == Hub.CurrentUser.Id)
                    .Select(r => Enum.Parse(typeof(ReactionType), r.Symbol))
                    .FirstOrDefault();

                return reaction;
            }

            return ReactionType.None;
        }

        #endregion

        #region Events

        /// <summary>
        /// Fires on MessageReceived of ChatHubService
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="message">Received Message object</param>
        private void OnMessageReceived(object sender, MessageViewModel vm)
        {
            if (CurrentChannel.ChannelId == vm.ChannelId)
            {
                Messages.Add(vm);
            }
        }

        /// <summary>
        /// Fires on MessageUpdated of ChatHubService
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="message">Received Message object</param>
        private async void OnMessagesUpdated(object sender, EventArgs e)
        {
            var messages = await Hub.GetMessages();

            UpdateView(messages);
        }

        /// <summary>
        /// Fires on TeamSwitched of ChatHubService
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="messages">List of message of the current team</param>
        private void OnTeamSwitched(object sender, IEnumerable<MessageViewModel> messages)
        {
            CurrentTeam = Hub.CurrentTeam;
            CurrentChannel = Hub.CurrentChannel;

            UpdateView(messages);
        }

        #endregion
    }
}
