using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Messenger.Commands.PrivateChat;
using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ChatNavViewModel : Observable
    {
        #region Private

        private ObservableCollection<PrivateChatViewModel> _chats;
        private bool _isBusy;
        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

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

        public ICommand SwitchChatCommand => new RelayCommand<WinUI.TreeViewItemInvokedEventArgs>(SwitchChat);

        public ICommand StartChatCommand => new StartChatCommand(this, ChatHubService);

        #endregion

        public ChatNavViewModel()
        {
            IsBusy = true;

            Chats = new ObservableCollection<PrivateChatViewModel>();
            ChatHubService.ChatsUpdated += OnChatsUpdated;
            ChatHubService.MessageReceived += OnMessageReceived;
            Initialize();
        }

        private async void Initialize()
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
                    foreach (PrivateChatViewModel vm in await ChatHubService.GetMyChats())
                    {
                        Chats.Add(vm);
                    }
                    IsBusy = false;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Command on chat item click and invokes ChatHubService to load messages of the selected chat
        /// </summary>
        /// <param name="args">Event argument from the event, contains the data of the invoked item</param>
        private async void SwitchChat(WinUI.TreeViewItemInvokedEventArgs args)
        {
            PrivateChatViewModel chat = args.InvokedItem as PrivateChatViewModel;

            // Invokes TeamSwitched event
            await ChatHubService.SwitchChat((uint)chat.Id);

            NavigationService.Open<ChatPage>();
        }

        /// <summary>
        /// Fires on TeamsUpdated in ChatHubService and refreshes the view
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="teams">Enumerable of teams</param>
        private void OnChatsUpdated(object sender, IEnumerable<PrivateChatViewModel> chats)
        {
            if (chats != null)
            {
                Chats.Clear();
                foreach (PrivateChatViewModel viewModel in chats)
                {
                    Chats.Add(viewModel);
                }
            }

            IsBusy = false;
        }

        /// <summary>
        /// Fires on MessageReceived in ChatHubService and refreshes the view
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="message">MessageViewModel received</param>
        private void OnMessageReceived(object sender, MessageViewModel message)
        {
            foreach (PrivateChatViewModel chat in _chats)
            {
                if (chat.MainChannel.ChannelId == message.ChannelId)
                {
                    chat.LastMessage = message;
                }
            }
        }
    }
}
