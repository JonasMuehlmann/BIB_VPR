using Messenger.Services;
using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Pages
{
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            NavigationService.Frame = NavigationFrame;
            NavigationService.ContentFrame = ContentFrame;
            DataContext = ViewModel;
        }
    }
}
