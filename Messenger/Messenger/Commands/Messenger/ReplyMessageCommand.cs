using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;
using System;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    /// <summary>
    /// Send a reply to selected message
    /// </summary>
    public class ReplyMessageCommand : ICommand
    {
        private readonly ChatViewModel _viewModel;

        public event EventHandler CanExecuteChanged;

        public ReplyMessageCommand(ChatViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        /// <summary>
        /// Checks the validity of the message the user is replying to
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Sets the model for ReplyMessage with the given message object
        /// </summary>
        public void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is MessageViewModel;

            if (!executable)
            {
                return;
            }

            MessageViewModel message = parameter as MessageViewModel;

            _viewModel.ReplyMessage = message;
            _viewModel.MessageToSend.ParentMessageId = message.Id;
        }
    }
}
