using Messenger.Commands.Messenger;
using Messenger.ViewModels.DataViewModels;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class ReplyView : UserControl
    {
        public MessageViewModel Reply
        {
            get { return (MessageViewModel)GetValue(ReplyProperty); }
            set { SetValue(ReplyProperty, value); }
        }

        public static readonly DependencyProperty ReplyProperty =
            DependencyProperty.Register("Reply", typeof(MessageViewModel), typeof(ReplyView), new PropertyMetadata(new MessageViewModel()));

        public ICommand ToggleReactionCommand { get => new ToggleReactionCommand(); }

        public ICommand DeleteReplyCommand { get => new DeleteMessageCommand(); }
        //{
        //    get { return (ICommand)GetValue(DeleteReplyCommandProperty); }
        //    set { SetValue(DeleteReplyCommandProperty, value); }
        //}

        //public static readonly DependencyProperty DeleteReplyCommandProperty =
        //    DependencyProperty.Register("DeleteReplyCommand", typeof(ICommand), typeof(ReplyView), new PropertyMetadata(null));

        public ReplyView()
        {
            InitializeComponent();
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void Delete_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DeleteReplyCommand.Execute(Reply);
        }
    }
}
