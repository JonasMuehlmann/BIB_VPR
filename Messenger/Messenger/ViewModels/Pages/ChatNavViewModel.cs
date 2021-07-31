using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
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

        public UserViewModel CurrentUser { get; set; }

        public ICommand SwitchChatCommand => new ChannelSwitchCommand();

        public ICommand StartChatCommand => new StartChatCommand(this);

        #endregion

        public ChatNavViewModel()
        {
            Initialize();
        }

        private async void Initialize()
        {
            IsBusy = true;
            Chats = new ObservableCollection<PrivateChatViewModel>();

            /** GET DATA FROM CACHE IF ALREADY INITIALIZED **/
            if (App.StateProvider != null)
            {
                Chats.Clear();

                foreach (PrivateChatViewModel chat in CacheQuery.GetMyChats())
                {
                    Chats.Add(chat);
                }

                IsBusy = false;
            }

            CurrentUser = await UserDataService.GetUserAsync();
        }

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

                IsBusy = false;
            }
        }

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
    }
}
