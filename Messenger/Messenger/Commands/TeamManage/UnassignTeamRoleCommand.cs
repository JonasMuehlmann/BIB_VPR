using System;
using System.Windows.Input;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.DialogBoxes;
using Messenger.Views.DialogBoxes;

namespace Messenger.Commands.TeamManage
{
    public class UnassignTeamRoleCommand : ICommand
    {
        private ManageMemberRolesDialogViewModel _vm;

        public event EventHandler CanExecuteChanged;

        public UnassignTeamRoleCommand(ManageMemberRolesDialogViewModel vm)
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
                && parameter is TeamRoleViewModel;

            if (!executable) return;

            try
            {
                TeamViewModel currentTeam = App.StateProvider.SelectedTeam;
                TeamRoleViewModel selectedRole = (TeamRoleViewModel)parameter;
                MemberViewModel targetMember = _vm.Member;

                bool isSuccess = await MessengerService.UnassignUserRole(selectedRole.Title, targetMember.Id, currentTeam.Id);
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
