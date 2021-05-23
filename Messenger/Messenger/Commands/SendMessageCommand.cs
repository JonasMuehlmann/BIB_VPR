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
        private readonly SignalRHubViewModel _viewModel;
        private readonly SignalRService _signalRService;

        public SendMessageCommand(SignalRHubViewModel viewModel, SignalRService signalRService)
        {
            _viewModel = viewModel;
            _signalRService = signalRService;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                await _signalRService.SendMessage(new Message()
                {
                    Content = parameter.ToString(),
                    CreationTime = DateTime.Now,
                    SenderId = _viewModel.User.Id,
                    RecipientId = _viewModel.CurrentTeamId
                });

                _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"SignalR Exception: {e.Message}");
                _viewModel.ErrorMessage = "Unable to send message.";
            }
        }
    }
}
