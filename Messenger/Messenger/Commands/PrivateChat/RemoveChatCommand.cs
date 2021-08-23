using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using System;
using System.Windows.Input;

namespace Messenger.Commands.PrivateChat
{
    public class RemoveChatCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public async void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is uint;

            if (!executable) return;

            try
            {
                OperationConfirmationDialog dialog = OperationConfirmationDialog.Set("You're about to end the chat");

                await dialog.ShowAsync();

                if (!dialog.Response) return;

                uint chatId = (uint)parameter;

                PrivateChatViewModel chat = CacheQuery.Get<PrivateChatViewModel>(chatId);

                if (chat == null)
                {
                    await ResultConfirmationDialog
                        .Set(false, $"No team was found with id: {chatId}")
                        .ShowAsync();
                }

                Team deleted = await MessengerService.DeleteTeam(chat.Id);

                if (deleted != null)
                {
                    await ResultConfirmationDialog
                        .Set(true, $"Removed team {deleted.Name}#{deleted.Id}")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, $"We could not remove the chat, try again: {e.Message}")
                    .ShowAsync();
            }
        }
    }
}
