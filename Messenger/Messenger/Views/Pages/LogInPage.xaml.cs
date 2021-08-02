using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Pages
{
    public sealed partial class LogInPage : Page
    {
        public LogInViewModel ViewModel { get; } = new LogInViewModel();

        public LogInPage()
        {
            InitializeComponent();
        }
    }
}
