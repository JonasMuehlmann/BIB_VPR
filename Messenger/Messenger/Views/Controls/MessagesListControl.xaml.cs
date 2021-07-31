using Messenger.ViewModels.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Controls
{
    public sealed partial class MessagesListControl : UserControl
    {
        public MessagesListControlViewModel ViewModel
        {
            get { return (MessagesListControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MessagesListControlViewModel), typeof(MessagesListControl), new PropertyMetadata(null));

        public MessagesListControl()
        {
            InitializeComponent();
        }
    }
}
