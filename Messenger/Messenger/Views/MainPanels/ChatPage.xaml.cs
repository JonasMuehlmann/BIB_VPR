using Messenger.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
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
            base.OnNavigatedTo(e);

            App.EventProvider.MessagesSwitched += ViewModel.OnMessagesSwitched;
            App.EventProvider.MessageUpdated += ViewModel.OnMessageUpdated;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            App.EventProvider.MessagesSwitched -= ViewModel.OnMessagesSwitched;
            App.EventProvider.MessageUpdated -= ViewModel.OnMessageUpdated;
        }
    }
}
