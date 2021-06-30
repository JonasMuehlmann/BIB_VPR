using Messenger.Core.Models;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Messenger.Controls.ChatControls
{
    public sealed partial class MessageControl : UserControl
    {
        public MessageViewModel Message
        {
            get { return (MessageViewModel)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(MessageViewModel), typeof(MessageControl), new PropertyMetadata(null));

        public string EditedContent
        {
            get { return (string)GetValue(EditedContentProperty); }
            set { SetValue(EditedContentProperty, value); }
        }

        public static readonly DependencyProperty EditedContentProperty =
            DependencyProperty.Register("EditedContent", typeof(string), typeof(MessageControl), new PropertyMetadata(string.Empty));

        public ICommand ReplyToCommand
        {
            get { return (ICommand)GetValue(ReplyToCommandProperty); }
            set { SetValue(ReplyToCommandProperty, value); }
        }

        public static readonly DependencyProperty ReplyToCommandProperty =
            DependencyProperty.Register("ReplyToCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public ICommand EnterEditModeCommand
        {
            get { return (ICommand)GetValue(EnterEditModeCommandProperty); }
            set { SetValue(EnterEditModeCommandProperty, value); }
        }

        public static readonly DependencyProperty EnterEditModeCommandProperty =
            DependencyProperty.Register("EnterEditModeCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public ICommand ExitEditModeCommand
        {
            get { return (ICommand)GetValue(ExitEditModeCommandProperty); }
            set { SetValue(ExitEditModeCommandProperty, value); }
        }

        public static readonly DependencyProperty ExitEditModeCommandProperty =
            DependencyProperty.Register("ExitEditModeCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public ICommand EditMessageCommand
        {
            get { return (ICommand)GetValue(EditMessageCommandProperty); }
            set { SetValue(EditMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty EditMessageCommandProperty =
            DependencyProperty.Register("EditMessageCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public ICommand DeleteMessageCommand
        {
            get { return (ICommand)GetValue(DeleteMessageCommandProperty); }
            set { SetValue(DeleteMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteMessageCommandProperty =
            DependencyProperty.Register("DeleteMessageCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public ICommand ToggleReactionCommand
        {
            get { return (ICommand)GetValue(ToggleReactionCommandProperty); }
            set { SetValue(ToggleReactionCommandProperty, value); }
        }

        public static readonly DependencyProperty ToggleReactionCommandProperty =
            DependencyProperty.Register("ToggleReactionCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public ReactionType MyReaction
        {
            get { return (ReactionType)GetValue(MyReactionProperty); }
            set { SetValue(MyReactionProperty, value); }
        }

        public static readonly DependencyProperty MyReactionProperty =
            DependencyProperty.Register("MyReaction", typeof(ReactionType), typeof(MessageControl), new PropertyMetadata(ReactionType.None));

        public MessageControl()
        {
            EnterEditModeCommand = new RelayCommand(EnterEditMode);
            ExitEditModeCommand = new RelayCommand(ExitEditMode);

            InitializeComponent();
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void EnterEditMode()
        {
            EditContent.Visibility = Visibility.Visible;
            ShowContent.Visibility = Visibility.Collapsed;

            EditedContent = ShowContent.Text;
        }

        private void ExitEditMode()
        {
            EditContent.Visibility = Visibility.Collapsed;
            ShowContent.Visibility = Visibility.Visible;
        }

        private void EditAcceptButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Message == null)
            {
                return;
            }

            var vm = new MessageViewModel()
            {
                Id = Message.Id,
                Content = NewContentTextBox.Text
            };

            EditMessageCommand?.Execute(vm);

            ExitEditMode();
        }

        private void DeleteButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Message == null
                || Message.Sender == null)
            {
                return;
            }

            var vm = new MessageViewModel()
            {
                Id = Message.Id,
                Sender = Message.Sender
            };

            DeleteMessageCommand?.Execute(vm);
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
    }
}
