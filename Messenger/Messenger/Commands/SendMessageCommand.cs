using Messenger.Services;
using Messenger.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messenger.Core.Services;
using Messenger.Core.Models;
using System.Windows.Input;
using System.Diagnostics;

namespace Messenger.Commands
{
    public class SendMessageCommand : ICommand
    {
        private readonly ChatHubViewModel _viewModel;
        private readonly MessengerService _messengerService;

        public SendMessageCommand(ChatHubViewModel viewModel, MessengerService messengerService)
        {
            _viewModel = viewModel;
            _messengerService = messengerService;
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
                // creates new message based on the current view model
                Message message = new Message()
                {
                    Content = parameter.ToString(),
                    CreationTime = DateTime.Now,
                    SenderId = _viewModel.User.Id,
                    RecipientId = _viewModel.CurrentTeamId
                };

                await _messengerService.SendMessage(message);

                _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception e)
            {
                _viewModel.ErrorMessage = $"Unable to send message. {e.Message}";
            }
        }
    }
}
