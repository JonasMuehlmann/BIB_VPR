using Messenger.ViewModels.DataViewModels;
using System.Linq;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Controls.ChatControls
{
    public sealed partial class ChatHeader : UserControl
    {
        public ICommand OpenTeamManagerCommand
        {
            get { return (ICommand)GetValue(OpenTeamManagerCommandProperty); }
            set { SetValue(OpenTeamManagerCommandProperty, value); }
        }

        public static readonly DependencyProperty OpenTeamManagerCommandProperty =
            DependencyProperty.Register("OpenTeamManagerCommand", typeof(ICommand), typeof(ChatHeader), new PropertyMetadata(null));

        public ICommand OpenSettingsCommand
        {
            get { return (ICommand)GetValue(OpenSettingsCommandProperty); }
            set { SetValue(OpenSettingsCommandProperty, value); }
        }

        public static readonly DependencyProperty OpenSettingsCommandProperty =
            DependencyProperty.Register("OpenSettingsCommand", typeof(ICommand), typeof(ChatHeader), new PropertyMetadata(null));

        public ICommand EditTeamDetailsCommand
        {
            get { return (ICommand)GetValue(EditTeamDetailsCommandProperty); }
            set { SetValue(EditTeamDetailsCommandProperty, value); }
        }

        public static readonly DependencyProperty EditTeamDetailsCommandProperty =
            DependencyProperty.Register("EditTeamDetailsCommand", typeof(ICommand), typeof(ChatHeader), new PropertyMetadata(null));

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

        public UserViewModel CurrentUser { get => App.StateProvider?.CurrentUser; }

        public ChatHeader()
        {
            InitializeComponent();
        }
    }
}
