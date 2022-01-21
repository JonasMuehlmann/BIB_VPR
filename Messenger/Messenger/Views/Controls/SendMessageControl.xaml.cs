using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Messenger.Views.Controls
{
    public sealed partial class SendMessageControl : UserControl
    {
        public SendMessageControlViewModel ViewModel
        {
            get { return (SendMessageControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SendMessageControlViewModel), typeof(SendMessageControl), new PropertyMetadata(null));

        private bool isInSearchMode;
        private DispatcherTimer _timer;
        private string searchTerm;
        private int currentMentionStartIndex;

        public SendMessageControl()
        {
            InitializeComponent();
            isInSearchMode = false;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(300);
            _timer.Tick += OnSearchTimer;
        }

        private void Reset_Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (ToggleButton button in ctlCategory.FindVisualChildren<ToggleButton>())
            {
                if (button.IsChecked == true) button.IsChecked = false;
            }

            ViewModel.ResetEmojisCommand.Execute(null);
        }

        private void tbxContent_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.Accept:
                    if (tbxContent.Text?.Length > 0)
                    {
                        ViewModel.SendMessageCommand?.Execute(ViewModel.ParentViewModel.MessageToSend);
                    }
                    break;
                default:
                    break;
            }
        }

        private void tbxContent_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            string newText = args.NewText;

            if (isInSearchMode
                && newText == " ")
            {
                QuitSearchMode();
                return;
            }

            // Enter search mode
            if (!string.IsNullOrEmpty(newText)
                && newText.EndsWith('@')
                && !isInSearchMode)
            {
                isInSearchMode = true;
                currentMentionStartIndex = newText.Length - 1;
                return;
            }

            if (!isInSearchMode || newText.Length <= currentMentionStartIndex + 1) return;

            searchTerm = newText.Substring(currentMentionStartIndex + 1);

            ViewModel.SearchMentionables(searchTerm);
        }

        private void lbxMentionables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
           Mentionable target = (sender as ListBox).SelectedItem as Mentionable;

            if (target == null || string.IsNullOrEmpty(target.TargetId)) return;

            switch (target.TargetType)
            {
                case MentionTarget.User:
                    string username = target.TargetName.Substring(0, target.TargetName.LastIndexOf('#'));
                    AddAndReplaceMentionableString(username, target);
                    break;
                case MentionTarget.Role:
                case MentionTarget.Channel:
                case MentionTarget.Message:
                case MentionTarget.All:
                    AddAndReplaceMentionableString(target.TargetName, target);
                    break;
                default:
                    break;
            }

            QuitSearchMode();
        }

        private void QuitSearchMode()
        {
            searchTerm = null;
            isInSearchMode = false;
            currentMentionStartIndex = -1;
            ViewModel.ResetSearchedMentionables();
        }

        private void AddAndReplaceMentionableString(string replaceString, Mentionable target)
        {
            target.IndexAndLength = new Tuple<int, int>(currentMentionStartIndex, replaceString.Length + 1);

            tbxContent.Text = tbxContent.Text.Replace("@" + searchTerm, "@" + replaceString + " ");
            tbxContent.Select(tbxContent.Text.Length, 0);
            ViewModel.ParentViewModel.MessageToSend.Mentionables.Add(target);
        }

        private void RemoveReply_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ViewModel.ParentViewModel.ReplyMessage = null;
            ViewModel.ParentViewModel.MessageToSend.ParentMessageId = null;
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            string emojiString = (sender as ToggleButton).Content as string;

            ViewModel.AddEmojiFilter((EmojiCategory)Enum.Parse(typeof(EmojiCategory), emojiString));
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            string emojiString = (sender as ToggleButton).Content as string;

            ViewModel.RemoveEmojiFilter((EmojiCategory)Enum.Parse(typeof(EmojiCategory), emojiString));
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string term = (sender as TextBox).Text;

            if (string.IsNullOrWhiteSpace(term) || term.Length <= 2) return;

            searchTerm = term;

            if (_timer.IsEnabled) _timer.Stop();

            _timer.Start();
        }

        private void Emoji_Tapped(object sender, TappedRoutedEventArgs e)
        {
            string emoji = (sender as TextBlock).Text;

            if (string.IsNullOrWhiteSpace(emoji)) return;

            tbxContent.Text += emoji;
        }

        private void OnSearchTimer(object sender, object e)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return;

            ViewModel.SearchEmojis(searchTerm);

            _timer.Stop();
        }
    }
}
