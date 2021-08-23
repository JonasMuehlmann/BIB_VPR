using Messenger.Commands;
using Messenger.Services;
using Messenger.Views.Pages;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class SideNavigation : UserControl
    {
        public ICommand NavigateToTeams { get => new RelayCommand(() => NavigationService.Navigate<TeamNavPage>()); }

        public ICommand NavigateToChats { get => new RelayCommand(() => NavigationService.Navigate<ChatNavPage>()); }

        public ICommand NavigateToNotifications { get => new RelayCommand(() => NavigationService.Navigate<NotificationNavPage>()); }

        public SideNavigation()
        {
            InitializeComponent();

            NavigationService.Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, NavigationEventArgs e)
        {
            Type type = e.SourcePageType;

            ToggleButtons(type);
        }

        private void ToggleButtons(Type type)
        {
            if (type == typeof(TeamNavPage))
            {
                TeamsButton.IsChecked = true;
                ChatsButton.IsChecked = false;
                NotificationsButton.IsChecked = false;
            }
            else if (type == typeof(ChatNavPage))
            {
                TeamsButton.IsChecked = false;
                ChatsButton.IsChecked = true;
                NotificationsButton.IsChecked = false;
            }
            else if (type == typeof(NotificationNavPage))
            {
                TeamsButton.IsChecked = false;
                ChatsButton.IsChecked = false;
                NotificationsButton.IsChecked = true;
            }
        }
    }
}
