using Messenger.Core.Helpers;
using Messenger.Helpers;
using Messenger.Models;
using Messenger.Services;
using Messenger.Services.Providers;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class ChannelSwitchCommand : ICommand
    {
        private ILogger _log = GlobalLogger.Instance;

        public ChannelSwitchCommand()
        {
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            bool executable = parameter != null
                && parameter is ChannelViewModel; 

            if (!executable) return;

            try
            {
                ChannelViewModel viewModel = parameter as ChannelViewModel;

                if (viewModel == null) return;

                if (CacheQuery.IsChannelOf<PrivateChatViewModel>(viewModel))
                {
                    /** GET FROM CACHE **/
                    PrivateChatViewModel chatViewModel = CacheQuery.Get<PrivateChatViewModel>(viewModel.TeamId);

                    /** UPDATES TEAM AS PRIVATE CHAT AND SETS CHANNEL TO MAIN **/
                    App.StateProvider.SelectedTeam = chatViewModel;
                    App.StateProvider.SelectedChannel = chatViewModel.MainChannel;

                    /** TRIGGERS NAVIGATION AND MESSAGE CONTROLS **/
                    App.EventProvider.Broadcast(
                        BroadcastOptions.MessagesSwitched,
                        BroadcastReasons.Loaded);
                }
                else if (CacheQuery.IsChannelOf<TeamViewModel>(viewModel))
                {
                    /** GET CHANNEL AND TEAM FROM CACHE **/
                    ChannelViewModel channel = CacheQuery.Get<ChannelViewModel>(viewModel.ChannelId);
                    TeamViewModel team = CacheQuery.Get<TeamViewModel>(channel.TeamId);

                    /** EXIT IF THE CHANNEL DOES NOT EXIST IN CACHE **/
                    if (channel == null
                        || team == null)
                    {
                        return;
                    }

                    /** UPDATE SELECTED TEAM/CHANNEL **/
                    App.StateProvider.SelectedTeam = team;
                    App.StateProvider.SelectedChannel = channel;

                    /** TRIGGERS NAVIGATION AND MESSAGE CONTROLS **/
                    App.EventProvider.Broadcast(
                        BroadcastOptions.MessagesSwitched,
                        BroadcastReasons.Loaded);
                }

                NavigationService.Open<ChatPage>();
            }
            catch (Exception e)
            {
                _log.Information($"Error while switching channel: {e.Message}");
            }
        }
    }
}
