using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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

namespace Messenger.Controls.ChatControls
{
    public sealed partial class ReplyControl : UserControl
    {
        public MessageViewModel Reply
        {
            get { return (MessageViewModel)GetValue(ReplyProperty); }
            set { SetValue(ReplyProperty, value); }
        }

        public static readonly DependencyProperty ReplyProperty =
            DependencyProperty.Register("Reply", typeof(MessageViewModel), typeof(ReplyControl), new PropertyMetadata(new MessageViewModel()));

        public ReplyControl()
        {
            InitializeComponent();
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
