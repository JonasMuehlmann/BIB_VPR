using Messenger.Core.Models;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Messenger.SignalR
{
    public class ChatHub : Hub
    {
        public async Task JoinTeam(string teamId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, teamId);
        }

        public async Task AddToTeam(string connectionId, string teamId)
        {
            await Groups.AddToGroupAsync(connectionId, teamId);
        }

        public async Task SendMessage(Message message)
        {
            string groupName = message.RecipientId.ToString();

            await Clients.Groups(groupName).SendAsync("ReceiveMessage", message);
        }
    }
}
