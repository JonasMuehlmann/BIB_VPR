using Messenger.ViewModels.DialogBoxes;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class ManageRolesDialog : ContentDialog
    {
        public ManageRolesDialogViewModel ViewModel { get; } = new ManageRolesDialogViewModel();

        public ManageRolesDialog()
        {
            InitializeComponent();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        

        private void SelectedPermissions_ItemClick(object sender, ItemClickEventArgs e)
        {

        }

        private void ColorPickerCancelButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPickerButton.Flyout.Hide();
        }

        private void ColorPickerOkButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedTeamRole.Color = colorPicker.Color;
        }

        private void colorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            ViewModel.SelectedTeamRole.Color = sender.Color;
        }

        private void manageRolesDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }
    }
}
