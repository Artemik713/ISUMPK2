using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Domain.Repositories
{
    public interface IChatRepository:IRepository<ChatMessage>
    {
        Task<IEnumerable<ChatMessage>> GetMessagesForUserAsync(Guid userId);
        Task<IEnumerable<ChatMessage>> GetMessagesForDepartmentAsync(Guid departmentId);
        Task<IEnumerable<ChatMessage>> GetConversationAsync(Guid senderId, Guid receiverId);
        Task<int> GetUnreadMessagesCountForUserAsync(Guid userId);
        Task MarkAsReadAsync(Guid messageId);
        Task MarkAllAsReadForUserAsync(Guid userId);
    }
}
