using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class ActionControl : UserControl
    {
        public ICommand ClearInboxCommand
        {
            get { return (ICommand)GetValue(ClearInboxCommandProperty); }
            set { SetValue(ClearInboxCommandProperty, value); }
        }

        public static readonly DependencyProperty ClearInboxCommandProperty =
            DependencyProperty.Register("ClearInboxCommand", typeof(ICommand), typeof(ActionControl), new PropertyMetadata(null));

        public ICommand RefreshInboxCommand
        {
            get { return (ICommand)GetValue(RefreshInboxCommandProperty); }
            set { SetValue(RefreshInboxCommandProperty, value); }
        }

        public static readonly DependencyProperty RefreshInboxCommandProperty =
            DependencyProperty.Register("RefreshInboxCommand", typeof(ICommand), typeof(ActionControl), new PropertyMetadata(null));

        public ActionControl()
        {
            InitializeComponent();
        }
    }
}
