using Messenger.ViewModels;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views
{
    public sealed partial class ChatPage : Page
    {
        public ChatViewModel ViewModel { get; } = new ChatViewModel();

        public ChatPage()
        {
            InitializeComponent();
        }
    }
}
