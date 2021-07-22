using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.PrivateChat;
using Messenger.Commands.TeamManage;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ChatNavViewModel : Observable
    {
        #region Private

        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

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

        public ICommand SwitchChatCommand => new ChannelSwitchCommand(ChatHubService);

        public ICommand StartChatCommand => new StartChatCommand(this, ChatHubService);

        #endregion

        public ChatNavViewModel()
        {
            IsBusy = true;
            Chats = new ObservableCollection<PrivateChatViewModel>();

            App.EventProvider.ChatsLoaded += OnChatsLoaded;
            App.EventProvider.PrivateChatUpdated += OnPrivateChatUpdated;
            App.EventProvider.MessageUpdated += OnMessageUpdated;

            Initialize();
        }

        private void Initialize()
        {
            switch (ChatHubService.ConnectionState)
            {
                case ChatHubConnectionState.Loading:
                    IsBusy = true;
                    break;
                case ChatHubConnectionState.NoDataFound:
                    IsBusy = false;
                    break;
                case ChatHubConnectionState.LoadedWithData:
                    Chats.Clear();
                    foreach (PrivateChatViewModel vm in CacheQuery.GetMyChats())
                    {
                        Chats.Add(vm);
                    }
                    IsBusy = false;
                    break;
                default:
                    break;
            }
        }

        private void OnMessageUpdated(object sender, BroadcastArgs e)
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

        private void OnPrivateChatUpdated(object sender, BroadcastArgs e)
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

        private void OnChatsLoaded(object sender, BroadcastArgs e)
        {
            IEnumerable<PrivateChatViewModel> chats = e.Payload as IEnumerable<PrivateChatViewModel>;

            if (chats != null && chats.Count() > 0)
            {
                _chats = new ObservableCollection<PrivateChatViewModel>(chats);
            }
        }
    }
}
