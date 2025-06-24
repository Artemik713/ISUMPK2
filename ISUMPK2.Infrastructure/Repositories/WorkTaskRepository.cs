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
    public class WorkTaskRepository : Repository<WorkTask>, IWorkTaskRepository
    {
        public WorkTaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task DeleteAsync(Guid id)
        {
            // Находим и удаляем связанные уведомления перед удалением задачи
            var notifications = await _context.Notifications
                .Where(n => n.TaskId == id)
                .ToListAsync();

            if (notifications.Any())
            {
                _context.Notifications.RemoveRange(notifications);
            }

            // Теперь можно безопасно удалить задачу
            var task = await GetByIdAsync(id);
            if (task != null)
            {
                _dbSet.Remove(task);
            }
        }
    }
}