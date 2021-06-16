using System;
using System.Collections.Generic;
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

namespace Messenger.Controls.Navigation
{
    public sealed partial class SideNavigation : UserControl
    {
        public ICommand NavigateToTeams
        {
            get { return (ICommand)GetValue(NavigateToTeamsProperty); }
            set { SetValue(NavigateToTeamsProperty, value); }
        }

        public static readonly DependencyProperty NavigateToTeamsProperty =
            DependencyProperty.Register("NavigateToTeams", typeof(ICommand), typeof(SideNavigation), new PropertyMetadata(null));

        public ICommand NavigateToChats
        {
            get { return (ICommand)GetValue(NavigateToChatsProperty); }
            set { SetValue(NavigateToChatsProperty, value); }
        }

        public static readonly DependencyProperty NavigateToChatsProperty =
            DependencyProperty.Register("NavigateToChats", typeof(ICommand), typeof(SideNavigation), new PropertyMetadata(null));

        public ICommand NavigateToNotifications
        {
            get { return (ICommand)GetValue(NavigateToNotificationsProperty); }
            set { SetValue(NavigateToNotificationsProperty, value); }
        }

        public static readonly DependencyProperty NavigateToNotificationsProperty =
            DependencyProperty.Register("NavigateToNotifications", typeof(ICommand), typeof(SideNavigation), new PropertyMetadata(null));

        public string CurrentPageName
        {
            get { return (string)GetValue(CurrentPageNameProperty); }
            set
            {
                SetValue(CurrentPageNameProperty, value);

            }
        }

        public bool IsTeamsActive => CurrentPageName == "TeamNavPage";

        public bool IsChatsActive => CurrentPageName == "ChatNavPage";

        public bool IsNotificationsActive => CurrentPageName == "NotificationNavPage";

        public static readonly DependencyProperty CurrentPageNameProperty =
            DependencyProperty.Register("CurrentPageName", typeof(string), typeof(SideNavigation), new PropertyMetadata(string.Empty));

        public SideNavigation()
        {
            InitializeComponent();
        }
    }
}
