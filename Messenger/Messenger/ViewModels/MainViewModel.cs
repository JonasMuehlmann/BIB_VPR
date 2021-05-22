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
        public ChatRoomViewModel ChatRoom { get; set; }

        public MainViewModel()
        {
            //ConnectToChatRoom();
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
