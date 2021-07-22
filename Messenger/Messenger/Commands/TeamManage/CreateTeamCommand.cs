using Messenger.Core.Helpers;
using Messenger.Services;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Commands.TeamManage
{
    public class CreateTeamCommand : ICommand
    {
        private readonly ChatHubService _hub;

        private ILogger _log = GlobalLogger.Instance;

        public CreateTeamCommand(ChatHubService hub)
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
            bool executable = App.StateProvider.CurrentUser != null;

            if (!executable) return;

            try
            {
                var dialog = new CreateTeamDialog();
                ContentDialogResult result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    await _hub.CreateTeam(dialog.TeamName, dialog.TeamDescription);

                    await ResultConfirmationDialog
                        .Set(true, $"You created a new team {dialog.TeamName}")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _log.Information($"Error while creating a new team: {e.Message}");
            }
        }
    }
}
