using System;

using Messenger.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
{
    // TODO WTS: Change the URL for your privacy policy in the Resource File, currently set to https://YourPrivacyUrlGoesHere
    public sealed partial class SettingsPage : Page
    {
        public bool editUserNameMode = false;
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            ViewModel.UnregisterEvents();
        }

        private void EditButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (editUserNameMode == false)
            {
                UserNametbk.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                UserNametbx.Visibility = Windows.UI.Xaml.Visibility.Visible;
                UserNametbx.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                editUserNameMode = true;
            }
            else if (editUserNameMode == true)
            {
                UserNametbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                UserNametbk.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ViewModel.User.Name = UserNametbx.Text;
                editUserNameMode = false;
            }

        }
    }
}
