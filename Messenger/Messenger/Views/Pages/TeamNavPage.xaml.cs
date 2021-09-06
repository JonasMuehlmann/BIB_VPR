using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views.Pages
{
    public sealed partial class TeamNavPage : Page
    {
        public TeamNavViewModel ViewModel { get; } = new TeamNavViewModel();

        public TeamNavPage()
        {
            InitializeComponent();
        }

        private void treeView_ItemInvoked(Microsoft.UI.Xaml.Controls.TreeView sender, Microsoft.UI.Xaml.Controls.TreeViewItemInvokedEventArgs args)
        {
            switch (args.InvokedItem)
            {
                case ChannelViewModel channel:
                    ViewModel.TeamChannelSwitchCommand.Execute(channel);
                    break;
                default:
                    break;
            }
        }
    }
}
