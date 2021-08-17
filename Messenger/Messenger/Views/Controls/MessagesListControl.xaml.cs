using Messenger.ViewModels.Controls;
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
            set {
                SetValue(ViewModelProperty, value);
                ViewModel.Messages.CollectionChanged += Messages_CollectionChanged;
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
            //messagesListView.ScrollIntoView(messagesListView.Items[messagesListView.Items.Count - 1], ScrollIntoViewAlignment.Leading);
            GetScrollViewer(messagesListView).ChangeView(0, 100, null);
        }
        private static ScrollViewer GetScrollViewer(DependencyObject element)
        {
            if (element is ScrollViewer)
            {
                return (ScrollViewer)element;
            }

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(element); i++)
            {
                var child = VisualTreeHelper.GetChild(element, i);

                var result = GetScrollViewer(child);
                if (result == null)
                {
                    continue;
                }
                else
                {
                    return result;
                }
            }

            return null;
        }
    }
}
