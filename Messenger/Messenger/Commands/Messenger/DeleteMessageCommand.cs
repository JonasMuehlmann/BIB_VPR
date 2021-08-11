using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class DeleteMessageCommand : ICommand
    {
        private ILogger _logger => GlobalLogger.Instance;

        public DeleteMessageCommand()
        {
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
               && parameter is MessageViewModel
               && App.StateProvider.CurrentUser != null
               && App.StateProvider.SelectedTeam != null;

            if (!executable)
            {
                return;
            }

            try
            {
                MessageViewModel vm = (MessageViewModel)parameter;
                TeamViewModel team = App.StateProvider.SelectedTeam;

                bool isSuccess = await MessengerService.DeleteMessage((uint)vm.Id, team.Id);

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
