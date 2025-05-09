using Microsoft.AspNetCore.SignalR.Client;
using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private HubConnection? _hubConnection;

        public event Action<NotificationModel>? OnReceiveNotification;

        public async Task ConnectAsync()
        {
            _hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:7001/notificationhub") // заменишь на свой адрес, если нужно
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<NotificationModel>("ReceiveNotification", (notification) =>
            {
                OnReceiveNotification?.Invoke(notification);
            });

            await _hubConnection.StartAsync();
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                await _hubConnection.DisposeAsync();
            }
        }

        public async Task JoinUserGroupAsync(string userId)
        {
            if (_hubConnection != null)
                await _hubConnection.InvokeAsync("JoinGroup", userId);
        }

        public async Task LeaveUserGroupAsync(string userId)
        {
            if (_hubConnection != null)
                await _hubConnection.InvokeAsync("LeaveGroup", userId);
        }
    }
}
