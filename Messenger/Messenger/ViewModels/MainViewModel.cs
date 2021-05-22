using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Messenger.Core.Helpers;
using Messenger.Core.Services;
using Messenger.Helpers;
using Microsoft.AspNetCore.SignalR.Client;

namespace Messenger.ViewModels
{
    public class MainViewModel : Observable
    {
        /// <summary>
        /// Shows an example of using service in view model, and adding the data to source for the view
        /// </summary>
        /* This should be deleted (example code) */
        private AzureTestDataService testDataService => Singleton<AzureTestDataService>.Instance;

        /* This should be deleted (example code) */
        public ObservableCollection<SampleTeam> SampleTeamsSource { get; } = new ObservableCollection<SampleTeam>();

        public ChatRoomViewModel ChatRoom { get; set; }

        public MainViewModel()
        {
            ConnectToChatRoom();
        }

        private async void InitalizeData()
        {
            /* This should be deleted (example code) */
            /* foreach (SampleTeam team in await testDataService.GetAllTeams())
            {
                SampleTeamsSource.Add(team);
            } */
        }

        private void ConnectToChatRoom()
        {
            HubConnection connection = new HubConnectionBuilder()
                .WithUrl("https://vpr.azurewebsites.net/chatroom")
                .Build();

            ChatRoom = ChatRoomViewModel.CreateConnectedViewModel(new Services.SignalRChatService(connection));
        }
    }
}
