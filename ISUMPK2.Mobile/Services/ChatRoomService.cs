using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ISUMPK2.Application.Services;
using ISUMPK2.Mobile.Models;

namespace ISUMPK2.Mobile.Services
{
    public class ChatRoomService : IChatRoomService
    {
        private readonly IChatService _chatService;

        public ChatRoomService(IChatService chatService)
        {
            _chatService = chatService;
        }

        public async Task<ChatRoomModel> GetChatRoomAsync(Guid chatId)
        {
            // Адаптировать под ваш фактический API
            // Например, можно получить сведения о чате через API департаментов
            return new ChatRoomModel
            {
                Id = chatId,
                Name = "Комната " + chatId.ToString().Substring(0, 8)
            };
        }

        public async Task<List<ChatMessageModel>> GetChatMessagesAsync(Guid chatId)
        {
            // Используем существующий метод для получения сообщений департамента
            var messages = await _chatService.GetMessagesForDepartmentAsync(chatId);

            // Конвертируем сообщения из DTO в модель
            return messages.Select(msg => new ChatMessageModel
            {
                Id = msg.Id,
                ChatRoomId = chatId,
                SenderId = msg.SenderId,
                SenderName = msg.SenderName,
                Content = msg.Message,
                SentAt = msg.CreatedAt
            }).ToList();
        }
    }
}