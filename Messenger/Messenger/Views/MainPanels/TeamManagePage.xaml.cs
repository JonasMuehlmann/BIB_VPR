using System;

using Messenger.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
{
    public sealed partial class TeamManagePage: Page
    {
        public TeamManageViewModel ViewModel { get; } = new TeamManageViewModel();

        public TeamManagePage()
        {
            InitializeComponent();

            AddMember.BorderThickness = new Windows.UI.Xaml.Thickness(0,0,0,3);
            RemoveMember.BorderThickness = new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Console.WriteLine("Test");
            if (e.Parameter as string != "")
            {
                ViewModel.ShellViewModel = e.Parameter as ShellViewModel;
            }
            //Console.WriteLine(e.Parameter as string);
        }

        private void AddMember_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            RemoveMember.BorderThickness = new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
            AddMember.BorderThickness = new Windows.UI.Xaml.Thickness(0, 0, 0, 3);
        }

        private void RemoveMember_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AddMember.BorderThickness = new Windows.UI.Xaml.Thickness(0, 0, 0, 0);
            RemoveMember.BorderThickness = new Windows.UI.Xaml.Thickness(0, 0, 0, 3);
        }
    }
}
