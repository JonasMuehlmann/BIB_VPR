using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Commands.TeamManage
{
    public class CreateChannelCommand : ICommand
    {
        private ILogger _log = GlobalLogger.Instance;

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
                CreateChannelDialog _dialog = new CreateChannelDialog();
                TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

                if (await _dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    uint? teamId = await MessengerService.CreateChannel(_dialog.ChannelName, selectedTeam.Id);

                    if (teamId != null)
                    {
                        await ResultConfirmationDialog
                            .Set(true, $"Created a new channel {_dialog.ChannelName}")
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
