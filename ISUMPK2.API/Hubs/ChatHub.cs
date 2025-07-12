using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ISUMPK2.Application.Services;
using ISUMPK2.Application.DTOs;
using System.Security.Claims;

namespace ISUMPK2.API.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUserService _userService;

        public ChatHub(IChatService chatService, IUserService userService)
        {
            _chatService = chatService;
            _userService = userService;
        }

        private Guid GetCurrentUserId()
        {
            // Получаем ID пользователя из claims
            var userIdString = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdString, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("Пользователь не авторизован");
        }

        public override async Task OnConnectedAsync()
        {
            try
            {
                var userId = GetCurrentUserId();
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
                Console.WriteLine($"ChatHub: Пользователь {userId} подключился");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatHub: Ошибка подключения: {ex.Message}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            try
            {
                var userId = GetCurrentUserId();
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
                Console.WriteLine($"ChatHub: Пользователь {userId} отключился");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatHub: Ошибка отключения: {ex.Message}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            Console.WriteLine($"ChatHub: Пользователь {userId} присоединился к группе");
        }

        public async Task JoinDepartmentGroup(string departmentId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Department_{departmentId}");
            Console.WriteLine($"ChatHub: Пользователь присоединился к отделу {departmentId}");
        }

        public async Task LeaveUserGroup(string userId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
        }

        public async Task LeaveDepartmentGroup(string departmentId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Department_{departmentId}");
        }

        public async Task SendMessageToUser(string receiverIdStr, string messageText)
        {
            try
            {
                Console.WriteLine($"ChatHub: Попытка отправки сообщения к {receiverIdStr}");

                var currentUserId = GetCurrentUserId();
                var receiverId = Guid.Parse(receiverIdStr);

                Console.WriteLine($"ChatHub: Отправитель {currentUserId}, получатель {receiverId}");

                // ЛЮБОЙ авторизованный пользователь может отправлять сообщения любому другому
                var messageDto = new ChatMessageCreateDto
                {
                    ReceiverId = receiverId,
                    Message = messageText
                };

                var savedMessage = await _chatService.SendMessageAsync(currentUserId, messageDto);
                Console.WriteLine($"ChatHub: Сообщение сохранено в БД с ID {savedMessage.Id}");

                // Получаем данные отправителя
                var sender = await _userService.GetUserByIdAsync(currentUserId);

                var messageModel = new
                {
                    Id = savedMessage.Id,
                    SenderId = savedMessage.SenderId,
                    SenderName = $"{sender?.FirstName} {sender?.LastName}".Trim(),
                    ReceiverId = savedMessage.ReceiverId,
                    Message = savedMessage.Message,
                    IsRead = savedMessage.IsRead,
                    CreatedAt = savedMessage.CreatedAt
                };

                Console.WriteLine($"ChatHub: Отправляем сообщение в группу User_{receiverId}");

                // Отправляем сообщение получателю
                await Clients.Group($"User_{receiverId}").SendAsync("ReceiveMessage", messageModel);

                // Отправляем подтверждение отправителю
                await Clients.Caller.SendAsync("MessageSent", messageModel);

                Console.WriteLine($"ChatHub: Сообщение успешно отправлено");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ChatHub: Ошибка отправки сообщения: {ex.Message}");
                await Clients.Caller.SendAsync("MessageError", $"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        public async Task SendMessageToDepartment(string departmentIdStr, string messageText)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                var departmentId = Guid.Parse(departmentIdStr);

                // Сохраняем сообщение в базе данных
                var messageDto = new ChatMessageCreateDto
                {
                    DepartmentId = departmentId,
                    Message = messageText
                };

                var savedMessage = await _chatService.SendMessageAsync(currentUserId, messageDto);

                // Получаем данные отправителя
                var sender = await _userService.GetUserByIdAsync(currentUserId);

                // Создаем модель сообщения
                var messageModel = new
                {
                    Id = savedMessage.Id,
                    SenderId = savedMessage.SenderId,
                    SenderName = $"{sender?.FirstName} {sender?.LastName}".Trim(),
                    DepartmentId = savedMessage.DepartmentId,
                    Message = savedMessage.Message,
                    IsRead = savedMessage.IsRead,
                    CreatedAt = savedMessage.CreatedAt
                };

                // Отправляем сообщение всем участникам отдела
                await Clients.Group($"Department_{departmentId}").SendAsync("ReceiveMessage", messageModel);
            }
            catch (Exception ex)
            {
                await Clients.Caller.SendAsync("MessageError", $"Ошибка отправки сообщения: {ex.Message}");
            }
        }
    }
}