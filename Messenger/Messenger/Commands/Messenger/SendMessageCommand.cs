using System;
using Messenger.Core.Models;
using System.Windows.Input;
using Serilog;
using Messenger.Core.Helpers;
using Messenger.Views.DialogBoxes;
using Messenger.Core.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.ViewModels.Pages;

namespace Messenger.Commands.Messenger
{
    /// <summary>
    /// Send the message to the current channel
    /// </summary>
    public class SendMessageCommand : ICommand
    {
        private readonly ChatViewModel _viewModel;
        private ILogger _logger => GlobalLogger.Instance;

        public SendMessageCommand(ChatViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Checks if the message is valid to be forwarded to the hub
        /// </summary>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Sends a new message to the hub and saves it to database
        /// </summary>
        public async void Execute(object parameter)
        {
            bool executable = _viewModel.MessageToSend != null
                && !string.IsNullOrEmpty(_viewModel.MessageToSend.Content)
                && App.StateProvider.CurrentUser != null
                && App.StateProvider.SelectedChannel != null;

            if (!executable)
            {
                return;
            }

            try
            {
                MessageViewModel message = _viewModel.MessageToSend;
                TeamViewModel team = App.StateProvider.SelectedTeam;

                // Records created timestamp
                message.CreationTime = DateTime.Now;
                message.SenderId = App.StateProvider.CurrentUser.Id;
                message.ChannelId = App.StateProvider.SelectedChannel.ChannelId;
                
                if (message.Mentionables.Count > 0)
                {
                    foreach (Mentionable mentionable in message.Mentionables)
                    {
                        mentionable.IndexAndLength.Deconstruct(out int index, out int length);
                        string mentionString = message.Content.Substring(index, length);

                        // CreateMention
                        uint? mentionId = await MentionService.CreateMention(mentionable.TargetType, mentionable.TargetId, App.StateProvider.CurrentUser.Id);

                        if (mentionId == null) break;

                        // Replaced mention id to send to database
                        message.Content = message.Content.Replace(mentionString, "@" + mentionId);
                    }
                }

                bool success = await MessengerService.SendMessage(message.ToDatabaseModel(), team.Id);
            }
            catch (Exception e)
            {
                _logger.Information($"Error while sending message: {e.Message}");
                await ResultConfirmationDialog
                        .Set(false, $"{e.Message}")
                        .ShowAsync();
            }
            finally
            {
                // Resets the models in the view model
                _viewModel.ReplyMessage = null;
                _viewModel.MessageToSend = new MessageViewModel();
            }
        }
    }
}
