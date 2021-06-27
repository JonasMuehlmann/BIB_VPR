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
    public class DeleteMessageCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            bool executable = parameter is uint;

            if (!executable)
            {
                return;
            }

            try
            {
            }
            catch (Exception e)
            {
                _logger.Information($"Error while deleting the message: {e.Message}");
            }
        }
    }
}
