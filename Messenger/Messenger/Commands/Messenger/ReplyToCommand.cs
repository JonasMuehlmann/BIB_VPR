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

        /// <summary>
        /// Checks the validity of the message the user is replying to
        /// </summary>
        public bool CanExecute(object parameter)
        {
            Message message = parameter as Message;

            bool canExecute = message != null
                && message.Sender != null
                && message.SenderId != null;

            return canExecute;
        }

        /// <summary>
        /// Sets the model for ReplyMessage with the given message object
        /// </summary>
        public void Execute(object parameter)
        {
            Message message = parameter as Message;

            _viewModel.ReplyMessage = message;
            _viewModel.MessageToSend.ParentMessageId = message.Id;
        }
    }
}
