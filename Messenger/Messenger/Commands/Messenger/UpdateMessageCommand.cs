using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class UpdateMessageCommand : ICommand
    {
        private ILogger _logger => GlobalLogger.Instance;

        public UpdateMessageCommand()
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

                bool isSuccess = await MessengerService.UpdateMessage((uint)vm.Id, vm.Content, team.Id);

                if (!isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(false, "We could not edit the message.")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while editing the message: {e.Message}");
            }
        }
    }
}
