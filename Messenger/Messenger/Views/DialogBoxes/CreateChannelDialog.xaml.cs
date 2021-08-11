using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class CreateChannelDialog : ContentDialog
    {
        public string ChannelName
        {
            get { return (string)GetValue(ChannelNameProperty); }
            set { SetValue(ChannelNameProperty, value); }
        }

        /// <summary>
        /// Name of the team to create
        /// </summary>
        public static readonly DependencyProperty ChannelNameProperty =
            DependencyProperty.Register("ChannelName", typeof(string), typeof(CreateTeamDialog), new PropertyMetadata(string.Empty));



        public CreateChannelDialog()
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
            if (string.IsNullOrEmpty(ChannelName))
            {
                args.Cancel = true;
                errorTextBlock.Text = "Team name is required.";
            }
            else if (ChannelName.Length < 3)
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
