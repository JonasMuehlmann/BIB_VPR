using System;

using Messenger.ViewModels;

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

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter as string != "")
            {
                ViewModel.ShellViewModel = e.Parameter as ShellViewModel;
            }
            await ViewModel.LoadDataAsync();
        }
    }
}
