using Messenger.Commands;
using Messenger.Commands.Messenger;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class MessageView : UserControl
    {
        public MessageViewModel Message
        {
            get { return (MessageViewModel)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(MessageViewModel), typeof(MessageView), new PropertyMetadata(null));

        public CommandBarOverflowButtonVisibility MyMessageActionsVisibility => Message != null && Message.IsMyMessage ? CommandBarOverflowButtonVisibility.Visible : CommandBarOverflowButtonVisibility.Collapsed;

        public ICommand ReplyToCommand
        {
            get { return (ICommand)GetValue(ReplyToCommandProperty); }
            set { SetValue(ReplyToCommandProperty, value); }
        }

        public static readonly DependencyProperty ReplyToCommandProperty =
            DependencyProperty.Register("ReplyToCommand", typeof(ICommand), typeof(MessageView), new PropertyMetadata(null));

        public ICommand UpdateMessageCommand
        {
            get { return (ICommand)GetValue(UpdateMessageCommandProperty); }
            set { SetValue(UpdateMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty UpdateMessageCommandProperty =
            DependencyProperty.Register("UpdateMessageCommand", typeof(ICommand), typeof(MessageView), new PropertyMetadata(null));

        public ICommand DeleteMessageCommand
        {
            get { return (ICommand)GetValue(DeleteMessageCommandProperty); }
            set { SetValue(DeleteMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteMessageCommandProperty =
            DependencyProperty.Register("DeleteMessageCommand", typeof(ICommand), typeof(MessageView), new PropertyMetadata(null));

        public ICommand ToggleReactionCommand
        {
            get { return (ICommand)GetValue(ToggleReactionCommandProperty); }
            set { SetValue(ToggleReactionCommandProperty, value); }
        }

        public static readonly DependencyProperty ToggleReactionCommandProperty =
            DependencyProperty.Register("ToggleReactionCommand", typeof(ICommand), typeof(MessageView), new PropertyMetadata(null));

        public MessageView()
        {
            InitializeComponent();
        }

        #region Reply
        private void ReplyButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ReplyToCommand.Execute(Message);
        }
        #endregion

        #region Reactions

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void LikeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleReactionCommand.Execute(new ToggleReactionArg()
            {
                Type = ReactionType.Like,
                Message = Message
            });
        }

        private void DislikeButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleReactionCommand.Execute(new ToggleReactionArg()
            {
                Type = ReactionType.Dislike,
                Message = Message
            });
        }

        private void SurprisedButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleReactionCommand.Execute(new ToggleReactionArg()
            {
                Type = ReactionType.Surprised,
                Message = Message
            });
        }

        private void AngryButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ToggleReactionCommand.Execute(new ToggleReactionArg()
            {
                Type = ReactionType.Angry,
                Message = Message
            });
        }

        #endregion

        #region Message Edit Mode

        private void EditButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            EnterEditMode();
        }

        private void EditAcceptButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Message == null)
            {
                return;
            }

            UpdateMessageCommand?.Execute(new MessageViewModel()
            {
                Id = Message.Id,
                Content = NewContentTextBox.Text
            });

            ExitEditMode();
        }

        private void EditCancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ExitEditMode();
        }

        private void EnterEditMode()
        {
            EditContent.Visibility = Visibility.Visible;
            ShowContent.Visibility = Visibility.Collapsed;
        }

        private void ExitEditMode()
        {
            EditContent.Visibility = Visibility.Collapsed;
            ShowContent.Visibility = Visibility.Visible;
        }

        #endregion

        private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            DeleteMessageCommand.Execute(Message);
        }
    }
}
