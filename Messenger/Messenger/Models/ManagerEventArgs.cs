using Messenger.ViewModels.DataViewModels;
using System;
using System.Collections.Generic;

namespace Messenger.Models
{
    public class ManagerEventArgs : EventArgs
    {
        public IEnumerable<MessageViewModel> Messages { get; set; }

        public TeamViewModel Team { get; set; }

        public ChannelViewModel Channel { get; set; }
    }
}
