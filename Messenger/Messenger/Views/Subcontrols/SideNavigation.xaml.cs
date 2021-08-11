using Messenger.Commands;
using Messenger.Services;
using Messenger.Views.Pages;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

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
        }
    }
}
