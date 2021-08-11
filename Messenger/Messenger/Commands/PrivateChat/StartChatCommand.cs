using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Commands.TeamManage
{
    public class StartChatCommand : ICommand
    {
        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public StartChatCommand()
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Opens the dialog to start a new private chat
        /// </summary>
        public async void Execute(object parameter)
        {
            try
            {
                CreateChatDialog dialog = new CreateChatDialog();

                if (await dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    UserViewModel currentUser = App.StateProvider.CurrentUser;

                    uint? chatId = await MessengerService.StartChat(currentUser.Id, dialog.ViewModel.SelectedUser.Id);

                    if (chatId != null)
                    {
                        await ResultConfirmationDialog
                                .Set(true, $"You have started a new chat with {dialog.ViewModel.SelectedUser.DisplayName}.")
                                .ShowAsync();
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while starting a new private chat: {e.Message}");

                await ResultConfirmationDialog
                            .Set(false, $"We could not start a new private chat.")
                            .ShowAsync();
            }
        }
    }
}
