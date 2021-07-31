using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;
using Messenger.Views.DialogBoxes;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Commands.TeamManage
{
    public class StartChatCommand : ICommand
    {
        private readonly ChatNavViewModel _viewModel;

        private CreateChatDialog _dialog;

        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public UserViewModel CurrentUser { get => App.StateProvider?.CurrentUser; }

        public StartChatCommand(ChatNavViewModel viewModel)
        {
            _viewModel = viewModel;
            _dialog = new CreateChatDialog()
            {
                OnSearch = SearchAndFilterUsers,
                GetSelectedUser = GetUserWithName
            };
        }

        /// <summary>
        /// Returns the search result from the database
        /// </summary>
        /// <param name="username">DisplayName of the user to search for</param>
        /// <returns>List of search result strings</returns>
        private async Task<IReadOnlyList<string>> SearchAndFilterUsers(string username)
        {
            return (await UserService.SearchUser(username))
                .TakeWhile((user) =>
                {
                    string[] data = user.Split('#');
                    return !(CurrentUser.Name == data[0]
                        && CurrentUser.NameId.ToString() == data[1]);
                }).ToList();
        }

        private async Task<User> GetUserWithName(string username, uint nameId)
        {
            return await UserService.GetUser(username, nameId);
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
            bool executable = _viewModel != null
                && CurrentUser != null;

            try
            {
                /** FEED CURRENT USER DATA **/
                _dialog.CurrentUser = CurrentUser;

                if (await _dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    /* EXIT IF THE CHAT EXISTS */
                    if (_dialog.SelectedUser == null
                        || _viewModel.Chats.Where(chat => chat.Partner.Id == _dialog.SelectedUser.Id).Count() > 0)
                    {
                        return;
                    }

                    uint? chatId = await MessengerService.StartChat(CurrentUser.Id, _dialog.SelectedUser.Id);

                    if (chatId != null)
                    {
                        await ResultConfirmationDialog
                                .Set(true, $"You have started a new chat with {_dialog.SelectedUser.DisplayName}.")
                                .ShowAsync();
                    }
                    else
                    {
                        await ResultConfirmationDialog
                                .Set(false, $"We could not create a new chat with {_dialog.SelectedUser.DisplayName}.")
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
