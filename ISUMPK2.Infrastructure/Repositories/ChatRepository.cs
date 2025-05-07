using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Infrastructure.Repositories
{
    public class ChatRepository : Repository<ChatMessage>, IChatRepository
    {
        public ChatRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesForUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesForDepartmentAsync(Guid departmentId)
        {
            return await _dbSet
                .Where(m => m.DepartmentId == departmentId)
                .Include(m => m.Sender)
                .Include(m => m.Department)
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetConversationAsync(Guid senderId, Guid receiverId)
        {
            return await _dbSet
                .Where(m => (m.SenderId == senderId && m.ReceiverId == receiverId) ||
                           (m.SenderId == receiverId && m.ReceiverId == senderId))
                .Include(m => m.Sender)
                .Include(m => m.Receiver)
                .OrderBy(m => m.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetUnreadMessagesCountForUserAsync(Guid userId)
        {
            return await _dbSet
                .CountAsync(m => m.ReceiverId == userId && !m.IsRead);
        }

        public async Task MarkAsReadAsync(Guid messageId)
        {
            var message = await _dbSet.FindAsync(messageId);
            if (message != null)
            {
                message.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            var messages = await _dbSet
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
