using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class AssignTeamRoleCommand : ICommand
    {
        private TeamManageViewModel _vm;

        public event EventHandler CanExecuteChanged;

        public AssignTeamRoleCommand(TeamManageViewModel vm)
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
                && parameter is TeamRole;

            if (!executable) return;

            try
            {
                TeamViewModel currentTeam = App.StateProvider.SelectedTeam;
                TeamRoleViewModel selectedRole = (TeamRoleViewModel)parameter;
                MemberViewModel targetMember = _vm.SelectedMember;

                bool isSuccess = await MessengerService.AssignUserRole(selectedRole.Title, targetMember.Id, currentTeam.Id);
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
