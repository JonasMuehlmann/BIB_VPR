using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Pages
{
    public sealed partial class NotificationNavPage : Page
    {
        public NotificationNavViewModel ViewModel { get; } = new NotificationNavViewModel();

        public NotificationNavPage()
        {
            InitializeComponent();
        }
    }
}
