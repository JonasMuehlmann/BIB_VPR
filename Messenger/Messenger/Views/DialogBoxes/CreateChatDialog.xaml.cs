using Windows.UI.Xaml.Controls;
using Messenger.ViewModels.DialogBoxes;

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class CreateChatDialog : ContentDialog
    {
        public CreateChatDialogViewModel ViewModel { get; set; }

        public CreateChatDialog()
        {
            ViewModel = new CreateChatDialogViewModel();
            InitializeComponent();
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (ViewModel.UserSearchPanelViewModel.SelectedUser == null)
            {
                args.Cancel = true;
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Hide();
        }
    }
}
