using Messenger.Commands;
using Messenger.Commands.TeamManage;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.Pages;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class ChatHeader : UserControl
    {
        public ICommand OpenTeamManageCommand { get => new RelayCommand(() => NavigationService.Open<TeamManagePage>()); }

        public ICommand CreateChannelCommand { get => new CreateChannelCommand(); }

        public ICommand OpenSettingsCommand { get => new RelayCommand(() => NavigationService.Open<SettingsPage>()); }

        public ICommand UpdateTeamDetailsCommand { get => new UpdateTeamDetailsCommand(); }

        public TeamViewModel CurrentTeam
        {
            get { return (TeamViewModel)GetValue(CurrentTeamProperty); }
            set { SetValue(CurrentTeamProperty, value); }
        }

        public static readonly DependencyProperty CurrentTeamProperty =
            DependencyProperty.Register("CurrentTeam", typeof(TeamViewModel), typeof(ChatHeader), new PropertyMetadata(null));

        public ChannelViewModel CurrentChannel
        {
            get { return (ChannelViewModel)GetValue(CurrentChannelProperty); }
            set { SetValue(CurrentChannelProperty, value); }
        }

        public static readonly DependencyProperty CurrentChannelProperty =
            DependencyProperty.Register("CurrentChannel", typeof(ChannelViewModel), typeof(ChatHeader), new PropertyMetadata(null));

        public UserViewModel CurrentUser
        {
            get { return (UserViewModel)GetValue(CurrentUserProperty); }
            set { SetValue(CurrentUserProperty, value); }
        }

        public static readonly DependencyProperty CurrentUserProperty =
            DependencyProperty.Register("CurrentUser", typeof(UserViewModel), typeof(ChatHeader), new PropertyMetadata(null));

        public ChatHeader()
        {
            InitializeComponent();
        }
    }
}
