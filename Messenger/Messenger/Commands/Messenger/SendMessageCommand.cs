using System;
using Messenger.Core.Models;
using System.Windows.Input;
using Serilog;
using Messenger.Core.Helpers;
using Messenger.Views.DialogBoxes;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;

namespace Messenger.Commands.Messenger
{
    public class SendMessageCommand : ICommand
    {
        private readonly ChatViewModel _viewModel;
        private ILogger _logger => GlobalLogger.Instance;

        public SendMessageCommand(ChatViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Checks if the message is valid to be forwarded to the hub
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
                && !string.IsNullOrEmpty(_viewModel.MessageToSend.Content)
                && App.StateProvider.CurrentUser != null
                && App.StateProvider.SelectedChannel != null;

            if (!executable)
            {
                return;
            }

            try
            {
                Message message = _viewModel.MessageToSend;
                TeamViewModel team = App.StateProvider.SelectedTeam;

                // Records created timestamp
                message.CreationTime = DateTime.Now;
                message.SenderId = App.StateProvider.CurrentUser.Id;
                message.RecipientId = App.StateProvider.SelectedChannel.ChannelId;

                bool success = await MessengerService.SendMessage(message, team.Id);
                //bool success = await MessengerService.SendMessage(_viewModel.MessageToSend, team.Id);

                if (success)
                {
                    // Resets the models in the view model
                    _viewModel.ReplyMessage = null;
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
