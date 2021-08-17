using Messenger.ViewModels.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Messenger.Views.Controls
{
    public sealed partial class MessagesListControl : UserControl
    {
        public MessagesListControlViewModel ViewModel
        {
            get { return (MessagesListControlViewModel)GetValue(ViewModelProperty); }
            set
            {
                SetValue(ViewModelProperty, value);
                ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
                Messages_CollectionChanged(null, null);
            }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MessagesListControlViewModel), typeof(MessagesListControl), new PropertyMetadata(null));

        public MessagesListControl()
        {
            InitializeComponent();
        }


        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            messagesScrollView.ChangeView(0, messageListView.ActualHeight, null);
        }
    }
}
