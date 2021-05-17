using System;

using Messenger.ViewModels;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
{
    public sealed partial class NotificationNavPage : Page
    {
        public NotificationNavViewModel ViewModel { get; } = new NotificationNavViewModel();

        public NotificationNavPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter as string != "")
            {
                ViewModel.ShellViewModel = e.Parameter as ShellViewModel;
            }
        }
    }
}
