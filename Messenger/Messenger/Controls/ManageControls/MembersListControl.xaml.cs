using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Die Elementvorlage "Benutzersteuerelement" wird unter https://go.microsoft.com/fwlink/?LinkId=234236 dokumentiert.

namespace Messenger.Controls.ManageControls
{
    public sealed partial class MembersListControl : UserControl
    {
        public ICommand OpenManageRolesCommand
        {
            get { return (ICommand)GetValue(OpenManageRolesCommandProperty); }
            set { SetValue(OpenManageRolesCommandProperty, value); }
        }

        public static readonly DependencyProperty OpenManageRolesCommandProperty =
            DependencyProperty.Register("OpenManageRolesCommand", typeof(ICommand), typeof(MembersListControl), new PropertyMetadata(null));

        public ObservableCollection<MemberViewModel> Members
        {
            get { return (ObservableCollection<MemberViewModel>)GetValue(MembersProperty); }
            set { SetValue(MembersProperty, value); }
        }

        public static readonly DependencyProperty MembersProperty =
            DependencyProperty.Register("Members", typeof(ObservableCollection<MemberViewModel>), typeof(MembersListControl), new PropertyMetadata(new ObservableCollection<MemberViewModel>()));

        public ICommand RemoveUserCommand
        {
            get { return (ICommand)GetValue(RemoveUserCommandProperty); }
            set { SetValue(RemoveUserCommandProperty, value); }
        }

        public static readonly DependencyProperty RemoveUserCommandProperty =
            DependencyProperty.Register("RemoveUserCommand", typeof(ICommand), typeof(MembersListControl), new PropertyMetadata(null));

        public MembersListControl()
        {
            this.InitializeComponent();
        }
    }
}
