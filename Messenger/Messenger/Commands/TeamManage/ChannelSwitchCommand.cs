using Messenger.Core.Helpers;
using Messenger.Helpers;
using Messenger.Services;
using Messenger.ViewModels.DataViewModels;
using Messenger.Views;
using Serilog;
using System;
using System.Windows.Input;

namespace Messenger.Commands.TeamManage
{
    public class ChannelSwitchCommand : ICommand
    {
        private readonly ChatHubService _hub;

        private ILogger _log = GlobalLogger.Instance;

        public ChannelSwitchCommand(ChatHubService hub)
        {
            _hub = hub;
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
                    _hub.SwitchChat(viewModel.TeamId);
                }
                else if (CacheQuery.IsChannelOf<TeamViewModel>(viewModel))
                {
                    _hub.SwitchTeamChannel(viewModel.ChannelId);
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
