using Messenger.Core.Models;
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

namespace Messenger.Controls.ChatControls
{
    public sealed partial class MessagesListControl : UserControl
    {
        public ObservableCollection<MessageViewModel> Messages
        {
            get { return (ObservableCollection<MessageViewModel>)GetValue(MessagesProperty); }
            set { SetValue(MessagesProperty, value); }
        }

        public static readonly DependencyProperty MessagesProperty =
            DependencyProperty.Register("Messages", typeof(ObservableCollection<MessageViewModel>), typeof(MessagesListControl), new PropertyMetadata(new ObservableCollection<MessageViewModel>()));

        public ICommand ReplyToCommand
        {
            get { return (ICommand)GetValue(ReplyToCommandProperty); }
            set { SetValue(ReplyToCommandProperty, value); }
        }

        public static readonly DependencyProperty ReplyToCommandProperty =
            DependencyProperty.Register("ReplyToCommand", typeof(ICommand), typeof(MessagesListControl), new PropertyMetadata(null));

        public ICommand EditMessageCommand
        {
            get { return (ICommand)GetValue(EditMessageCommandProperty); }
            set { SetValue(EditMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty EditMessageCommandProperty =
            DependencyProperty.Register("EditMessageCommand", typeof(ICommand), typeof(MessagesListControl), new PropertyMetadata(null));



        public ICommand DeleteMessageCommand
        {
            get { return (ICommand)GetValue(DeleteMessageCommandProperty); }
            set { SetValue(DeleteMessageCommandProperty, value); }
        }

        public static readonly DependencyProperty DeleteMessageCommandProperty =
            DependencyProperty.Register("DeleteMessageCommand", typeof(ICommand), typeof(MessagesListControl), new PropertyMetadata(null));



        public MessagesListControl()
        {
            InitializeComponent();
        }
    }
}
