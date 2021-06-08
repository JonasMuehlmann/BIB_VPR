using System;

using Messenger.ViewModels;
using Windows.System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
{
    public sealed partial class TeamNavPage : Page
    {
        public TeamNavViewModel ViewModel { get; } = new TeamNavViewModel();

        public TeamNavPage()
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

        private void NewTeam_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SymbolIcon newTeamIcon = (sender as Button).Content as SymbolIcon;


            if (SearchField.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                SearchField.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                newTeamIcon.Symbol = Symbol.Add;
            }
            else
            {
                SearchField.Visibility = Windows.UI.Xaml.Visibility.Visible;
                newTeamIcon.Symbol = Symbol.Remove;
            }
        }

        private void NewTeam_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                TextBox tb = sender as TextBox;

                ViewModel.NewTeam(tb.Text, "");
            }
        }
    }
}
