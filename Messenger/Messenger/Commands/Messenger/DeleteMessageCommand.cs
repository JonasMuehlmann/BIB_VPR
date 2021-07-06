using Messenger.Core.Helpers;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class DeleteMessageCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private ILogger _logger => GlobalLogger.Instance;

        public DeleteMessageCommand(ChatHubService hub)
        {
            _hub = hub;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
               && parameter is MessageViewModel;

            if (!executable)
            {
                return;
            }

            try
            {
                MessageViewModel vm = (MessageViewModel)parameter;

                bool isSuccess = await _hub.DeleteMessage((uint)vm.Id);

                if (!isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(false, $"We could not delete the message. (ID: {(uint)vm.Id})")
                        .ShowAsync();
                }
                else
                {
                    await ResultConfirmationDialog
                        .Set(true, $"The message was deleted. (ID: {(uint)vm.Id})")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while deleting the message: {e.Message}");
            }
        }
    }
}
