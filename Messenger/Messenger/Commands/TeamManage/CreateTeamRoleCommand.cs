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
    public class CreateTeamRoleCommand : ICommand
    {
        private ManageRolesDialogViewModel _vm;

        public CreateTeamRoleCommand(ManageRolesDialogViewModel vm)
        {
            _vm = vm;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is string;

            try
            {
                TeamViewModel currentTeam = App.StateProvider.SelectedTeam;
                string roleTitle = parameter.ToString();

                bool isSuccess = await MessengerService.CreateTeamRole(roleTitle, currentTeam.Id);

                if (isSuccess)
                {
                    _vm.NewRoleTitle = string.Empty;
                }
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, $"Error while creating a new team role: {e.Message}")
                    .ShowAsync();
            }
        }
    }
}
