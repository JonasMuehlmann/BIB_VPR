using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Pages
{
    public sealed partial class ChatPage : Page
    {
        public ChatViewModel ViewModel { get; set; }

        public ChatPage()
        {
            ViewModel = new ChatViewModel();

            InitializeComponent();
        }
    }
}
