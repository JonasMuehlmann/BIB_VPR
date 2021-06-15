using System;

using Messenger.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Pickers;
using Windows.Storage;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Messenger.Views
{
    public sealed partial class ChatPage : Page
    {
        public ChatViewModel ViewModel { get; } = new ChatViewModel();

        public ChatPage()
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

        private void Reply_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }

        private void React_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {

        }
    }
}
