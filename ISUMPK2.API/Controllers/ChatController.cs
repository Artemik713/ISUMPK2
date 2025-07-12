using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ISUMPK2.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService chatService)
        {
            _chatService = chatService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ChatMessageDto>> GetMessage(Guid id)
        {
            try
            {
                var message = await _chatService.GetMessageByIdAsync(id);
                if (message == null)
                {
                    return NotFound();
                }
                return Ok(message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка получения сообщения: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}")]
        [Authorize] // Любой авторизованный пользователь может получать сообщения
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessagesForUser(Guid userId)
        {
            try
            {
                // Получаем текущего пользователя
                var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"ChatController: Current user ID from token: {currentUserIdString}");
                Console.WriteLine($"ChatController: Requested user ID: {userId}");

                if (!Guid.TryParse(currentUserIdString, out var currentUserId))
                {
                    return Unauthorized("Невалидный токен пользователя");
                }

                // ДЛЯ ЧАТА: Разрешаем любому пользователю получать сообщения для чата
                // Можно добавить ограничение: пользователь может получать только свои сообщения или переписку с другими
                Console.WriteLine($"ChatController: Пользователь {currentUserId} запрашивает сообщения для {userId}");

                var messages = await _chatService.GetMessagesForUserAsync(userId);
                Console.WriteLine($"ChatController: Найдено {messages?.Count() ?? 0} сообщений");

                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка в ChatController: {ex.Message}");
                return StatusCode(500, $"Ошибка получения сообщений пользователя: {ex.Message}");
            }
        }

        [HttpGet("conversation")]
        [Authorize] // Любой может получать переписку
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetConversation(Guid senderId, Guid receiverId)
        {
            try
            {
                // Получаем текущего пользователя
                var currentUserIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!Guid.TryParse(currentUserIdString, out var currentUserId))
                {
                    return Unauthorized("Невалидный токен пользователя");
                }

                Console.WriteLine($"ChatController: Запрос переписки между {senderId} и {receiverId} от пользователя {currentUserId}");

                // Проверяем, что текущий пользователь участвует в переписке
                if (currentUserId != senderId && currentUserId != receiverId)
                {
                    return Forbid("Вы можете просматривать только свои переписки");
                }

                var messages = await _chatService.GetConversationAsync(senderId, receiverId);
                Console.WriteLine($"ChatController: Найдено {messages?.Count() ?? 0} сообщений в переписке");

                return Ok(messages);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка получения переписки: {ex.Message}");
                return StatusCode(500, $"Ошибка получения переписки: {ex.Message}");
            }
        }

        [HttpGet("department/{departmentId}")]
        public async Task<ActionResult<IEnumerable<ChatMessageDto>>> GetMessagesForDepartment(Guid departmentId)
        {
            try
            {
                var messages = await _chatService.GetMessagesForDepartmentAsync(departmentId);
                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка получения сообщений отдела: {ex.Message}");
            }
        }

        [HttpPost("send")]
        public async Task<ActionResult<ChatMessageDto>> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                var messageDto = new ChatMessageCreateDto
                {
                    ReceiverId = request.ReceiverId,
                    DepartmentId = request.DepartmentId,
                    Message = request.Message
                };

                var sentMessage = await _chatService.SendMessageAsync(request.SenderId, messageDto);
                return Ok(sentMessage);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка отправки сообщения: {ex.Message}");
            }
        }

        [HttpPut("{messageId}/mark-read")]
        public async Task<IActionResult> MarkAsRead(Guid messageId)
        {
            try
            {
                await _chatService.MarkAsReadAsync(messageId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка отметки сообщения как прочитанного: {ex.Message}");
            }
        }

        [HttpPut("user/{userId}/mark-all-read")]
        public async Task<IActionResult> MarkAllAsReadForUser(Guid userId)
        {
            try
            {
                await _chatService.MarkAllAsReadForUserAsync(userId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка отметки всех сообщений как прочитанных: {ex.Message}");
            }
        }

        [HttpGet("user/{userId}/unread-count")]
        public async Task<ActionResult<int>> GetUnreadMessagesCount(Guid userId)
        {
            try
            {
                var count = await _chatService.GetUnreadMessagesCountForUserAsync(userId);
                return Ok(count);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка получения количества непрочитанных сообщений: {ex.Message}");
            }
        }
    }

    public class SendMessageRequest
    {
        public Guid SenderId { get; set; }
        public Guid? ReceiverId { get; set; }
        public Guid? DepartmentId { get; set; }
        public string Message { get; set; }
    }
}