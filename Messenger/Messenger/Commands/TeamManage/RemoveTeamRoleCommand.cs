using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class RemoveTeamRoleCommand : ICommand
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
                TeamRoleViewModel teamRole = (TeamRoleViewModel)parameter;
                TeamViewModel currentTeam = App.StateProvider.SelectedTeam;

                bool isSuccess = await MessengerService.DeleteTeamRole(teamRole.Title, currentTeam.Id);
            }
            catch (Exception e)
            {

            }
        }
    }
}
