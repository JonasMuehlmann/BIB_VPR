using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
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
            bool executable = parameter != null;

            if (!executable)
            {
                return;
            }

            try
            {
                if (parameter is TeamManageViewModel)
                {
                    Invitation invitation = new Invitation()
                    {
                        TeamId = (parameter as TeamManageViewModel).CurrentTeam.Id,
                        UserId = (parameter as TeamManageViewModel).SelectedSearchedUser.Id
                    };

                    Member member = await _hub.InviteUser(invitation);

                    if (member != null)
                    {
                        _viewModel.Members.Add(member);
                    }
                    else
                    {
                        await ResultConfirmationDialog
                            .Set(false, "We could not invite the user, try again.")
                            .ShowAsync();
                    }
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
