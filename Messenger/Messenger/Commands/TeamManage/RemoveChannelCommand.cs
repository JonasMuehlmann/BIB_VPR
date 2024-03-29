﻿using Messenger.Core.Models;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views.DialogBoxes;
using System;
using System.Linq;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    /// <summary>
    /// Remove the channel from the team
    /// </summary>
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
                OperationConfirmationDialog dialog = OperationConfirmationDialog.Set("You're about to remove a channel");

                await dialog.ShowAsync();

                if (!dialog.Response) return;

                TeamViewModel selectedTeam = App.StateProvider.SelectedTeam;

                uint channelId = (uint)parameter;

                ChannelViewModel channel = CacheQuery.Get<ChannelViewModel>(channelId);

                if (channel == null)
                {
                    await ResultConfirmationDialog
                        .Set(false, $"No channel was found with id: {channelId}")
                        .ShowAsync();
                    return;
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
