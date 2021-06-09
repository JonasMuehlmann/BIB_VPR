using System;

using Messenger.ViewModels;
using Windows.System;
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
                UserNametbx.Select(UserNametbx.Text.Length, 0);
                editUserNameMode = true;
            }
            else if (editUserNameMode == true)
            {
                if (UserNametbx.Text  == "")
                {
                    UserNametbx.Focus(Windows.UI.Xaml.FocusState.Keyboard);
                    UserNametbx.Select(UserNametbx.Text.Length, 0);
                    return;
                }
                UserNametbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                UserNametbk.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ViewModel.User.Name = UserNametbx.Text;
                editUserNameMode = false;
            }
        }

        private void UserNametbx_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (UserNametbx.Text == "")
                {
                    return;
                }
                UserNametbx.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                UserNametbk.Visibility = Windows.UI.Xaml.Visibility.Visible;
                ViewModel.User.Name = UserNametbx.Text;
            }
        }
    }
}
