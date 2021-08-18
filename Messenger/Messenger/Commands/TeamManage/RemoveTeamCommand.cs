using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class RemoveTeamCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is uint;

            if (!executable) return;

            try
            {
                OperationConfirmationDialog dialog = OperationConfirmationDialog.Set("You're about to remove a team");

                await dialog.ShowAsync();

                if (!dialog.Response) return;

                TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

                uint teamId = (uint)parameter;

                TeamViewModel team = CacheQuery.Get<TeamViewModel>(teamId);

                if (team == null)
                {
                    await ResultConfirmationDialog
                        .Set(false, $"No team was found with id: {teamId}")
                        .ShowAsync();
                }

                Team deleted = await MessengerService.DeleteTeam(team.Id);

                if (deleted != null)
                {
                    await ResultConfirmationDialog
                        .Set(true, $"Removed team {team.TeamName}#{team.Id}")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, $"We could not remove the user, try again: {e.Message}")
                    .ShowAsync();
            }
        }
    }
}
