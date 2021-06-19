using Messenger.Core.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

        public CreateChatDialog()
        {
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(UserName))
            {
                args.Cancel = true;
                errorTextBlock.Text = "No user was selected.";
            }
            else if (SelectedUser == null)
            {
                args.Cancel = true;
                errorTextBlock.Text = "Please choose the user from the search result.";
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (UserName.Length < 3)
            {
                return;
            }

            var result = await OnSearch?.Invoke(UserName);

            if (result != null && result.Count > 0)
            {
                SearchResults.Clear();

                foreach (var userString in result)
                {
                    var userdata = userString.Split('#');
                    var displayName = userdata[0];
                    var nameId = Convert.ToUInt32(userdata[1]);

                    SearchResults.Add(new User() { DisplayName = displayName, NameId = nameId });
                }
            }
        }
    }
}
