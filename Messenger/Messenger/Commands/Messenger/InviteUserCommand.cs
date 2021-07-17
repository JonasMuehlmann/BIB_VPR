using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class InviteUserCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private ILogger _logger = GlobalLogger.Instance;

        public InviteUserCommand(ChatHubService hub)
        {
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

                    bool isSuccess = await _hub.InviteUser(invitation);
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while inviting a user: {e.Message}");
            }
        }
    }
}
