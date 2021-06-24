using Messenger.Commands.Messenger;
using Messenger.Core.Models;
using Messenger.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Messenger.Controls.ChatControls
{
    public sealed partial class SendMessageControl : UserControl
    {
        public IReadOnlyList<StorageFile> Attachments
        {
            get { return (IReadOnlyList<StorageFile>)GetValue(AttachmentsProperty); }
            set { SetValue(AttachmentsProperty, value); }
        }

        public static readonly DependencyProperty AttachmentsProperty =
            DependencyProperty.Register("Attachments", typeof(IReadOnlyList<StorageFile>), typeof(SendMessageControl), new PropertyMetadata(null));

        public string MessageContent
        {
            get { return (string)GetValue(MessageContentProperty); }
            set { SetValue(MessageContentProperty, value); }
        }

        public static readonly DependencyProperty MessageContentProperty =
            DependencyProperty.Register("MessageContent", typeof(string), typeof(SendMessageControl), new PropertyMetadata(string.Empty));

        public ICommand SendMessageCommand
        {
            get { return (ICommand)GetValue(SendMessageCommandProperty); }
            set { SetValue(SendMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty SendMessageCommandProperty =
            DependencyProperty.Register("SendMessageCommand", typeof(ICommand), typeof(SendMessageControl), new PropertyMetadata(null));

        public ICommand OpenFilesCommand
        {
            get { return (ICommand)GetValue(OpenFilesCommandProperty); }
            set { SetValue(OpenFilesCommandProperty, value); }
        }

        public static readonly DependencyProperty OpenFilesCommandProperty =
            DependencyProperty.Register("OpenFilesCommand", typeof(ICommand), typeof(SendMessageControl), new PropertyMetadata(null));

        public Message ReplyMessage
        {
            get { return (Message)GetValue(ReplyMessageProperty); }
            set { SetValue(ReplyMessageProperty, value); }
        }

        public static readonly DependencyProperty ReplyMessageProperty =
            DependencyProperty.Register("ReplyMessage", typeof(Message), typeof(SendMessageControl), new PropertyMetadata(null));

        public SendMessageControl()
        {
            InitializeComponent();

        }
    }
}
