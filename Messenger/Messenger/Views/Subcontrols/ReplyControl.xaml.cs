using Messenger.ViewModels.DataViewModels;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class ReplyControl : UserControl
    {
        public MessageViewModel Reply
        {
            get { return (MessageViewModel)GetValue(ReplyProperty); }
            set { SetValue(ReplyProperty, value); }
        }

        public static readonly DependencyProperty ReplyProperty =
            DependencyProperty.Register("Reply", typeof(MessageViewModel), typeof(ReplyControl), new PropertyMetadata(new MessageViewModel()));

        public ICommand ToggleReactionCommand
        {
            get { return (ICommand)GetValue(ToggleReactionCommandProperty); }
            set { SetValue(ToggleReactionCommandProperty, value); }
        }

        public static readonly DependencyProperty ToggleReactionCommandProperty =
            DependencyProperty.Register("ToggleReactionCommand", typeof(ICommand), typeof(ReplyControl), new PropertyMetadata(null));

        public ReplyControl()
        {
            InitializeComponent();
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
