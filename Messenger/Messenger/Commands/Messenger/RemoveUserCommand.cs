using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels;
using Messenger.Views.DialogBoxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class RemoveUserCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private readonly TeamManageViewModel _viewModel;

        public RemoveUserCommand(TeamManageViewModel viewModel, ChatHubService hub)
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
            try
            {
                string userId = parameter.ToString();

                bool isSuccess = await _hub.RemoveUser(userId, _hub.SelectedTeam.Id);

                if (isSuccess)
                {
                    Member memberToRemove = _viewModel.Members.Single(member => member.Id == userId);

                    _viewModel.Members.Remove(memberToRemove);
                }
                else
                {
                    await ResultConfirmationDialog
                            .Set(false, "We could not remove the user, try again.")
                            .ShowAsync();
                }
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, "We could not remove the user, try again.")
                    .ShowAsync();
            }
        }
    }
}
