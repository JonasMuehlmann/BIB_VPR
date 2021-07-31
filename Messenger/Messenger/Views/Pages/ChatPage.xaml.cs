using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views.Pages
{
    public sealed partial class ChatPage : Page
    {
        public ChatViewModel ViewModel { get; } = new ChatViewModel();

        public ChatPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            App.EventProvider.MessagesSwitched += ViewModel.MessagesListViewModel.OnMessagesSwitched;
            App.EventProvider.MessageUpdated += ViewModel.MessagesListViewModel.OnMessageUpdated;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            App.EventProvider.MessagesSwitched -= ViewModel.MessagesListViewModel.OnMessagesSwitched;
            App.EventProvider.MessageUpdated -= ViewModel.MessagesListViewModel.OnMessageUpdated;

            base.OnNavigatedFrom(e);
        }
    }
}
