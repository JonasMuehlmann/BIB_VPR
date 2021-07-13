using Messenger.Core.Models;
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

        public Team CurrentTeam
        {
            get { return (Team)GetValue(CurrentTeamProperty); }
            set { SetValue(CurrentTeamProperty, value); }
        }

        public static readonly DependencyProperty CurrentTeamProperty =
            DependencyProperty.Register("CurrentTeam", typeof(Team), typeof(ChatHeader), new PropertyMetadata(null));

        public ChatHeader()
        {
            InitializeComponent();
        }
    }
}
