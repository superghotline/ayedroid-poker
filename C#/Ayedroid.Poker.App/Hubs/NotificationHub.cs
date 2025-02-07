﻿using Ayedroid.Poker.App.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Ayedroid.Poker.App.Hubs
{
    public class NotificationHub : Hub<INotificationClient>
    {
        private const string UsersGroup = "Users";

        public NotificationHub()
        {

        }

        public override async Task OnConnectedAsync()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UsersGroup);
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, UsersGroup);
            await base.OnDisconnectedAsync(exception);
        }
    }
}
