using Messenger.Models;
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
        public ObservableCollection<Member> Members
        {
            get { return (ObservableCollection<Member>)GetValue(MembersProperty); }
            set { SetValue(MembersProperty, value); }
        }

        public static readonly DependencyProperty MembersProperty =
            DependencyProperty.Register("Members", typeof(ObservableCollection<Member>), typeof(MembersListControl), new PropertyMetadata(new ObservableCollection<Member>()));

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
