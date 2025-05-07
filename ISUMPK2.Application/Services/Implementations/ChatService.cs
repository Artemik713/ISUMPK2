using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services.Implementations
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IUserRepository _userRepository;

        public ChatService(IChatRepository chatRepository, IUserRepository userRepository)
        {
            _chatRepository = chatRepository;
            _userRepository = userRepository;
        }

        public async Task<ChatMessageDto> GetMessageByIdAsync(Guid id)
        {
            var message = await _chatRepository.GetByIdAsync(id);
            return message != null ? MapToDto(message) : null;
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesForUserAsync(Guid userId)
        {
            var messages = await _chatRepository.GetMessagesForUserAsync(userId);
            return messages.Select(MapToDto);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetMessagesForDepartmentAsync(Guid departmentId)
        {
            var messages = await _chatRepository.GetMessagesForDepartmentAsync(departmentId);
            return messages.Select(MapToDto);
        }

        public async Task<IEnumerable<ChatMessageDto>> GetConversationAsync(Guid senderId, Guid receiverId)
        {
            var messages = await _chatRepository.GetConversationAsync(senderId, receiverId);
            return messages.Select(MapToDto);
        }

        public async Task<ChatMessageDto> SendMessageAsync(Guid senderId, ChatMessageCreateDto messageDto)
        {
            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = messageDto.ReceiverId,
                DepartmentId = messageDto.DepartmentId,
                Message = messageDto.Message,
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _chatRepository.AddAsync(message);
            await _chatRepository.SaveChangesAsync();

            return MapToDto(message);
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            await _chatRepository.MarkAsReadAsync(messageId);
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            await _chatRepository.MarkAllAsReadForUserAsync(userId);
        }

        public async Task<int> GetUnreadMessagesCountForUserAsync(Guid userId)
        {
            return await _chatRepository.GetUnreadMessagesCountForUserAsync(userId);
        }

        private ChatMessageDto MapToDto(ChatMessage message)
        {
            return new ChatMessageDto
            {
                Id = message.Id,
                SenderId = message.SenderId,
                SenderName = message.Sender?.UserName,
                ReceiverId = message.ReceiverId,
                ReceiverName = message.Receiver?.UserName,
                DepartmentId = message.DepartmentId,
                DepartmentName = message.Department?.Name,
                Message = message.Message,
                IsRead = message.IsRead,
                CreatedAt = message.CreatedAt
            };
        }
    }
}
