using System;
using System.Linq;
using System.Windows.Input;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.DialogBoxes;
using Messenger.Views.DialogBoxes;

namespace Messenger.Commands.TeamManage
{
    public class RevokePermissionCommand : ICommand
    {
        private ManageRolesDialogViewModel _vm;

        public event EventHandler CanExecuteChanged;

        public RevokePermissionCommand(ManageRolesDialogViewModel vm)
        {
            _vm = vm;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is Permissions;

            if (!executable) return;

            try
            {
                TeamViewModel currentTeam = App.StateProvider.SelectedTeam;
                TeamRoleViewModel selectedRole = _vm.SelectedTeamRole;
                TeamRoleViewModel pendingChange = _vm.PendingChange;
                Permissions permission = (Permissions)parameter;

                if (selectedRole.Permissions.Any(p => p == permission))
                {
                    bool isSuccess = await MessengerService.RevokePermission(currentTeam.Id, selectedRole.Title, permission);
                    return;
                }

                if (pendingChange.Permissions.Any(p => p == permission))
                {
                    pendingChange.Permissions.Remove(permission);
                    return;
                }
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, $"Error while revoking a permission from a role: {e.Message}")
                    .ShowAsync();
            }
        }
    }
}
