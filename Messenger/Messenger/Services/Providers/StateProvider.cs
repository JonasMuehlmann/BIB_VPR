using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Helpers.MessageHelpers;
using Messenger.Helpers.TeamHelpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using Microsoft.Toolkit.Uwp.Notifications;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Messenger.Services.Providers
{
    public class StateProvider : Observable
    {
        #region Private

        private TeamViewModel _selectedTeam;

        private ChannelViewModel _selectedChannel;

        private UserViewModel _currentUser;

        #endregion

        #region Properties

        /// <summary>
        /// Instance to manage teams and private chats
        /// </summary>
        public readonly TeamManager TeamManager;

        /// <summary>
        /// Instance to manage messages by channel id
        /// </summary>
        public readonly MessageManager MessageManager;

        /// <summary>
        /// Currently selected team view model
        /// </summary>
        public TeamViewModel SelectedTeam
        {
            get { return _selectedTeam; }
            set { Set(ref _selectedTeam, value); }
        }

        /// <summary>
        /// Currently selected channel view model
        /// </summary>
        public ChannelViewModel SelectedChannel
        {
            get { return _selectedChannel; }
            set { Set(ref _selectedChannel, value); }
        }

        /// <summary>
        /// Currently logged-in user
        /// </summary>
        public UserViewModel CurrentUser
        {
            get { return _currentUser; }
            set { Set(ref _currentUser, value); }
        }

        #endregion

        public StateProvider()
        {
            TeamManager = new TeamManager(this);
            MessageManager = new MessageManager();
        }
    }

    /// <summary>
    /// Extension methods for StateProvider
    /// </summary>
    public static class StateProviderExtension
    {
        /// <summary>
        /// Initializes the provider with teams/chats and messages, loaded from database
        /// </summary>
        /// <param name="provider">Provider instance(Extension)</param>
        /// <param name="user">Currently logged-in user</param>
        /// <returns>Initialized provider instance</returns>
        public static async Task<StateProvider> Initialize(this StateProvider provider, UserViewModel user)
        {
            provider.CurrentUser = user;

            SendProgressToast();

            /** LOAD TEAMS/PRIVATE CHATS FROM DATABASE **/
            await provider.TeamManager.Initialize(user);

            UpdateProgress(2);

            await provider.LoadAllMessages();

            UpdateProgress(3);

            /* BROADCAST MY TEAMS */
            App.EventProvider.Broadcast(
                BroadcastOptions.TeamsLoaded,
                BroadcastReasons.Loaded);

            /* BROADCAST MY CHATS */
            App.EventProvider.Broadcast(
                BroadcastOptions.ChatsLoaded,
                BroadcastReasons.Loaded);

            return provider;
        }

        /// <summary>
        /// Loads all messages from the database
        /// </summary>
        /// <param name="provider">Provider instance(Extension)</param>
        /// <returns>Task to be awaited</returns>
        public static async Task LoadAllMessages(this StateProvider provider)
        {
            /** LOAD MESSAGES FOR TEAMS **/
            if (provider.TeamManager.MyTeams.Count() > 0)
            {
                await provider.MessageManager.CreateEntry(provider.TeamManager.MyTeams);
            }

            /** LOAD MESSAGES FOR PRIVATE CHATS **/
            if (provider.TeamManager.MyChats.Count() > 0)
            {
                await provider.MessageManager.CreateEntry(provider.TeamManager.MyChats);
            }
        }

        public static void SendProgressToast()
        {
            string tag = "initialization";

            // Construct the toast content with data bound fields
            var content = new ToastContentBuilder()
                .AddText("Initializing the application...")
                .AddVisualChild(new AdaptiveProgressBar()
                {
                    Title = "Progress",
                    Value = new BindableProgressBarValue("progressValue"),
                    ValueStringOverride = new BindableString("progressValueString"),
                    Status = new BindableString("progressStatus")
                })
                .GetToastContent();

            // Generate the toast notification
            var toast = new ToastNotification(content.GetXml());

            toast.Tag = tag;

            toast.Data = new NotificationData();
            toast.Data.Values["progressValue"] = "0";
            toast.Data.Values["progressValueString"] = $"0/2 Loaded";
            toast.Data.Values["progressStatus"] = "Loading Teams and Chats...";

            toast.Data.SequenceNumber = 1;

            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public static void UpdateProgress(uint sequence)
        {
            string tag = "initialization";
            
            var data = new NotificationData
            {
                SequenceNumber = sequence
            };

            data.Values["progressValue"] = $"{sequence - 1}";
            data.Values["progressValueString"] = $"{sequence - 1}/2 Loaded";

            if (sequence == 2)
            {
                data.Values["progressStatus"] = "Loading Messages...";
            }
            else if (sequence == 3)
            {
                data.Values["progressStatus"] = "Complete!";
            }

            ToastNotificationManager.CreateToastNotifier().Update(data, tag);
        }
    }
}
