using Messenger.Services;
using Messenger.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Messenger.Core.Models;
using System.Windows.Input;

namespace Messenger.Commands
{
    public class SendChatRoomMessageCommand : ICommand
    {
        private readonly ChatRoomViewModel _viewModel;
        private readonly SignalRChatService _chatService;

        public SendChatRoomMessageCommand(ChatRoomViewModel viewModel, SignalRChatService chatService)
        {
            _viewModel = viewModel;
            _chatService = chatService;
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
                await _chatService.SendMessage(new Message()
                {
                    Content = parameter.ToString(),
                    CreationTime = DateTime.Now,
                    SenderId = _viewModel.User.Id
                });

                _viewModel.ErrorMessage = string.Empty;
            }
            catch (Exception)
            {
                _viewModel.ErrorMessage = "Unable to send color message.";
            }
        }
    }
}
