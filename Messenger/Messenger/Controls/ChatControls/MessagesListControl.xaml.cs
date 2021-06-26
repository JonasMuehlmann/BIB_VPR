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

        public MessagesListControl()
        {
            InitializeComponent();
        }
    }
}
