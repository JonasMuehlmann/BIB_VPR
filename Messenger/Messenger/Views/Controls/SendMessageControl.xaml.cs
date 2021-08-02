using Messenger.ViewModels.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace Messenger.Views.Controls
{
    public sealed partial class SendMessageControl : UserControl
    {
        public SendMessageControlViewModel ViewModel
        {
            get { return (SendMessageControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(SendMessageControlViewModel), typeof(SendMessageControl), new PropertyMetadata(null));

        public SendMessageControl()
        {
            InitializeComponent();
        }

        private void tbxContent_PreviewKeyDown(object sender, KeyRoutedEventArgs e)
        {
            switch (e.Key)
            {
                case Windows.System.VirtualKey.Enter:
                case Windows.System.VirtualKey.Accept:
                    if (tbxContent.Text?.Length > 0)
                    {
                        ViewModel.SendMessageCommand?.Execute(ViewModel.ParentViewModel.MessageToSend);
                    }
                    break;
                default:
                    break;
            }
        }
    }
}
