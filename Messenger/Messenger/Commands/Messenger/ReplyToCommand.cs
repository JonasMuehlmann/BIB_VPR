using Messenger.Core.Models;
using Messenger.ViewModels;
using System;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class ReplyToCommand : ICommand
    {
        private readonly ChatViewModel _viewModel;

        public event EventHandler CanExecuteChanged;

        public ReplyToCommand(ChatViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            Message message = parameter as Message;

            if (message != null)
            {
                _viewModel.ReplyMessage = message;
                _viewModel.MessageToSend.ParentMessageId = message.Id;
            }
        }
    }
}
