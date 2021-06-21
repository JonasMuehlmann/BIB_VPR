using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// Die Elementvorlage "Inhaltsdialogfeld" wird unter https://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.

namespace Messenger.Views.DialogBoxes
{
    public sealed partial class ResultConfirmationDialog : ContentDialog
    {
        public bool IsSuccess
        {
            get { return (bool)GetValue(IsSuccessProperty); }
            set { SetValue(IsSuccessProperty, value); }
        }

        public static readonly DependencyProperty IsSuccessProperty =
            DependencyProperty.Register("IsSuccess", typeof(bool), typeof(ResultConfirmationDialog), new PropertyMetadata(false));

        public string ContentText
        {
            get { return (string)GetValue(ContentTextProperty); }
            set { SetValue(ContentTextProperty, value); }
        }

        public static readonly DependencyProperty ContentTextProperty =
            DependencyProperty.Register("ContentText", typeof(string), typeof(ResultConfirmationDialog), new PropertyMetadata(string.Empty));

        public ResultConfirmationDialog(bool isSuccess, string contentText)
        {
            InitializeComponent();

            IsSuccess = isSuccess;
            ContentText = contentText;

            if (isSuccess)
            {
                HeaderText.Text = "Success!";
                HeaderSymbol.Symbol = Symbol.Accept;
                HeaderSymbol.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
            }
            else
            {
                HeaderText.Text = "Something went wrong...";
                HeaderSymbol.Symbol = Symbol.DisableUpdates;
                HeaderSymbol.Foreground = new SolidColorBrush(Colors.IndianRed);
            }
        }

        public static ResultConfirmationDialog Set(bool isSuccess, string contentText)
        {
            var dialog = new ResultConfirmationDialog(isSuccess, contentText);

            return dialog;
        }

        private void OnConfirm(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
