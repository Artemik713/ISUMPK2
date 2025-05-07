using ISUMPK2.Web.Models;
using Microsoft.AspNetCore.SignalR.Client;

namespace ISUMPK2.Web.Services
{
    public class ChatHubService : IChatHubService
    {
        private readonly ILocalStorageService _localStorageService;
        private HubConnection _hubConnection;
        private bool _isConnected;

        public event Action<ChatMessageModel> OnReceiveMessage;

        public ChatHubService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
            InitializeHub();
        }

        private void InitializeHub()
        {
            string baseUrl = "https://localhost:7001"; // В реальном приложении берите из конфигурации
            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/chatHub")
                .WithAutomaticReconnect()
                .Build();

            _hubConnection.On<ChatMessageModel>("ReceiveMessage", (message) =>
            {
                OnReceiveMessage?.Invoke(message);
            });
        }

        public async Task ConnectAsync()
        {
            if (!_isConnected)
            {
                try
                {
                    await _hubConnection.StartAsync();
                    _isConnected = true;

                    var userId = await _localStorageService.GetItemAsync<string>("userId");
                    if (!string.IsNullOrEmpty(userId))
                    {
                        await JoinUserGroupAsync(userId);
                    }
                }
                catch (Exception)
                {
                    // Логирование ошибки
                }
            }
        }

        public async Task DisconnectAsync()
        {
            if (_isConnected)
            {
                await _hubConnection.StopAsync();
                _isConnected = false;
            }
        }

        public async Task JoinUserGroupAsync(string userId)
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("JoinUserGroup", userId);
            }
        }

        public async Task JoinDepartmentGroupAsync(string departmentId)
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("JoinDepartmentGroup", departmentId);
            }
        }

        public async Task LeaveUserGroupAsync(string userId)
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("LeaveUserGroup", userId);
            }
        }

        public async Task LeaveDepartmentGroupAsync(string departmentId)
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("LeaveDepartmentGroup", departmentId);
            }
        }

        public async Task SendMessageToUserAsync(string userId, string message)
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("SendMessageToUser", userId, message);
            }
        }

        public async Task SendMessageToDepartmentAsync(string departmentId, string message)
        {
            if (_isConnected)
            {
                await _hubConnection.InvokeAsync("SendMessageToDepartment", departmentId, message);
            }
        }
    }
}
