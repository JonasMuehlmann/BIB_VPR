using Messenger.Core.Helpers;
using Messenger.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class EditMessageCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private ILogger _logger => GlobalLogger.Instance;

        public EditMessageCommand(ChatHubService hub)
        {
            _hub = hub;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
