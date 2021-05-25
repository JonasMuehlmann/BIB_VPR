using System;

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

        private SymbolIcon lastSelected;

        public ShellPage()
        {
            InitializeComponent();
            DataContext = ViewModel;
            ViewModel.Initialize(shellFrame, sideFrame);
            //get default colors
            lastSelected = ChatIcon;
        }

        private void NavigationButton_Clicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SymbolIcon icon = (sender as Button).Content as SymbolIcon;
            if (lastSelected != icon)
            {
                icon.Foreground = Resources["SystemAccent"] as Brush;
                lastSelected.Foreground = Resources["SystemTextColor"] as Brush;
                lastSelected = icon;
            }
        }
    }
}
