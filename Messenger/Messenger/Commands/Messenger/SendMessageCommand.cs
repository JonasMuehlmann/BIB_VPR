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

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Sends a new message to the hub and saves it to database
        /// </summary>
        /// <param name="parameter">Content of a message</param>
        public async void Execute(object parameter)
        {
            try
            {
                uint? parentMessageId = null;
                string content = parameter.ToString();

                // Is message a reply to an another message
                if (_viewModel.ReplyMessage != null)
                {
                    parentMessageId = _viewModel.ReplyMessage.Id;
                }

                // Records input, created timestamp and parent message id
                var message = new Message()
                {
                    Content = content,
                    CreationTime = DateTime.Now,
                    ParentMessageId = parentMessageId
                };

                // User/Team data will be handled in ChatHubService
                await _hub.SendMessage(message);

                // Reset the view model for MessageToSend
                _viewModel.MessageToSend = null;
            }
            catch (Exception e)
            {
                _logger.Information($"Error while sending message: {e.Message}");
            }
        }
    }
}
