using System;

using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
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

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.EventProvider.TeamsLoaded += ViewModel.OnTeamsLoaded;
            App.EventProvider.TeamUpdated += ViewModel.OnTeamUpdated;
            App.EventProvider.ChannelUpdated += ViewModel.OnChannelUpdated;
            App.EventProvider.MessageUpdated += ViewModel.OnMessageUpdated;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            App.EventProvider.TeamsLoaded -= ViewModel.OnTeamsLoaded;
            App.EventProvider.TeamUpdated -= ViewModel.OnTeamUpdated;
            App.EventProvider.ChannelUpdated -= ViewModel.OnChannelUpdated;
            App.EventProvider.MessageUpdated -= ViewModel.OnMessageUpdated;
        }
    }
}
