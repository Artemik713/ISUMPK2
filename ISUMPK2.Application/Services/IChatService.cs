using ISUMPK2.Application.DTOs;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ISUMPK2.Application.Services
{
    public interface IChatService
    {
        Task<ChatMessageDto> GetMessageByIdAsync(Guid id);
        Task<IEnumerable<ChatMessageDto>> GetMessagesForUserAsync(Guid userId);
        Task<IEnumerable<ChatMessageDto>> GetMessagesForDepartmentAsync(Guid departmentId);
        Task<IEnumerable<ChatMessageDto>> GetConversationAsync(Guid senderId, Guid receiverId);
        Task<int> GetUnreadMessagesCountForUserAsync(Guid userId);
        Task<ChatMessageDto> SendMessageAsync(Guid senderId, ChatMessageCreateDto messageDto);
        Task MarkAsReadAsync(Guid messageId);
        Task MarkAllAsReadForUserAsync(Guid userId);
    }
}
