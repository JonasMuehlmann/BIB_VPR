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
    }
}
