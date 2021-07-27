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
    public class GrantPermissionCommand : ICommand
    {
        private ManageRolesDialogViewModel _vm;

        public event EventHandler CanExecuteChanged;

        public GrantPermissionCommand(ManageRolesDialogViewModel vm)
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
                && parameter is Permissions
                && _vm != null
                && _vm.SelectedTeamRole != null;

            if (!executable) return;

            try
            {
                TeamViewModel currentTeam = App.StateProvider.SelectedTeam;
                TeamRoleViewModel selectedRole = _vm.SelectedTeamRole;
                Permissions permission = (Permissions)parameter;

                bool isSuccess = await MessengerService.GrantPermission(currentTeam.Id, selectedRole.Title, permission);
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, $"Error while granting a permission to a role: {e.Message}")
                    .ShowAsync();
            }
        }
    }
}
