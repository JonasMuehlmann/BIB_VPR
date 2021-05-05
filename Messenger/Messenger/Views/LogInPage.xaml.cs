using System;

using Messenger.ViewModels;

using Windows.UI.Xaml.Controls;

namespace Messenger.Views
{
    public sealed partial class LogInPage : Page
    {
        public LogInViewModel ViewModel { get; } = new LogInViewModel();

        public LogInPage()
        {
            InitializeComponent();
        }
    }
}
