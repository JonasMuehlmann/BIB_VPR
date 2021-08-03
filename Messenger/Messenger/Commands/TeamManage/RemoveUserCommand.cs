using System;
using System.Windows.Input;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;

namespace Messenger.Commands.TeamManage
{
    public class RemoveUserCommand : ICommand
    {
        public RemoveUserCommand()
        {
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && !string.IsNullOrEmpty(parameter.ToString())
                && App.StateProvider.SelectedTeam != null;

            if (!executable) return;

            try
            {
                OperationConfirmationDialog dialog = OperationConfirmationDialog.Set("You're about to remove a member");

                await dialog.ShowAsync();

                if (!dialog.Response) return;

                TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

                string userId = parameter.ToString();

                User user = await UserService.GetUser(userId);

                if (user == null)
                {
                    await ResultConfirmationDialog
                        .Set(false, $"No user was found with id: {userId}")
                        .ShowAsync();
                }

                bool isSuccess = await MessengerService.RemoveMember(userId, selectedTeam.Id);

                if (isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(true, $"Invited user \"{user.DisplayName}\" to the team")
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
