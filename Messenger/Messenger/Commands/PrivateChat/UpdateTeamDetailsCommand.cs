using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Commands.PrivateChat
{
    public class UpdateTeamDetailsCommand : ICommand
    {
        private ILogger _log => GlobalLogger.Instance;

        private ChangeTeamDialog _dialog;

        public event EventHandler CanExecuteChanged;

        public UpdateTeamDetailsCommand()
        {
            _dialog = new ChangeTeamDialog();
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

                _dialog.TeamName = selectedTeam.TeamName;
                _dialog.TeamDescription = selectedTeam.Description;

                bool isSuccess = true;

                /** CREATE ON CONFIRM & CHANGES **/
                if (await _dialog.ShowAsync() == ContentDialogResult.Primary
                    && (_dialog.TeamName != selectedTeam.TeamName
                        || _dialog.TeamDescription != selectedTeam.Description))
                {
                    isSuccess &= await MessengerService.UpdateTeamName(_dialog.TeamName, selectedTeam.Id);
                    isSuccess &= await MessengerService.UpdateTeamDescription(_dialog.TeamDescription, selectedTeam.Id);
                }

                if (!isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(false, "We could not update the team details.")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _log.Information($"Error while updating team details: {e.Message}");
            }
        }
    }
}
