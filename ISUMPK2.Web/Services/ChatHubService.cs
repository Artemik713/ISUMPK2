using Microsoft.AspNetCore.SignalR.Client;
using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public class ChatHubService : IChatHubService, IAsyncDisposable
    {
        private readonly ILocalStorageService _localStorageService;
        private HubConnection _hubConnection;
        private bool _isConnected;
        private string _currentUserId;

        public event Action<ChatMessageModel> OnReceiveMessage;
        public event Action<ChatMessageModel> OnMessageSent;
        public event Action<string> OnMessageError;

        public bool IsConnected => _isConnected && _hubConnection?.State == HubConnectionState.Connected;

        public ChatHubService(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        private async Task InitializeHubAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }

            string baseUrl = "https://localhost:7110"; // ИСПРАВЛЕН URL - такой же как API
            var token = await GetTokenAsync();

            _hubConnection = new HubConnectionBuilder()
                .WithUrl($"{baseUrl}/chatHub", options =>
                {
                    if (!string.IsNullOrEmpty(token))
                    {
                        options.AccessTokenProvider = () => Task.FromResult(token);
                    }
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                .Build();

            _hubConnection.On<object>("ReceiveMessage", (messageData) =>
            {
                try
                {
                    var message = ParseMessage(messageData);
                    OnReceiveMessage?.Invoke(message);
                }
                catch (Exception ex)
                {
                    OnMessageError?.Invoke($"Ошибка обработки полученного сообщения: {ex.Message}");
                }
            });

            _hubConnection.On<object>("MessageSent", (messageData) =>
            {
                try
                {
                    var message = ParseMessage(messageData);
                    OnMessageSent?.Invoke(message);
                }
                catch (Exception ex)
                {
                    OnMessageError?.Invoke($"Ошибка обработки отправленного сообщения: {ex.Message}");
                }
            });

            _hubConnection.On<string>("MessageError", (error) =>
            {
                OnMessageError?.Invoke(error);
            });

            _hubConnection.Reconnecting += (exception) =>
            {
                _isConnected = false;
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += async (connectionId) =>
            {
                _isConnected = true;
                if (!string.IsNullOrEmpty(_currentUserId))
                {
                    await JoinUserGroupAsync(_currentUserId);
                }
            };

            _hubConnection.Closed += (exception) =>
            {
                _isConnected = false;
                return Task.CompletedTask;
            };
        }

        private async Task<string> GetTokenAsync()
        {
            try
            {
                return await _localStorageService.GetItemAsync<string>("authToken") ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private ChatMessageModel ParseMessage(object messageData)
        {
            try
            {
                // Преобразуем объект в ChatMessageModel
                var json = System.Text.Json.JsonSerializer.Serialize(messageData);
                Console.WriteLine($"ChatHubService: Парсим сообщение: {json}");

                var options = new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var result = System.Text.Json.JsonSerializer.Deserialize<ChatMessageModel>(json, options);
                Console.WriteLine($"ChatHubService: Сообщение успешно распарсено от {result.SenderName}");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatHubService: Ошибка парсинга сообщения: {ex.Message}");
                throw;
            }
        }

        public async Task ConnectAsync()
        {
            if (_isConnected && _hubConnection?.State == HubConnectionState.Connected)
                return;

            try
            {
                await InitializeHubAsync();
                await _hubConnection.StartAsync();
                _isConnected = true;

                // Получаем ID текущего пользователя из localStorage
                var userIdString = await _localStorageService.GetItemAsync<string>("userId");
                Console.WriteLine($"ChatHubService: Получен userId из localStorage: {userIdString}");

                if (!string.IsNullOrEmpty(userIdString))
                {
                    await JoinUserGroupAsync(userIdString);
                    _currentUserId = userIdString;
                    Console.WriteLine($"ChatHubService: Присоединились к группе User_{userIdString}");
                }
                else
                {
                    Console.WriteLine("ChatHubService: ВНИМАНИЕ - userId не найден в localStorage");
                }
            }
            catch (Exception ex)
            {
                _isConnected = false;
                Console.WriteLine($"ChatHubService: Ошибка подключения: {ex.Message}");
                OnMessageError?.Invoke($"Ошибка подключения к чату: {ex.Message}");
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.StopAsync();
                _isConnected = false;
            }
        }

        public async Task JoinUserGroupAsync(string userId)
        {
            if (_isConnected && _hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("JoinUserGroup", userId);
                _currentUserId = userId;
            }
        }

        public async Task JoinDepartmentGroupAsync(string departmentId)
        {
            if (_isConnected && _hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("JoinDepartmentGroup", departmentId);
            }
        }

        public async Task LeaveUserGroupAsync(string userId)
        {
            if (_isConnected && _hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveUserGroup", userId);
            }
        }

        public async Task LeaveDepartmentGroupAsync(string departmentId)
        {
            if (_isConnected && _hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("LeaveDepartmentGroup", departmentId);
            }
        }

        public async Task SendMessageToUserAsync(string userId, string message)
        {
            if (_isConnected && _hubConnection?.State == HubConnectionState.Connected)
            {
                Console.WriteLine($"ChatHubService: Отправляем сообщение пользователю {userId}: {message}");
                await _hubConnection.InvokeAsync("SendMessageToUser", userId, message);
            }
            else
            {
                Console.WriteLine("ChatHubService: Нет соединения с сервером чата");
                OnMessageError?.Invoke("Нет соединения с сервером чата");
            }
        }

        public async Task SendMessageToDepartmentAsync(string departmentId, string message)
        {
            if (_isConnected && _hubConnection?.State == HubConnectionState.Connected)
            {
                await _hubConnection.InvokeAsync("SendMessageToDepartment", departmentId, message);
            }
            else
            {
                OnMessageError?.Invoke("Нет соединения с сервером чата");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }
}