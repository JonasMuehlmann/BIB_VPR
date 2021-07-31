using Messenger.ViewModels.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Controls
{
    public sealed partial class MembersListControl : UserControl
    {
        public MembersListControlViewModel ViewModel
        {
            get { return (MembersListControlViewModel)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register("ViewModel", typeof(MembersListControlViewModel), typeof(MembersListControl), new PropertyMetadata(null));

        public MembersListControl()
        {
            InitializeComponent();
        }
    }
}
