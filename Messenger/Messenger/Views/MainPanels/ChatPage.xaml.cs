using System;

using Messenger.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

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
    }
}
