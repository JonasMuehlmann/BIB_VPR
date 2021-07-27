using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Commands.TeamManage
{
    public class CreateTeamCommand : ICommand
    {
        private ILogger _log = GlobalLogger.Instance;

        private CreateTeamDialog _dialog;

        public CreateTeamCommand()
        {
            _dialog = new CreateTeamDialog();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = App.StateProvider.CurrentUser != null;

            if (!executable) return;

            try
            {
                if (await _dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    uint? teamId = await MessengerService.CreateTeam(App.StateProvider.CurrentUser.Id, _dialog.TeamName, _dialog.TeamDescription);

                    if (teamId != null)
                    {
                        await ResultConfirmationDialog
                            .Set(true, $"You created a new team {_dialog.TeamName}")
                            .ShowAsync();
                    }
                }
            }
            catch (Exception e)
            {
                _log.Information($"Error while creating a new team: {e.Message}");
            }
        }
    }
}
