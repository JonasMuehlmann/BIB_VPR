using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Messenger.Services.Providers
{
    public class EventProvider
    {
        private SignalRService SignalRService => Singleton<SignalRService>.Instance;

        private ToastNotificationsService Toast => Singleton<ToastNotificationsService>.Instance;

        /// <summary>
        /// Events with the payload of a list of objects
        /// Fires on 'loaded' events including:
        /// • Loaded from DB (initialize)
        /// • Loaded from cache (switch channel/chats, etc.)
        /// </summary>
        #region Loaded Events

        public event EventHandler<BroadcastArgs> TeamsLoaded;

        public event EventHandler<BroadcastArgs> ChatsLoaded;

        public event EventHandler<BroadcastArgs> MessagesLoaded;

        public event EventHandler<BroadcastArgs> NotificationsLoaded;

        #endregion

        /// <summary>
        /// Events with the payload of a single object,
        /// mostly fired by Signal-R to trigger data update
        /// </summary>
        #region Updated Events

        public event EventHandler<BroadcastArgs> TeamUpdated;

        public event EventHandler<BroadcastArgs> ChannelUpdated;

        public event EventHandler<BroadcastArgs> PrivateChatUpdated;

        public event EventHandler<BroadcastArgs> MessageUpdated;

        public event EventHandler<BroadcastArgs> UserUpdated;

        public event EventHandler<BroadcastArgs> NotificationReceived;

        #endregion

        #region State Events

        public event EventHandler<BroadcastArgs> MessagesSwitched;

        #endregion

        public EventProvider()
        {
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            SignalRService.ReceiveMessage += OnReceiveMessage;
            SignalRService.ReceiveInvitation += OnReceiveInvitation;
            SignalRService.ReceiveNotification += OnReceiveNotification;
            SignalRService.MessageUpdated += OnMessageUpdated;
            SignalRService.MessageDeleted += OnMessageDeleted;
            SignalRService.MessageReactionsUpdated += OnMessageReactionsUpdated;
            SignalRService.TeamCreated += OnTeamCreated;
            SignalRService.TeamUpdated += OnTeamUpdated;
            SignalRService.TeamDeleted += OnTeamDeleted;
            SignalRService.TeamRoleUpdated += OnTeamRoleUpdated;
            SignalRService.TeamRoleDeleted += OnTeamRoleDeleted;
            SignalRService.ChannelCreated += OnChannelCreated;
            SignalRService.ChannelUpdated += OnChannelUpdated;
            SignalRService.ChannelDeleted += OnChannelDeleted;
            SignalRService.MemberAdded += OnMemberAdded;
            SignalRService.MemberUpdated += OnMemberUpdated;
            SignalRService.MemberRemoved += OnMemberRemoved;
            SignalRService.UserUpdated += OnUserUpdated;
            SignalRService.ReceiveInvitation += OnReceiveInvitation;
        }

        /// <summary>
        /// Called to raise events from outside of the provider
        /// </summary>
        /// <param name="target">Target event to raise</param>
        /// <param name="reason">Reason of the event raise</param>
        /// <param name="parameter">Parameter to raise the event with</param>
        public void Broadcast(BroadcastOptions target, BroadcastReasons reason = BroadcastReasons.Loaded, object parameter = null)
        {
            /** ARGUMENT TO BE PASSED ON **/
            BroadcastArgs args = new BroadcastArgs()
            {
                Reason = reason,
                Payload = parameter
            };

            /** STATIC PAYLOADS FOR LOADED EVENTS **/
            switch (target)
            {
                case BroadcastOptions.MessagesSwitched:
                    if (CacheQuery.TryGetMessages(App.StateProvider.SelectedChannel.ChannelId, out ObservableCollection<MessageViewModel> messages))
                    {
                        args.Payload = messages;
                    }
                    break;
                case BroadcastOptions.TeamsLoaded:
                    args.Payload = CacheQuery.GetMyTeams();
                    break;
                case BroadcastOptions.ChatsLoaded:
                    args.Payload = CacheQuery.GetMyChats();
                    break;
                case BroadcastOptions.NotificationsLoaded:
                    args.Payload = CacheQuery.GetNotifications();
                    break;
                default:
                    break;
            }

            /** EVENT TRIGGERS **/
            switch (target)
            {
                /* LOADED EVENTS */
                case BroadcastOptions.MessagesSwitched:
                    MessagesLoaded?.Invoke(this, args);
                    break;
                case BroadcastOptions.TeamsLoaded:
                    TeamsLoaded?.Invoke(this, args);
                    break;
                case BroadcastOptions.ChatsLoaded:
                    ChatsLoaded?.Invoke(this, args);
                    break;
                case BroadcastOptions.NotificationsLoaded:
                    NotificationsLoaded?.Invoke(this, args);
                    break;
                /* UPDATED EVENTS */
                case BroadcastOptions.TeamUpdated:
                    if (!(parameter is TeamViewModel))
                        break;
                    TeamUpdated?.Invoke(this, args);
                    break;
                case BroadcastOptions.ChannelUpdated:
                    if (!(parameter is ChannelViewModel))
                        break;
                    ChannelUpdated?.Invoke(this, args);
                    break;
                case BroadcastOptions.ChatUpdated:
                    if (!(parameter is PrivateChatViewModel))
                        break;
                    PrivateChatUpdated?.Invoke(this, args);
                    break;
                case BroadcastOptions.MessageUpdated:
                    if (!(parameter is MessageViewModel))
                        break;
                    MessageUpdated?.Invoke(this, args);
                    break;
                case BroadcastOptions.UserUpdated:
                    if (!(parameter is User))
                        break;
                    MessageUpdated?.Invoke(this, args);
                    break;
                case BroadcastOptions.NotificationReceived:
                    if (!(parameter is NotificationViewModel))
                        break;
                    NotificationReceived?.Invoke(this, args);
                    break;
                default:
                    break;
            }
        }

        #region Notification

        private void OnReceiveNotification(object sender, SignalREventArgs<Notification> e)
        {
            bool isValid = e.Value != null;

            if (!isValid)
            {
                return;
            }

            Notification data = e.Value;
            NotificationViewModel viewModel = new NotificationViewModel(data);

            Broadcast(
                BroadcastOptions.NotificationReceived,
                BroadcastReasons.Created,
                viewModel);
        }

        #endregion

        #region Message

        private async void OnReceiveMessage(object sender, SignalREventArgs<Message> args)
        {
            bool isValid = args.Value != null
                && !string.IsNullOrEmpty(args.Value.SenderId);

            /** EXIT IF MESSAGE IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Message message = args.Value;
            
            /** ADD TO CACHE **/
            MessageViewModel viewModel = await CacheQuery.AddOrUpdate<MessageViewModel>(message);

            /** SEND TOAST IF CHANNEL CURRENTLY NOT SELECTED **/
            if (App.StateProvider.SelectedChannel.ChannelId != message.RecipientId)
            {
                Toast.ShowMessageReceived(App.StateProvider.SelectedTeam, viewModel);
            }

            /** TRIGGER MESSAGE UPDATED (CREATED) **/
            Broadcast(
                BroadcastOptions.MessageUpdated,
                BroadcastReasons.Created,
                viewModel);
        }

        private async void OnMessageUpdated(object sender, SignalREventArgs<Message> args)
        {
            bool isValid = args.Value != null;

            /** EXIT IF MESSAGE IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Message message = args.Value;

            /** UPDATE IN CACHE **/
            MessageViewModel viewModel = await CacheQuery.AddOrUpdate<MessageViewModel>(message);

            /** TRIGGER MESSAGE UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.MessageUpdated,
                BroadcastReasons.Updated,
                viewModel);
        }

        private void OnMessageDeleted(object sender, SignalREventArgs<Message> args)
        {
            bool isValid = args.Value != null;

            /** EXIT IF MESSAGE IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Message message = args.Value;

            /** REMOVE FROM CACHE **/
            MessageViewModel viewModel = CacheQuery.Remove<MessageViewModel>(message);

            /** TRIGGER MESSAGE UPDATED (DELETED) **/
            Broadcast(
                BroadcastOptions.MessageUpdated,
                BroadcastReasons.Deleted,
                viewModel);
        }

        private async void OnMessageReactionsUpdated(object sender, SignalREventArgs<Message> args)
        {
            bool isValid = args.Value != null;

            if (!isValid)
            {
                return;
            }

            Message message = args.Value;

            /** RELOAD FROM DB **/
            MessageViewModel viewModel = await CacheQuery.AddOrUpdate<MessageViewModel>(message);

            /** TRIGGER MESSAGE UPDATED (DELETED) **/
            Broadcast(
                BroadcastOptions.MessageUpdated,
                BroadcastReasons.Updated,
                viewModel);
        }

        #endregion

        #region Team

        private async void OnReceiveInvitation(object sender, SignalREventArgs<Team> e)
        {
            bool isValid = e.Value != null;

            if (!isValid)
            {
                return;
            }

            Team team = e.Value;

            if (string.IsNullOrEmpty(team.Name))
            {
                PrivateChatViewModel chatViewModel = await CacheQuery.AddOrUpdate<PrivateChatViewModel>(team);

                Toast.ShowInvitationReceived(chatViewModel);

                Broadcast(
                    BroadcastOptions.ChatUpdated,
                    BroadcastReasons.Updated,
                    chatViewModel);
            }
            else
            {
                TeamViewModel teamViewModel = await CacheQuery.AddOrUpdate<TeamViewModel>(team);

                Toast.ShowInvitationReceived(teamViewModel);

                Broadcast(
                    BroadcastOptions.TeamUpdated,
                    BroadcastReasons.Updated,
                    teamViewModel);
            }
        }

        private async void OnTeamCreated(object sender, SignalREventArgs<Team> e)
        {
            bool isValid = e.Value != null;

            if (!isValid) return;

            Team team = e.Value;

            if (string.IsNullOrEmpty(team.Name))
            {
                PrivateChatViewModel chatViewModel = await CacheQuery.AddOrUpdate<PrivateChatViewModel>(team);

                Broadcast(
                    BroadcastOptions.ChatUpdated,
                    BroadcastReasons.Created,
                    chatViewModel);
            }
            else
            {
                TeamViewModel teamViewModel = await CacheQuery.AddOrUpdate<TeamViewModel>(team);

                Broadcast(
                    BroadcastOptions.TeamUpdated,
                    BroadcastReasons.Created,
                    teamViewModel);
            }
        }

        private async void OnTeamUpdated(object sender, SignalREventArgs<Team> e)
        {
            bool isValid = e.Value != null;

            if (!isValid)
            {
                return;
            }

            Team team = e.Value;
            bool isPrivateChat = string.IsNullOrEmpty(team.Name);

            if (isPrivateChat)
            {
                /** ADD PRIVATE CHAT TO CACHE **/
                PrivateChatViewModel chatViewModel = await CacheQuery.AddOrUpdate<PrivateChatViewModel>(team);

                UpdateSelectedTeam(chatViewModel);

                Broadcast(
                    BroadcastOptions.ChatUpdated,
                    BroadcastReasons.Updated,
                    chatViewModel);
            }
            else
            {
                /** ADD TEAM TO CACHE **/
                TeamViewModel teamViewModel = await CacheQuery.AddOrUpdate<TeamViewModel>(team);

                UpdateSelectedTeam(teamViewModel);

                Broadcast(
                    BroadcastOptions.TeamUpdated,
                    BroadcastReasons.Updated,
                    teamViewModel);
            }

        }

        private void OnTeamDeleted(object sender, SignalREventArgs<Team> e)
        {
            bool isValid = e.Value != null;

            if (!isValid)
            {
                return;
            }

            Team team = e.Value;

            if (string.IsNullOrEmpty(team.Name))
            {
                PrivateChatViewModel chatViewModel = CacheQuery.Remove<PrivateChatViewModel>(team);

                Broadcast(
                    BroadcastOptions.ChatUpdated,
                    BroadcastReasons.Deleted,
                    chatViewModel);
            }
            else
            {
                TeamViewModel teamViewModel = CacheQuery.Remove<TeamViewModel>(team);

                Broadcast(
                    BroadcastOptions.TeamUpdated,
                    BroadcastReasons.Deleted,
                    teamViewModel);
            }
        }

        #endregion

        #region TeamRole

        private async void OnTeamRoleUpdated(object sender, SignalREventArgs<TeamRole> e)
        {
            bool isValid = e.Value != null;

            if (!isValid) return;

            TeamRole teamRole = e.Value;

            TeamRoleViewModel teamRoleViewModel = await CacheQuery.AddOrUpdate<TeamRoleViewModel>(teamRole);

            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>(teamRoleViewModel.TeamId);

            UpdateSelectedTeam(teamViewModel);

            Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                teamViewModel);
        }

        private void OnTeamRoleDeleted(object sender, SignalREventArgs<TeamRole> e)
        {
            bool isValid = e.Value != null;

            if (!isValid) return;

            TeamRole teamRole = e.Value;

            TeamRoleViewModel teamRoleViewModel = CacheQuery.Remove<TeamRoleViewModel>(teamRole.Id);

            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>(teamRoleViewModel.TeamId);

            UpdateSelectedTeam(teamViewModel);

            Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                teamViewModel);
        }

        #endregion

        #region Channel

        private async void OnChannelCreated(object sender, SignalREventArgs<Channel> e)
        {
            bool isValid = e.Value != null;

            /** EXIT IF TEAM IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Channel channel = e.Value;

            /** ADD TO CACHE **/
            ChannelViewModel viewModel = await CacheQuery.AddOrUpdate<ChannelViewModel>(channel);

            /** TRIGGER CHANNEL UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.ChannelUpdated,
                BroadcastReasons.Created,
                viewModel);
        }

        private async void OnChannelUpdated(object sender, SignalREventArgs<Channel> e)
        {
            bool isValid = e.Value != null;

            /** EXIT IF TEAM IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Channel channel = e.Value;

            /** ADD OR UPDATE TO CACHE **/
            ChannelViewModel viewModel = await CacheQuery.AddOrUpdate<ChannelViewModel>(channel);

            /** UPDATE STATE IF SELECTED **/
            if (App.StateProvider.SelectedChannel.ChannelId == viewModel.ChannelId)
            {
                App.StateProvider.SelectedChannel = viewModel;
            }

            /** TRIGGER CHANNEL UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.ChannelUpdated,
                BroadcastReasons.Updated,
                viewModel);
        }

        private void OnChannelDeleted(object sender, SignalREventArgs<Channel> e)
        {
            bool isValid = e.Value != null;

            /** EXIT IF TEAM IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Channel channel = e.Value;

            /** REMOVE FROM CACHE **/
            ChannelViewModel viewModel = CacheQuery.Remove<ChannelViewModel>(channel);

            /** TRIGGER CHANNEL UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.ChannelUpdated,
                BroadcastReasons.Deleted,
                viewModel);
        }

        #endregion

        #region Member

        private async void OnMemberAdded(object sender, SignalREventArgs<User, Team> e)
        {
            bool isValid = e.FirstValue != null
                && e.SecondValue != null;

            /** EXIT IF INVITATION IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            User user = e.FirstValue;
            Team team = e.SecondValue;

            MemberViewModel viewModel = await CacheQuery.AddOrUpdate<MemberViewModel>((uint)team.Id, user);
            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>((uint)team.Id);

            UpdateSelectedTeam(teamViewModel);

            /** TRIGGER TEAM UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                teamViewModel);
        }

        private async void OnMemberUpdated(object sender, SignalREventArgs<User, Team> e)
        {
            bool isValid = e.FirstValue != null
                && e.SecondValue != null;

            /** EXIT IF INVITATION IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            User user = e.FirstValue;
            Team team = e.SecondValue;

            MemberViewModel viewModel = await CacheQuery.AddOrUpdate<MemberViewModel>(team.Id, user);
            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>((uint)team.Id);

            UpdateSelectedTeam(teamViewModel);

            /** TRIGGER TEAM UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                teamViewModel);
        }

        private void OnMemberRemoved(object sender, SignalREventArgs<User, Team> e)
        {
            bool isValid = e.FirstValue != null
                && e.SecondValue != null;

            /** EXIT IF INVITATION IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            User user = e.FirstValue;
            Team team = e.SecondValue;

            MemberViewModel viewModel = CacheQuery.Remove<MemberViewModel>((uint)team.Id, user.Id);
            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>((uint)team.Id);

            UpdateSelectedTeam(teamViewModel);

            /** TRIGGER TEAM UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                teamViewModel);
        }

        #endregion

        #region User Data

        private void OnUserUpdated(object sender, SignalREventArgs<User> e)
        {
            bool isValid = e.Value != null;

            if (!isValid) return;

            User user = e.Value;

            Broadcast(
                BroadcastOptions.UserUpdated,
                BroadcastReasons.Updated,
                user);
        }

        #endregion

        #region Helpers

        private void UpdateSelectedTeam(TeamViewModel updatedViewModel)
        {
            TeamViewModel currentTeam = App.StateProvider.SelectedTeam;

            if (currentTeam != null && currentTeam.Id == updatedViewModel.Id)
            {
                ChannelViewModel currentChannel = App.StateProvider.SelectedChannel;

                App.StateProvider.SelectedTeam = updatedViewModel;
                App.StateProvider.SelectedChannel = updatedViewModel.Channels.SingleOrDefault(c => c.ChannelId == currentChannel.ChannelId);
            }
        }

        #endregion
    }
}
