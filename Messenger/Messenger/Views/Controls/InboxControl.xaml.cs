using Messenger.Models;
using Messenger.ViewModels.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Controls
{
    public sealed partial class InboxControl : UserControl
    {
        public InboxControlViewModel ViewModel
        {
            get { return (InboxControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(InboxControlViewModel), typeof(InboxControl), new PropertyMetadata(null));

        public InboxControl()
        {
            InitializeComponent();
        }
    }
}
