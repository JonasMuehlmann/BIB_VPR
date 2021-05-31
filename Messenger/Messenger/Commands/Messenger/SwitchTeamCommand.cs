using Messenger.ViewModels;
using System;
using Messenger.Core.Models;
using System.Windows.Input;
using System.Collections.ObjectModel;

namespace Messenger.Commands.Messenger
{
    public class SwitchTeamCommand : ICommand
    {
        private readonly ChatHubViewModel _viewModel;

        public SwitchTeamCommand(ChatHubViewModel viewModel)
        {
            _viewModel = viewModel;
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Switch the team to be shown on the view
        /// </summary>
        /// <param name="parameter">Id of a team</param>
        public void Execute(object parameter)
        {
            _viewModel.CurrentTeamId = Convert.ToUInt32(parameter);
            _viewModel.Messages = _viewModel.MessagesByConnectedTeam.GetOrAdd(_viewModel.CurrentTeamId, new ObservableCollection<Message>());
        }
    }
}
