using Messenger.ViewModels.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Controls
{
    public sealed partial class UserSearchPanel : UserControl
    {
        public UserSearchPanelViewModel ViewModel
        {
            get { return (UserSearchPanelViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(UserSearchPanelViewModel), typeof(UserSearchPanel), new PropertyMetadata(null));

        public UserSearchPanel()
        {
            InitializeComponent();
        }

        private void SearchUserBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput
                && sender.Text.Length > 2)
            {
                ViewModel.SearchCommand?.Execute(sender.Text);
            }
        }

        private void SearchUserBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            ViewModel.GetSelectedUserCommand?.Execute(args.SelectedItem.ToString());
        }
    }
}
