using Messenger.Core.Models;
using Messenger.ViewModels.DataViewModels;
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

namespace Messenger.Controls.ChatControls
{
    public sealed partial class MessageControl : UserControl
    {
        public MessageViewModel Message
        {
            get { return (MessageViewModel)GetValue(MessageProperty); }
            set { SetValue(MessageProperty, value); }
        }

        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register("Message", typeof(MessageViewModel), typeof(MessageControl), new PropertyMetadata(null));

        public ICommand ReplyToCommand
        {
            get { return (ICommand)GetValue(ReplyToCommandProperty); }
            set { SetValue(ReplyToCommandProperty, value); }
        }

        public static readonly DependencyProperty ReplyToCommandProperty =
            DependencyProperty.Register("ReplyToCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public ICommand DownloadAttachmentCommand
        {
            get { return (ICommand)GetValue(DownloadAttachmentCommandProperty); }
            set { SetValue(DownloadAttachmentCommandProperty, value); }
        }

        public static readonly DependencyProperty DownloadAttachmentCommandProperty =
            DependencyProperty.Register("DownloadAttachmentCommand", typeof(ICommand), typeof(MessageControl), new PropertyMetadata(null));

        public MessageControl()
        {
            InitializeComponent();
        }

        private void AppBarButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}
