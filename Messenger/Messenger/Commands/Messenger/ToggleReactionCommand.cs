using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Models;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.Messenger
{
    /// <summary>
    /// Make reaction on the selected message, remove if already made
    /// </summary>
    public class ToggleReactionCommand : ICommand
    {
        private ILogger _logger => GlobalLogger.Instance;

        public ToggleReactionCommand()
        {
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
                ToggleReactionArg arg = (ToggleReactionArg)parameter;
                MessageViewModel message = arg.Message;
                ReactionType type = arg.Type;

                TeamViewModel team = App.StateProvider.SelectedTeam;
                UserViewModel currentUser = App.StateProvider.CurrentUser;

                bool isSuccess;

                /** IF USER HAS ALREADY MADE A REACTION **/
                if (message.HasReacted)
                {
                    /* IF TYPE IS DIFFERENT, REMOVE THE OLD ONE AND MAKE THE NEW ONE */
                    if (message.MyReaction != type)
                    {
                        Reaction removed = await MessengerService.DeleteMessageReaction((uint)message.Id, currentUser.Id, team.Id, message.MyReaction.ToString());

                        Reaction created = await MessengerService.CreateMessageReaction((uint)message.Id, currentUser.Id, team.Id, type.ToString());

                        isSuccess = removed != null && created != null;
                    }
                    /* ELSE REMOVE ONLY */
                    else
                    {
                        Reaction removed = await MessengerService.DeleteMessageReaction((uint)message.Id, currentUser.Id, team.Id, message.MyReaction.ToString());

                        isSuccess = removed != null;
                    }
                }
                /** ELSE MAKE ONLY **/
                else
                {
                    Reaction created = await MessengerService.CreateMessageReaction((uint)message.Id, currentUser.Id, team.Id, type.ToString());

                    isSuccess = created != null;
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
