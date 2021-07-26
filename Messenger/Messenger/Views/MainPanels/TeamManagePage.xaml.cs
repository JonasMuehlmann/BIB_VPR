using System;

using Messenger.ViewModels;

using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Views
{
    public sealed partial class TeamManagePage: Page
    {
        public TeamManageViewModel ViewModel { get; } = new TeamManageViewModel();

        public TeamManagePage()
        {
            InitializeComponent();
        }
    }
}
