using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.Pages;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class InviteUserCommand : ICommand
    {
        private readonly TeamManageViewModel _viewModel;
        private ILogger _logger = GlobalLogger.Instance;

        public InviteUserCommand(TeamManageViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is TeamManageViewModel
                && _viewModel != null
                && _viewModel.SelectedSearchedUser != null;

            if (!executable)
            {
                return;
            }

            try
            {
                TeamManageViewModel viewModel = (TeamManageViewModel)parameter;

                uint teamId = App.StateProvider.SelectedTeam.Id;
                string userId = viewModel.SelectedSearchedUser.Id;

                User user = await UserService.GetUser(userId);

                if (user == null)
                {
                    await ResultConfirmationDialog
                        .Set(false, $"No user was found with id: {userId}")
                        .ShowAsync();
                }

                bool isSuccess = await MessengerService.SendInvitation(userId, teamId);

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
