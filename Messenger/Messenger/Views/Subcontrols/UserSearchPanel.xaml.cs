using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class UserSearchPanel : UserControl
    {
        public static readonly DependencyProperty OnSearchProperty =
            DependencyProperty.Register("OnSearch", typeof(Func<string, Task<IReadOnlyList<string>>>), typeof(UserSearchPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty GetSelectedUserProperty =
            DependencyProperty.Register("GetSelectedUser", typeof(Func<string, uint, Task<User>>), typeof(UserSearchPanel), new PropertyMetadata(null));

        public static readonly DependencyProperty SelectedUserProperty =
            DependencyProperty.Register("SelectedUser", typeof(User), typeof(UserSearchPanel), new PropertyMetadata(null));

        public Func<string, Task<IReadOnlyList<string>>> OnSearch
        {
            get { return (Func<string, Task<IReadOnlyList<string>>>)GetValue(OnSearchProperty); }
            set { SetValue(OnSearchProperty, value); }
        }

        public Func<string, uint, Task<User>> GetSelectedUser
        {
            get { return (Func<string, uint, Task<User>>)GetValue(GetSelectedUserProperty); }
            set { SetValue(GetSelectedUserProperty, value); }
        }

        public User SelectedUser
        {
            get { return (User)GetValue(SelectedUserProperty); }
            set { SetValue(SelectedUserProperty, value); }
        }

        public UserSearchPanel()
        {
            InitializeComponent();
        }

        private async void SearchUserBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput
                && sender.Text.Length > 2)
            {

                var searchUsers = await OnSearch?.Invoke(sender.Text);

                if (searchUsers == null)
                {
                    return;
                }

                sender.ItemsSource = searchUsers.ToList();
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

            SelectedUser = selected;
        }
    }
}
