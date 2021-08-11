using Messenger.ViewModels.DataViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class UserProfileFlyout : UserControl
    {
        public MemberViewModel Member
        {
            get { return (MemberViewModel)GetValue(MemberProperty); }
            set { SetValue(MemberProperty, value); }
        }

        public static readonly DependencyProperty MemberProperty =
            DependencyProperty.Register("Member", typeof(MemberViewModel), typeof(UserProfileFlyout), new PropertyMetadata(null));

        public UserProfileFlyout()
        {
            InitializeComponent();
        }
    }
}
