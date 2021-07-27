using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class RemoveChannelCommand : ICommand
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
                TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

                uint channelId = (uint)parameter;

                ChannelViewModel channel = CacheQuery.Get<ChannelViewModel>(channelId);

                if (channel == null)
                {
                    await ResultConfirmationDialog
                        .Set(false, $"No channel was found with id: {channelId}")
                        .ShowAsync();
                }

                Channel deleted = await MessengerService.DeleteChannel(channel.ChannelId);

                if (deleted != null)
                {
                    await ResultConfirmationDialog
                        .Set(true, $"Removed channel {channel.ChannelName}#{channel.ChannelId} from the team #{channel.TeamId}")
                        .ShowAsync();
                }
            }
            catch (Exception e)
            {
                await ResultConfirmationDialog
                    .Set(false, $"We could not remove the user, try again: {e.Message}")
                    .ShowAsync();
            }
        }
    }
}
