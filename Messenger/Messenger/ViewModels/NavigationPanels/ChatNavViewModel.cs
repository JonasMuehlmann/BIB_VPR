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
using WinUI = Microsoft.UI.Xaml.Controls;

namespace Messenger.ViewModels
{
    public class ChatNavViewModel : Observable
    {
        #region Private

        private ObservableCollection<PrivateChat> _chats;
        private bool _isBusy;
        private ChatHubService ChatHubService => Singleton<ChatHubService>.Instance;

        #endregion

        #region Properties

        public ObservableCollection<PrivateChat> Chats
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

            Chats = new ObservableCollection<PrivateChat>();
            ChatHubService.TeamsUpdated += OnTeamsUpdated;
            LoadAsync();
        }

        private void LoadAsync()
        {
            if (ChatHubService.CurrentUser?.Teams != null
                && ChatHubService.CurrentUser?.Teams.Count > 0)
            {
                FilterAndUpdateChats(ChatHubService.CurrentUser.Teams);
                IsBusy = false;
            }
        }

        /// <summary>
        /// Command on chat item click and invokes ChatHubService to load messages of the selected chat
        /// </summary>
        /// <param name="args">Event argument from the event, contains the data of the invoked item</param>
        private async void SwitchChat(WinUI.TreeViewItemInvokedEventArgs args)
        {
            uint teamId = (args.InvokedItem as Team).Id;

            // Invokes TeamSwitched event
            await ChatHubService.SwitchTeam(teamId);
        }

        /// <summary>
        /// Fires on TeamsUpdated in ChatHubService and refreshes the view
        /// </summary>
        /// <param name="sender">Service that invoked the event</param>
        /// <param name="teams">Enumerable of teams</param>
        private void OnTeamsUpdated(object sender, IEnumerable<Team> teams)
        {
            if (teams != null)
            {
                FilterAndUpdateChats(teams);
            }

            IsBusy = false;
        }

        /// <summary>
        /// Filters out private chats from the given teams list and updates the view
        /// </summary>
        /// <param name="teams">List of teams from the ChatHubService</param>
        private void FilterAndUpdateChats(IEnumerable<Team> teams)
        {
            if (teams != null)
            {
                var chatsList = teams
                    .Where(team => string.IsNullOrEmpty(team.Name))
                    .Select(data => PrivateChat.CreatePrivateChatFromTeamData(data));

                Chats.Clear();
                foreach (var chat in chatsList)
                {
                    Chats.Add(chat);
                }
            }
        }
    }
}
