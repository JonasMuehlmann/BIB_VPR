using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    /// <summary>
    /// Invite the user to the current team
    /// </summary>
    public class InviteUserCommand : ICommand
    {
        private ILogger _logger = GlobalLogger.Instance;

        public InviteUserCommand()
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
                && parameter is User;

            if (!executable)
            {
                return;
            }

            try
            {
                User user = (User)parameter;
                uint teamId = App.StateProvider.SelectedTeam.Id;

                bool isSuccess = await MessengerService.SendInvitation(user.Id, teamId);

                if (isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(true, $"Invited user \"{user.DisplayName}\" to the team")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while inviting a user: {e.Message}");

                await ResultConfirmationDialog
                            .Set(false, "We could not invite the user, try again.")
                            .ShowAsync();
            }
        }
    }
}
