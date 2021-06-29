using Messenger.Core.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    public class ToggleReactionCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private ILogger _logger => GlobalLogger.Instance;

        public ToggleReactionCommand(ChatHubService hub)
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
            bool executable = parameter != null
                && parameter is ToggleReactionArg;

            if (!executable)
            {
                return;
            }

            try
            {
                var arg = (ToggleReactionArg)parameter;
                MessageViewModel message = arg.Message;
                ReactionType type = arg.Type;

                bool isSuccess;
                if (message.HasReacted)
                {
                    if (message.MyReaction != type)
                    {
                        var removed = await _hub.RemoveReaction((uint)message.Id, message.MyReaction);
                        var made = await _hub.MakeReaction((uint)message.Id, type);

                        isSuccess = removed && made;
                    }
                    else
                    {
                        isSuccess = await _hub.MakeReaction((uint)message.Id, message.MyReaction);
                    }
                }
                else
                {
                    isSuccess = await _hub.MakeReaction((uint)message.Id, type);
                }

                if (!isSuccess)
                {
                    await ResultConfirmationDialog
                        .Set(false, "We could not update the reaction")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while updating the reaction: {e.Message}");
            }
        }
    }
}
