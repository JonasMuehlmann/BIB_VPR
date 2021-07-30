using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class OperationConfirmationDialog : ContentDialog
    {
        public bool Response
        {
            get { return (bool)GetValue(ResponseProperty); }
            set { SetValue(ResponseProperty, value); }
        }

        public static readonly DependencyProperty ResponseProperty =
            DependencyProperty.Register("Response", typeof(bool), typeof(OperationConfirmationDialog), new PropertyMetadata(false));

        public string ContentText
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }

        public static readonly DependencyProperty ContentTextProperty =
            DependencyProperty.Register("ContentText", typeof(string), typeof(OperationConfirmationDialog), new PropertyMetadata(string.Empty));

        private OperationConfirmationDialog()
        {
            InitializeComponent();
        }

        public static OperationConfirmationDialog Set(string content)
        {
            return new OperationConfirmationDialog()
            {
                ContentText = content
            };
        }

        private void ContinueButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Response = true;
            Hide();
        }

        private void CancelButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            Response = false;
            Hide();
        }
    }
}
