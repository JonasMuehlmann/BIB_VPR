using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Messenger.Commands;
using Messenger.Commands.PrivateChat;
using Messenger.Commands.TeamManage;
using Messenger.Core.Helpers;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;

namespace Messenger.ViewModels.Pages
{
    public class ChatNavViewModel : Observable
    {
        #region Private

        private UserDataService UserDataService => Singleton<UserDataService>.Instance;

        private ObservableCollection<PrivateChatViewModel> _chats;

        private bool _isBusy;

        #endregion

        #region Properties

        /// <summary>
        /// Loaded chats list of the current user
        /// </summary>
        public ObservableCollection<PrivateChatViewModel> Chats
        {
            get
            {
                return _chats;
            }
            set
            {
                Set(ref _chats, value);
            }
        }

        /// <summary>
        /// Shows loading spinner on true
        /// </summary>
        public bool IsBusy
        {
            get
            {
                return _isBusy;
            }
            set
            {
                Set(ref _isBusy, value);
            }
        }

        /// <summary>
        /// Currently logged-in user
        /// </summary>
        public UserViewModel CurrentUser { get; set; }

        #endregion

        #region Commands

        /// <summary>
        /// Switches to another private chat
        /// </summary>
        public ICommand SwitchChatCommand => new ChannelSwitchCommand();

        /// <summary>
        /// Opens a start chat dialog to create a new private chat
        /// </summary>
        public ICommand StartChatCommand => new StartChatCommand();

        /// <summary>
        /// Manually reloads all chats and messages
        /// </summary>
        public ICommand ReloadCommand => new RelayCommand(Reload);

        #endregion

        public ChatNavViewModel()
        {
            Initialize();

            App.EventProvider.ChatsLoaded += OnChatsLoaded;
            App.EventProvider.PrivateChatUpdated += OnChatUpdated;
            App.EventProvider.MessageUpdated += OnMessageUpdated;
        }

        /// <summary>
        /// Initializes view model with currently logged-in user and chats list, if already loaded in cache
        /// </summary>
        private async void Initialize()
        {
            if (Chats != null) return;

            IsBusy = true;
            Chats = new ObservableCollection<PrivateChatViewModel>();

            /** GET DATA FROM CACHE IF ALREADY INITIALIZED **/
            if (App.StateProvider != null)
            {
                LoadFromCache();
            }

            CurrentUser = await UserDataService.GetUserAsync();

            IsBusy = false;
        }

        /// <summary>
        /// Loads the chats list from the cache
        /// </summary>
        private void LoadFromCache()
        {
            Chats.Clear();

            foreach (PrivateChatViewModel chat in CacheQuery.GetMyChats())
            {
                Chats.Add(chat);
            }
        }

        /// <summary>
        /// Manually reloads all chats list and messages
        /// </summary>
        private async void Reload()
        {
            IsBusy = true;

            Chats.Clear();

            await Task.Delay(TimeSpan.FromMilliseconds(500));
            await CacheQuery.Reload();
        }

        #region Events

        /// <summary>
        /// Fired by EventProvider on ChatsLoaded
        /// </summary>
        public void OnChatsLoaded(object sender, BroadcastArgs e)
        {
            IEnumerable<PrivateChatViewModel> chats = e.Payload as IEnumerable<PrivateChatViewModel>;

            if (chats != null && chats.Count() > 0)
            {
                Chats.Clear();

                foreach (PrivateChatViewModel chat in chats)
                {
                    Chats.Add(chat);
                }

            }

            IsBusy = false;
        }

        /// <summary>
        /// Fired by EventProvider on PrivateChatUpdated
        /// </summary>
        public void OnChatUpdated(object sender, BroadcastArgs e)
        {
            PrivateChatViewModel privateChat = e.Payload as PrivateChatViewModel;

            if (privateChat == null)
            {
                return;
            }

            if (e.Reason == BroadcastReasons.Created)
            {
                _chats.Add(privateChat);
            }
            else if (e.Reason == BroadcastReasons.Updated)
            {
                PrivateChatViewModel target = _chats.Single(t => t.Id == privateChat.Id);
                int index = _chats.IndexOf(target);

                _chats[index] = privateChat;
            }
            else if (e.Reason == BroadcastReasons.Deleted)
            {
                PrivateChatViewModel target = _chats.Single(t => t.Id == privateChat.Id);

                if (target != null)
                {
                    _chats.Remove(target);
                }
            }
        }

        /// <summary>
        /// Fired by EventProvider on MessageUpdated(for LastMessage)
        /// </summary>
        public void OnMessageUpdated(object sender, BroadcastArgs e)
        {
            if (e.Reason == BroadcastReasons.Created)
            {
                MessageViewModel message = e.Payload as MessageViewModel;

                foreach (PrivateChatViewModel privateChat in _chats)
                {
                    if (privateChat.MainChannel.ChannelId == message.ChannelId)
                    {
                        privateChat.LastMessage = message;
                        break;
                    }
                }
            }
        }

        #endregion
    }
}
