using System;
using System.Windows.Input;
using Serilog;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;

namespace Messenger.Commands.UserData
{
    public class UpdateUserNameCommand : ICommand
    {
        private ILogger _logger => GlobalLogger.Instance;

        public UpdateUserNameCommand()
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
                && parameter is string
                && !string.IsNullOrEmpty(parameter.ToString())
                && App.StateProvider.CurrentUser != null;

            if (!executable)
            {
                return;
            }

            try
            {
                UserViewModel currentUser = App.StateProvider.CurrentUser;
                string newUsername = parameter.ToString();

                bool isSuccess = await MessengerService.UpdateUserUsername(currentUser.Id, newUsername);

                if (isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(true, "Successfully updated the username")
                        .ShowAsync();
                }
                else
                {
                    await ResultConfirmationDialog
                        .Set(false, "We could not updated the username")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while editing username: {e.Message}");
            }
        }
    }
}
