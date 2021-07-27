using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.DialogBoxes;
using Messenger.Views.DialogBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

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
                Permissions permission = (Permissions)parameter;

                bool isSuccess = await MessengerService.RevokePermission(currentTeam.Id, selectedRole.Title, permission);
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
