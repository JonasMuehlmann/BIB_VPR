using Messenger.Core.Helpers;
using Messenger.Core.Models;
using Messenger.Services;
using Messenger.ViewModels;
using Messenger.Views.DialogBoxes;
using Serilog;
using Serilog.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml.Controls;

namespace Messenger.Commands.PrivateChat
{
    public class StartChatCommand : ICommand
    {
        private readonly ChatHubService _hub;
        private readonly ChatNavViewModel _viewModel;

        private ILogger _logger => GlobalLogger.Instance;
        private CreateChatDialog _dialog;

        public event EventHandler CanExecuteChanged;

        public StartChatCommand(ChatNavViewModel viewModel, ChatHubService hub)
        {
            _viewModel = viewModel;
            _hub = hub;
            _dialog = new CreateChatDialog()
            {
                OnSearch = SearchUsers,
                GetSelectedUser = GetUserWithName
            };
        }

        /// <summary>
        /// Returns the search result from the database
        /// </summary>
        /// <param name="username">DisplayName of the user to search for</param>
        /// <returns>List of search result strings</returns>
        private async Task<IReadOnlyList<string>> SearchUsers(string username)
        {
            var userStrings = await _hub.SearchUser(username);

            return userStrings
                .TakeWhile((user) =>
                {
                    var data = user.Split('#');
                    return !(_hub.CurrentUser.Name == data[0]
                        && _hub.CurrentUser.NameId.ToString() == data[1]);
                }).ToList();
        }

        private async Task<User> GetUserWithName(string username, uint nameId)
        {
            var user = await _hub.GetUserWithNameId(username, nameId);

            return user;
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = _viewModel != null;

            return canExecute;
        }

        /// <summary>
        /// Opens the dialog to start a new private chat
        /// </summary>
        public async void Execute(object parameter)
        {
            LogContext.PushProperty("Method", "Execute");
            LogContext.PushProperty("SourceContext", GetType().Name);

            try
            {
                if (_hub.CurrentUser == null)
                {
                    return;
                }

                _dialog.CurrentUser = _hub.CurrentUser;

                if (await _dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    var userdata = _dialog.SelectedUser;

                    if (userdata == null)
                    {
                        return;
                    }

                    if (_viewModel.Chats
                        .Where(chat => chat.Partner.Id == userdata.Id)
                        .Count() > 0)
                    {
                        _logger.Information($"Cannot start a second private chat with the same user.");

                        await ResultConfirmationDialog
                            .Set(false, $"You have already started a chat with {userdata.DisplayName}.")
                            .ShowAsync();

                        return;
                    }

                    _logger.Information($"Requesting to start a new private chat with {userdata.DisplayName}");

                    bool isSuccess = await _hub.StartChat(userdata.Id);

                    if (isSuccess)
                    {
                        await ResultConfirmationDialog
                                .Set(true, $"You have started a new chat with {userdata.DisplayName}.")
                                .ShowAsync();
                    }
                    else
                    {
                        await ResultConfirmationDialog
                                .Set(false, $"We could not create a new chat with {userdata.DisplayName}.")
                                .ShowAsync();
                    }
                }

                _dialog.SelectedUser = null;
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
