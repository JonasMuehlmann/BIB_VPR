using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Messenger.Services.Providers
{
    public class EventProvider
    {
        /// <summary>
        /// Payload: loaded enumerable list
        /// </summary>
        #region Loaded Events

        public event EventHandler<BroadcastArgs> TeamsLoaded;

        public event EventHandler<BroadcastArgs> ChatsLoaded;

        public event EventHandler<BroadcastArgs> MessagesLoaded;

        #endregion

        /// <summary>
        /// Payload: updated single object
        /// </summary>
        #region Updated Events

        public event EventHandler<BroadcastArgs> TeamUpdated;

        public event EventHandler<BroadcastArgs> ChannelUpdated;

        public event EventHandler<BroadcastArgs> PrivateChatUpdated;

        public event EventHandler<BroadcastArgs> MessageUpdated;

        public event EventHandler<BroadcastArgs> UserUpdated;

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
            SignalRService.MessageUpdated += OnMessageUpdated;
            SignalRService.MessageDeleted += OnMessageDeleted;
            SignalRService.TeamCreated += OnTeamCreated;
            SignalRService.TeamUpdated += OnTeamUpdated;
            SignalRService.TeamDeleted += OnTeamDeleted;
            SignalRService.ChannelCreated += OnChannelCreated;
            SignalRService.ChannelUpdated += OnChannelUpdated;
            SignalRService.ChannelDeleted += OnChannelDeleted;
            SignalRService.MemberAdded += OnMemberAdded;
            SignalRService.MemberUpdated += OnMemberUpdated;
            SignalRService.MemberRemoved += OnMemberRemoved;
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
                    args.Payload = CacheQuery.GetMessagesByChannelId(App.StateProvider.SelectedChannel.ChannelId);
                    break;
                case BroadcastOptions.TeamsLoaded:
                    args.Payload = CacheQuery.GetMyTeams();
                    break;
                case BroadcastOptions.ChatsLoaded:
                    args.Payload = CacheQuery.GetMyChats();
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
                default:
                    break;
            }
        }

        #region Message

        /// <summary>
        /// Loads the sender information and saves the message to the cache
        /// Fires on "ReceiveMessage"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Received message object</param>
        private async void OnReceiveMessage(object sender, SignalREventArgs<Message> e)
        {
            bool isValid = e.Value != null
                && !string.IsNullOrEmpty(e.Value.SenderId);

            /** EXIT IF MESSAGE IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Message message = e.Value;
            
            /** ADD TO CACHE **/
            MessageViewModel viewModel = await CacheQuery.AddOrUpdate<MessageViewModel>(message);

            /** TRIGGER MESSAGE UPDATED (CREATED) **/
            Broadcast(
                BroadcastOptions.MessageUpdated,
                BroadcastReasons.Created,
                viewModel);
        }

        /// <summary>
        /// Fires on "MessageUpdated"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Id of the team that the user was invited to</param>
        private async void OnMessageUpdated(object sender, SignalREventArgs<Message> e)
        {
            bool isValid = e.Value != null;

            /** EXIT IF MESSAGE IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Message message = e.Value;

            /** UPDATE IN CACHE **/
            MessageViewModel viewModel = await CacheQuery.AddOrUpdate<MessageViewModel>(message);

            /** TRIGGER MESSAGE UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.MessageUpdated,
                BroadcastReasons.Updated,
                viewModel);
        }

        /// <summary>
        /// Fires on "MessageDeleted"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="message">Id of the team that the user was invited to</param>
        private void OnMessageDeleted(object sender, SignalREventArgs<Message> e)
        {
            bool isValid = e.Value != null;

            /** EXIT IF MESSAGE IS NOT VALID **/
            if (!isValid)
            {
                return;
            }

            Message message = e.Value;

            /** REMOVE FROM CACHE **/
            MessageViewModel viewModel = CacheQuery.Remove<MessageViewModel>(message);

            /** TRIGGER MESSAGE UPDATED (DELETED) **/
            Broadcast(
                BroadcastOptions.MessageUpdated,
                BroadcastReasons.Deleted,
                viewModel);
        }

        #endregion

        #region Team

        /// <summary>
        /// Fires on "ReceiveInvitation"
        /// </summary>
        /// <param name="sender">Service that triggered this event</param>
        /// <param name="teamId">Id of the team that the user was invited to</param>
        private async void OnReceiveInvitation(object sender, SignalREventArgs<User, Team> e)
        {
            bool isValid = e.FirstValue != null
                && e.SecondValue != null;

            if (!isValid)
            {
                return;
            }

            User user = e.FirstValue;
            Team team = e.SecondValue;

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

        private async void OnTeamCreated(object sender, SignalREventArgs<Team> e)
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

            if (string.IsNullOrEmpty(team.Name))
            {
                PrivateChatViewModel chatViewModel = await CacheQuery.AddOrUpdate<PrivateChatViewModel>(team);

                Broadcast(
                    BroadcastOptions.ChatUpdated,
                    BroadcastReasons.Updated,
                    chatViewModel);
            }
            else
            {
                TeamViewModel teamViewModel = await CacheQuery.AddOrUpdate<TeamViewModel>(team);

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
                    BroadcastReasons.Created,
                    teamViewModel);
            }
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

            MemberViewModel viewModel = await CacheQuery.AddOrUpdate<MemberViewModel>(user);
            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>((uint)team.Id);

            /** TRIGGER TEAM UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Created,
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

            MemberViewModel viewModel = await CacheQuery.AddOrUpdate<MemberViewModel>(user);
            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>((uint)team.Id);

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

            MemberViewModel viewModel = CacheQuery.Remove<MemberViewModel>(user);
            TeamViewModel teamViewModel = CacheQuery.Get<TeamViewModel>((uint)team.Id);

            /** TRIGGER TEAM UPDATED (UPDATED) **/
            Broadcast(
                BroadcastOptions.TeamUpdated,
                BroadcastReasons.Updated,
                teamViewModel);
        }

        #endregion
    }
}
