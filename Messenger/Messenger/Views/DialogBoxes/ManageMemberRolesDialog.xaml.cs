using Messenger.Controls.Shared;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.DialogBoxes;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class ManageMemberRolesDialog : ContentDialog
    {
        public ManageMemberRolesDialogViewModel ViewModel { get; set; }

        public RoleTag OpenedTag { get; set; }

        public ManageMemberRolesDialog(MemberViewModel member)
        {
            ViewModel = new ManageMemberRolesDialogViewModel(member);

            InitializeComponent();
        }

        private void CloseButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            Hide();
        }
    }
}
