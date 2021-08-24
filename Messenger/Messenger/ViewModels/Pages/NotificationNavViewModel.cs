using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Messenger.Commands;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.ViewModels.Controls;

namespace Messenger.ViewModels.Pages
{
    public class NotificationNavViewModel : Observable
    {
        public InboxControlViewModel InboxControlViewModel { get; set; }

        public NotificationNavViewModel()
        {
            InboxControlViewModel = new InboxControlViewModel();
        }
    }
}
