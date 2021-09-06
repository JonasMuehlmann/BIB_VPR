using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.Controls;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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

        private bool _mentionableSearchMode = false;

        public SendMessageControl()
        {
            InitializeComponent();
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

        private async void tbxContent_BeforeTextChanging(TextBox sender, TextBoxBeforeTextChangingEventArgs args)
        {
            string newText = args.NewText;
            string oldText = sender.Text;

            if (newText.Length < oldText.Length) return;

            if (newText[newText.Length - 1] == '@' && !_mentionableSearchMode)
            {
                _mentionableSearchMode = true;
                return;
            }
            else if (newText[newText.Length - 1] == ' ' && _mentionableSearchMode)
            {
                _mentionableSearchMode = false;
                return;
            }

            if (_mentionableSearchMode)
            {
                int searchStringIndex = newText.IndexOf('@');

                if (newText.Substring(searchStringIndex).Length > 1)
                {
                    string searchString = newText.Substring(searchStringIndex + 1);

                    IList<Mentionable> mentionables = await MentionService.SearchMentionable(searchString, App.StateProvider.SelectedTeam.Id);


                    if (mentionables != null && mentionables.Count > 0)
                    {

                    }
                }
            }       
        }
    }
}
