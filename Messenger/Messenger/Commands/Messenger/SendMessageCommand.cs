using Messenger.ViewModels;
using System;
using Messenger.Core.Models;
using System.Windows.Input;
using Messenger.Services;
using Serilog;
using Messenger.Core.Helpers;

namespace Messenger.Commands.Messenger
{
    public class SendMessageCommand : ICommand
    {
        private readonly ChatViewModel _viewModel;
        private readonly ChatHubService _hub;
        private ILogger _logger => GlobalLogger.Instance;

        public SendMessageCommand(ChatViewModel viewModel, ChatHubService hub)
        {
            _viewModel = viewModel;
            _hub = hub;
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Chekcs if the message is valid to be forwarded to the hub
        /// </summary>
        public bool CanExecute(object parameter)
        {
            bool canExecute = _viewModel.MessageToSend != null
                && !string.IsNullOrEmpty(_viewModel.MessageToSend.Content);

            return canExecute;
        }

        /// <summary>
        /// Sends a new message to the hub and saves it to database
        /// </summary>
        public async void Execute(object parameter)
        {
            try
            {
                // Records created timestamp
                _viewModel.MessageToSend.CreationTime = DateTime.Now;

                // User/Team data will be handled in ChatHubService
                await _hub.SendMessage(_viewModel.MessageToSend);

                // Reset the view model for MessageToSend
                _viewModel.ReplyMessage = null;
                _viewModel.SelectedFiles = null;
                _viewModel.MessageToSend = new Message();
            }
            catch (Exception e)
            {
                _logger.Information($"Error while sending message: {e.Message}");
            }
        }
    }
}
