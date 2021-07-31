using Messenger.ViewModels.DataViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Messenger.Views.Subcontrols
{
    public sealed partial class RoleTag : UserControl
    {
        public TeamRoleViewModel Role
        {
            get { return (TeamRoleViewModel)GetValue(RoleProperty); }
            set { SetValue(RoleProperty, value); }
        }

        public static readonly DependencyProperty RoleProperty =
            DependencyProperty.Register("Role", typeof(TeamRoleViewModel), typeof(RoleTag), new PropertyMetadata(null));

        public RoleTag()
        {
            InitializeComponent();
        }
    }
}
