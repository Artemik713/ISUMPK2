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
    public class NotificationRepository : Repository<Notification>, INotificationRepository
    {
        public NotificationRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsForUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .Include(n => n.Task)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Notification>> GetReadNotificationsForUserAsync(Guid userId)
        {
            return await _dbSet
                .Where(n => n.UserId == userId && n.IsRead)
                .Include(n => n.Task)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _dbSet.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            var notifications = await _dbSet
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId)
        {
            return await _dbSet
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }
    }
}
