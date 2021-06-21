using Messenger.Core.Helpers;
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
            _dialog = new CreateChatDialog();
            _dialog.OnSearch = SearchUsers;
        }

        /// <summary>
        /// Returns the search result from the database
        /// </summary>
        /// <param name="username">DisplayName of the user to search for</param>
        /// <returns>List of search result strings</returns>
        private async Task<IList<string>> SearchUsers(string username)
        {
            var userStrings = await _hub.SearchUser(username);

            return userStrings;
        }

        public bool CanExecute(object parameter)
        {
            bool canExecute = !_viewModel.IsBusy;

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
                if (await _dialog.ShowAsync() == ContentDialogResult.Primary)
                {
                    var selectedUser = _dialog.SelectedUser;

                    if (selectedUser == null)
                    {
                        return;
                    }

                    if (_viewModel.Chats
                        .Where(chat => chat.Partner.Id == selectedUser.Id)
                        .Count() > 0)
                    {
                        _logger.Information($"Cannot start a second private chat with the same user.");
                        return;
                    }

                    await _hub.StartChat(selectedUser.DisplayName, selectedUser.NameId);
                }
            }
            catch (Exception e)
            {
                _logger.Information($"Error while starting a new private chat: {e.Message}");
            }
        }
    }
}
