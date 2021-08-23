using Messenger.Commands.Messenger;
using Messenger.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.ViewModels.Controls
{
    public class SendMessageControlViewModel
    {
        public ChatViewModel ParentViewModel { get; set; }

        public ICommand AttachFileCommand { get => new AttachFileCommand(ParentViewModel); }

        public ICommand SendMessageCommand { get => new SendMessageCommand(ParentViewModel); }

        public SendMessageControlViewModel(ChatViewModel parentViewModel)
        {
            ParentViewModel = parentViewModel;
        }
    }
}
