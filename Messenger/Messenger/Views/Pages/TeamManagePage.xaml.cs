using Messenger.ViewModels.Pages;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Pages
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
