using Messenger.Core.Models;
using Messenger.ViewModels.Controls;
using System;
using System.Collections.Generic;
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

        private DispatcherTimer _timer;
        private string _searchTerm;

        public SendMessageControl()
        {
            InitializeComponent();
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(300);
            _timer.Tick += OnSearchTimer;
        }

        private void Reset_Tapped(object sender, TappedRoutedEventArgs e)
        {
            foreach (var item in FindVisualChildren<ToggleButton>(ctlCategory))
            {
                ToggleButton button = item as ToggleButton;

                if (button.IsChecked == true)
                {
                    button.IsChecked = false;
                }
            }

            ViewModel.ResetEmojisCommand.Execute(null);
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null)
                yield break;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                if (child != null && child is T)
                    yield return (T)child;

                foreach (T childOfChild in FindVisualChildren<T>(child))
                    yield return childOfChild;
            }
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

            _searchTerm = term;

            if (_timer.IsEnabled)
            {
                _timer.Stop();
            }

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
            if (string.IsNullOrWhiteSpace(_searchTerm)) return;

            ViewModel.SearchEmojis(_searchTerm);

            _timer.Stop();
        }
    }
}
