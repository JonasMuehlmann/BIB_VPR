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
    }
}
