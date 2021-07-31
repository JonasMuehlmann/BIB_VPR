using Messenger.Models;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class InboxControl : UserControl
    {
        public ObservableCollection<Notification> Notifications
        {
            get { return (ObservableCollection<Notification>)GetValue(NotificationsProperty); }
            set { SetValue(NotificationsProperty, value); }
        }

        public static readonly DependencyProperty NotificationsProperty =
            DependencyProperty.Register("Notifications", typeof(ObservableCollection<Notification>), typeof(InboxControl), new PropertyMetadata(new ObservableCollection<Notification>()));

        public InboxControl()
        {
            InitializeComponent();
        }
    }
}
