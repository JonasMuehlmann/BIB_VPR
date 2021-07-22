using Messenger.Core.Helpers;
using Messenger.Services;
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
        private readonly ChatHubService _hub;

        private ILogger _log => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public UpdateTeamDetailsCommand(ChatHubService hub)
        {
            _hub = hub;
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

                // Opens the dialog box for the input
                var dialog = new ChangeTeamDialog()
                {
                    TeamName = selectedTeam.TeamName,
                    TeamDescription = selectedTeam.Description
                };

                // Create team on confirm
                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    await _hub.UpdateTeam(dialog.TeamName, dialog.TeamDescription);
                }
            }
            catch (Exception e)
            {
                _log.Information($"Error while updating team details: {e.Message}");
            }
        }
    }
}
