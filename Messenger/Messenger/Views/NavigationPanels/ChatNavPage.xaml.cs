using System;

using Messenger.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
{
    public sealed partial class ChatNavPage : Page
    {
        public ChatNavViewModel ViewModel { get; } = new ChatNavViewModel();

        public ChatNavPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Console.WriteLine("Test");
            if (e.Parameter as string != "")
            {
                ViewModel.ShellViewModel = e.Parameter as ShellViewModel;
            }
            //Console.WriteLine(e.Parameter as string);
        }

        private void NewChat_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SymbolIcon newChatIcon = (sender as Button).Content as SymbolIcon;
           

            if(SearchField.Visibility == Windows.UI.Xaml.Visibility.Visible)
            {
                SearchField.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                newChatIcon.Symbol = Symbol.Add;
            }
            else
            {
                SearchField.Visibility = Windows.UI.Xaml.Visibility.Visible;
                newChatIcon.Symbol = Symbol.Remove;
            }
        }
    }
}
