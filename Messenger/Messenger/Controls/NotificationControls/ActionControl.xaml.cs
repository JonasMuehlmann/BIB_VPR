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

namespace Messenger.Controls.NotificationControls
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
