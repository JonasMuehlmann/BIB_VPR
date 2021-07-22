using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class OpenManageRolesCommand : ICommand
    {
        private ILogger _log => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            try
            {
                await ManageRolesDialog.Open();
            }
            catch (Exception e)
            {
                _log.Information($"Error while opening ManageRolesDialog: {e.Message}");
            }
        }
    }
}
