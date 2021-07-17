using Messenger.Services;
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

        public RemoveUserCommand(ChatHubService hub)
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
            try
            {
                string userId = parameter.ToString();

                await _hub.RemoveUser(userId, _hub.SelectedTeam.Id);
            }
            catch (Exception e)
            {
                
            }
        }
    }
}
