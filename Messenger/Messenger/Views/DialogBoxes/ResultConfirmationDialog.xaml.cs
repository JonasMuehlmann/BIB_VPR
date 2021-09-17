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

        private ResultConfirmationDialog()
        {
            InitializeComponent();
        }

        public static ResultConfirmationDialog Set(bool isSuccess, object message)
        {
            string m;
            try
            {
                m = (string)message;
            }
            catch (System.Exception e)
            {
                return null;
            }
            ResultConfirmationDialog dialog = new ResultConfirmationDialog
            {
                IsSuccess = isSuccess,
                ContentText = m
            };

            if (isSuccess)
            {
                dialog.HeaderText.Text = "Success!";
                dialog.HeaderSymbol.Symbol = Symbol.Accept;
                dialog.HeaderSymbol.Foreground = new SolidColorBrush(Colors.MediumSeaGreen);
            }
            else
            {
                dialog.HeaderText.Text = "Something went wrong...";
                dialog.HeaderSymbol.Symbol = Symbol.DisableUpdates;
                dialog.HeaderSymbol.Foreground = new SolidColorBrush(Colors.IndianRed);
            }

            return dialog;
        }

        private void OnConfirm(object sender, RoutedEventArgs e)
        {
            Hide();
        }
    }
}
