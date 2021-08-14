using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using Messenger.Views.Pages;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class StartChatWithUserCommand : ICommand
    {
        private ILogger _logger => GlobalLogger.Instance;

        public event EventHandler CanExecuteChanged;

        public StartChatWithUserCommand()
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
            bool executable = parameter != null
                && parameter is MemberViewModel;

            if (!executable)
            {
                return;
            }

            try
            {
                MemberViewModel selectedMember = parameter as MemberViewModel;
                UserViewModel currentUser = App.StateProvider.CurrentUser;

                foreach (PrivateChatViewModel chat in CacheQuery.GetMyChats())
                {
                    if (chat.Partner.Id == selectedMember.Id)
                    {
                        SwitchToChatPage(chat);
                        return;
                    }
                }

                uint? chatId = await MessengerService.StartChat(currentUser.Id, selectedMember.Id);

                if (chatId != null)
                {
                    await ResultConfirmationDialog
                            .Set(true, $"You have started a new chat with {selectedMember.Name}.")
                            .ShowAsync();

                    PrivateChatViewModel chat = CacheQuery.Get<PrivateChatViewModel>(chatId);

                    if (chat != null)
                    {
                        SwitchToChatPage(chat);
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

        private void SwitchToChatPage(PrivateChatViewModel chat)
        {
            App.StateProvider.SelectedTeam = chat;
            App.StateProvider.SelectedChannel = chat.MainChannel;

            NavigationService.Navigate<ChatNavPage>();
            NavigationService.Open<ChatPage>();
        }
    }
}
