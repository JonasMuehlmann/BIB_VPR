using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class ChangeTeamDialog : ContentDialog
    {
        public string TeamName
        {
            get { return (string)GetValue(TeamNameProperty); }
            set { SetValue(TeamNameProperty, value); }
        }

        /// <summary>
        /// Name of the team to create
        /// </summary>
        public static readonly DependencyProperty TeamNameProperty =
            DependencyProperty.Register("TeamName", typeof(string), typeof(ChangeTeamDialog), new PropertyMetadata(string.Empty));

        public string TeamDescription
        {
            get { return (string)GetValue(TeamDescriptionProperty); }
            set { SetValue(TeamDescriptionProperty, value); }
        }

        /// <summary>
        /// Description of the team to create (optional)
        /// </summary>
        public static readonly DependencyProperty TeamDescriptionProperty =
            DependencyProperty.Register("TeamDescription", typeof(string), typeof(ChangeTeamDialog), new PropertyMetadata(string.Empty));

        public ChangeTeamDialog()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Fires if user clicks the confirm button
        /// </summary>
        /// <param name="sender">Sender type is ContentDialog</param>
        /// <param name="args">Click event args</param>
        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (string.IsNullOrEmpty(TeamName))
            {
                args.Cancel = true;
                errorTextBlock.Text = "Team name is required.";
            }
            else if (TeamName.Length < 3)
            {
                args.Cancel = true;
                errorTextBlock.Text = "Name should have at least 3 letters.";
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }
    }
}
