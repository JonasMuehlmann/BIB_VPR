using Messenger.Core.Helpers;
using Messenger.Services;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.Commands.UserData
{
    public class EditBioCommand : ICommand
    {
        private readonly UserDataService _service;

        private ILogger _logger => GlobalLogger.Instance;

        public EditBioCommand(UserDataService service)
        {
            _service = service;
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
                && !string.IsNullOrEmpty(parameter.ToString());

            if (!executable)
            {
                return;
            }

            try
            {
                bool isSuccess = await _service.UpdateUserBio(parameter.ToString());

                if (isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(true, "Successfully updated the bio")
                        .ShowAsync();
                }
                else
                {
                    await ResultConfirmationDialog
                        .Set(false, "We could not updated the bio")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while editing user bio: {e.Message}");
            }
        }
    }
}
