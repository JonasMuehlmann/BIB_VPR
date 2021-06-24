using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class CreateChatDialog : ContentDialog
    {
        public string UserName
        {
            get { return (string)GetValue(UserNameProperty); }
            set { SetValue(UserNameProperty, value); }
        }

        public static readonly DependencyProperty UserNameProperty =
            DependencyProperty.Register("UserName", typeof(string), typeof(CreateChatDialog), new PropertyMetadata(string.Empty));

        public User SelectedUser
        {
            get { return (User)GetValue(SelectedUserProperty); }
            set { SetValue(SelectedUserProperty, value); }
        }

        public static readonly DependencyProperty SelectedUserProperty =
            DependencyProperty.Register("SelectedUser", typeof(User), typeof(CreateChatDialog), new PropertyMetadata(null));

        public ObservableCollection<User> SearchResults
        {
            get { return (ObservableCollection<User>)GetValue(SearchResultsProperty); }
            set { SetValue(SearchResultsProperty, value); }
        }

        public static readonly DependencyProperty SearchResultsProperty =
            DependencyProperty.Register("SearchResults", typeof(ObservableCollection<User>), typeof(CreateChatDialog), new PropertyMetadata(new ObservableCollection<User>()));

        public Func<string, Task<IList<string>>> OnSearch
        {
            get { return (Func<string, Task<IList<string>>>)GetValue(OnSearchProperty); }
            set { SetValue(OnSearchProperty, value); }
        }

        public static readonly DependencyProperty OnSearchProperty =
            DependencyProperty.Register("OnSearch", typeof(Func<string, Task<IList<string>>>), typeof(CreateChatDialog), new PropertyMetadata(null));

        public Func<string, uint, Task<User>> GetSelectedUser
        {
            get { return (Func<string, uint, Task<User>>)GetValue(GetSelectedUserProperty); }
            set { SetValue(GetSelectedUserProperty, value); }
        }

        public static readonly DependencyProperty GetSelectedUserProperty =
            DependencyProperty.Register("GetSelectedUser", typeof(Func<string, uint, Task<User>>), typeof(CreateChatDialog), new PropertyMetadata(null));

        public CreateChatDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                args.Cancel = true;
            }
            else if (SelectedUser == null)
            {
                args.Cancel = true;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void SearchUserBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var searchUsers = await OnSearch?.Invoke(sender.Text);

                sender.ItemsSource = searchUsers;
            }
        }

        private async void SearchUserBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            string searchString = args.SelectedItem.ToString();
            string[] userdata = searchString.Split('#');
            string displayName = userdata[0];
            uint nameId = Convert.ToUInt32(userdata[1]);

            sender.Text = searchString;

            User selected = await GetSelectedUser?.Invoke(displayName, nameId);

            UserInfoPanel.DataContext = selected;
            SelectedUser = selected;

            if (UserInfoPanel.Visibility != Visibility.Visible)
            {
                UserInfoPanel.Visibility = Visibility.Visible;
            }
        }
    }
}
