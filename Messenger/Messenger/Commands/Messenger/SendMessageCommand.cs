using Messenger.ViewModels;
using System;
using Messenger.Core.Models;
using System.Windows.Input;
using Messenger.Services;
using Serilog;
using Messenger.Core.Helpers;
using Messenger.Views.DialogBoxes;

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
            return true;
        }

        /// <summary>
        /// Sends a new message to the hub and saves it to database
        /// </summary>
        public async void Execute(object parameter)
        {
            bool executable = _viewModel.MessageToSend != null
                && !string.IsNullOrEmpty(_viewModel.MessageToSend.Content);

            if (!executable)
            {
                return;
            }

            try
            {
                Message message = _viewModel.MessageToSend;

                // Records created timestamp
                message.CreationTime = DateTime.Now;

                // Sender/Recipient data will be handled in ChatHubService
                bool success = await _hub.SendMessage(_viewModel.MessageToSend);

                if (success)
                {
                    // Resets the models in the view model
                    _viewModel.ReplyMessage = null;
                    _viewModel.SelectedFiles = null;
                    _viewModel.MessageToSend = new Message();
                }
                else
                {
                    await ResultConfirmationDialog
                        .Set(false, $"{message}")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while sending message: {e.Message}");
            }
        }
    }
}
