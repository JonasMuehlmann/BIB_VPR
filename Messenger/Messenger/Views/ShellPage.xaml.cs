using System;
using Messenger.Services;
using Messenger.ViewModels;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Messenger.Views
{
    // TODO WTS: Change the icons and titles for all NavigationViewItems in ShellPage.xaml.
    public sealed partial class ShellPage : Page
    {
        public ShellViewModel ViewModel { get; } = new ShellViewModel();

        public ShellPage()
        {
            InitializeComponent();
            NavigationService.Frame = sideFrame;
            NavigationService.ContentFrame = shellFrame;
            DataContext = ViewModel;
        }
    }
}
