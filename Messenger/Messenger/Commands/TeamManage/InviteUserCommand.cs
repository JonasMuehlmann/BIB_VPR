using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class InviteUserCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private readonly TeamManageViewModel _viewModel;
        private ILogger _logger = GlobalLogger.Instance;

        public InviteUserCommand(TeamManageViewModel viewModel, ChatHubService hub)
        {
            _viewModel = viewModel;
            _hub = hub;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is TeamManageViewModel;

            if (!executable)
            {
                return;
            }

            try
            {
                TeamManageViewModel viewModel = parameter as TeamManageViewModel;

                uint teamId = App.StateProvider.SelectedTeam.Id;
                string userId = viewModel.SelectedSearchedUser.Id;

                MemberViewModel member = await _hub.InviteUser(userId, teamId);

                if (member == null)
                {
                    await ResultConfirmationDialog
                        .Set(false, "We could not invite the user, try again.")
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
