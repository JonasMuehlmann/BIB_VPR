using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Messenger.Services;

namespace Messenger.ViewModels
{
    public class MainViewModel : Observable
    {
        #region Private

        private SignalRHubViewModel _hub;

        #endregion

        /// <summary>
        /// SignalR hub with an established connection, preconfigured with the current user data
        /// </summary>
        public SignalRHubViewModel Hub
        {
            get { return _hub; }
            set
            {
                _hub = value;
                Set(ref _hub, value);
            }
        }

        public MainViewModel()
        {
            InitializeHub();
        }

        private void InitializeHub()
        {
            // Gets connection to the signalR hub
            Hub = SignalRHubViewModel.CreateConnectedViewModel();
        }
    }
}
