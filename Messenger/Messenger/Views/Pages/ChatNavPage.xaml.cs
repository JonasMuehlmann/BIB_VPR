using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views.Pages
{
    public sealed partial class ChatNavPage : Page
    {
        public ChatNavViewModel ViewModel { get; } = new ChatNavViewModel();

        public ChatNavPage()
        {
            InitializeComponent();
        }

        private void treeView_ItemInvoked(Microsoft.UI.Xaml.Controls.TreeView sender, Microsoft.UI.Xaml.Controls.TreeViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case PrivateChatViewModel chat:
                    ViewModel.SwitchChatCommand.Execute(chat.MainChannel);
                    break;
                default:
                    break;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.EventProvider.ChatsLoaded += ViewModel.OnChatsLoaded;
            App.EventProvider.PrivateChatUpdated += ViewModel.OnChatUpdated;
            App.EventProvider.MessageUpdated += ViewModel.OnMessageUpdated;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            App.EventProvider.ChatsLoaded -= ViewModel.OnChatsLoaded;
            App.EventProvider.PrivateChatUpdated -= ViewModel.OnChatUpdated;
            App.EventProvider.MessageUpdated -= ViewModel.OnMessageUpdated;
        }
    }
}
