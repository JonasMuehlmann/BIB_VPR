using System;
using Messenger.ViewModels.Pages;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views.Pages
{
    public sealed partial class SettingsPage : Page
    {
        public bool editUserNameMode = false;
        public SettingsViewModel ViewModel { get; } = new SettingsViewModel();

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            switch (ViewModel.ElementTheme)
            {
                case ElementTheme.Light:
                    ThemeGridView.SelectedIndex = 0;
                    break;
                case ElementTheme.Dark:
                    ThemeGridView.SelectedIndex = 1;
                    break;
                default:
                    break;
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);
            ViewModel.UnregisterEvents();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (editUserNameMode == false)
            {
                UserNametbk.Visibility = Visibility.Collapsed;
                UserNametbx.Visibility = Visibility.Visible;
                UserNametbx.Focus(FocusState.Keyboard);
                UserNametbx.Select(UserNametbx.Text.Length, 0);
                editUserNameMode = true;
            }
            else if (editUserNameMode == true)
            {
                if (UserNametbx.Text  == "")
                {
                    UserNametbx.Focus(FocusState.Keyboard);
                    UserNametbx.Select(UserNametbx.Text.Length, 0);
                    return;
                }
                UserNametbx.Visibility = Visibility.Collapsed;
                UserNametbk.Visibility = Visibility.Visible;
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
                UserNametbx.Visibility = Visibility.Collapsed;
                UserNametbk.Visibility = Visibility.Visible;
                ViewModel.User.Name = UserNametbx.Text;
                ViewModel.UpdateUsernameCommand?.Execute(ViewModel.User.Name);
            }
        }

        private void GridView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Enum.TryParse(typeof(ElementTheme), ((sender as GridView).SelectedItem as GridViewItem).Tag.ToString(), out object result))
            {
                ViewModel.SwitchThemeCommand?.Execute(result);
            }
        }

        private void EditCancelButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ViewModel.CurrentBio))
            {
                UserBioPlaceholder.Visibility = Visibility.Visible;
            }
            else
            {
                UserBioTextBlock.Visibility = Visibility.Visible;
            }

            EditBioPanel.Visibility = Visibility.Collapsed;
        }

        private void EditAcceptButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            ViewModel.UpdateUserBioCommand?.Execute(ViewModel.CurrentBio);

            EditBioPanel.Visibility = Visibility.Collapsed;
            UserBioTextBlock.Visibility = Visibility.Visible;
        }

        private void EditButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            UserBioPlaceholder.Visibility = Visibility.Collapsed;
            UserBioTextBlock.Visibility = Visibility.Collapsed;

            EditBioPanel.Visibility = Visibility.Visible;
        }
    }
}
