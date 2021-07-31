using System;
using System.Windows.Input;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;

namespace Messenger.Commands.TeamManage
{
    public class UpdateTeamRoleCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is TeamRoleViewModel;

            if (!executable) return;

            try
            {
                TeamViewModel currentTeam = App.StateProvider.SelectedTeam;
                TeamRoleViewModel teamRole = (TeamRoleViewModel)parameter;

                bool isSuccess = await MessengerService.UpdateTeamRole(teamRole.Id, teamRole.Title, teamRole.Color.ToHex());

                if (teamRole.PendingPermissions.Count > 0)
                {
                    foreach (Permissions permission in teamRole.PendingPermissions)
                    {
                        isSuccess &= await MessengerService.GrantPermission(teamRole.TeamId, teamRole.Title, permission);
                    }
                }
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, $"Error while updating a new team role: {e.Message}")
                    .ShowAsync();
            }
        }
    }
}
