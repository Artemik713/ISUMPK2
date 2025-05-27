using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;  
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Infrastructure.Repositories
{
    public class TaskRepository : Repository<Domain.Entities.WorkTask>, ITaskRepository
    {
        public TaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksByStatusAsync(int statusId)
        {
            return await _dbSet
                .Where(t => t.StatusId == statusId)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .ToListAsync();
        }
        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTaskStatusByIdAsync(int statusId)
        {
            return await _dbSet
                .Where(t => t.StatusId == statusId)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksByAssigneeAsync(Guid assigneeId)
        {
            return await _dbSet
                .Where(t => t.AssigneeId == assigneeId)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksByCreatorAsync(Guid creatorId)
        {
            return await _dbSet
                .Where(t => t.CreatorId == creatorId)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksByDepartmentAsync(Guid departmentId)
        {
            return await _dbSet
                .Where(t => t.DepartmentId == departmentId)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksByPriorityAsync(int priorityId)
        {
            return await _dbSet
                .Where(t => t.PriorityId == priorityId)
                .Include(t => t.Status)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _dbSet
                .Where(t => t.DueDate >= startDate && t.DueDate <= endDate)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksByProductAsync(Guid productId)
        {
            return await _dbSet
                .Where(t => t.ProductId == productId)
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetOverdueTasks()
        {
            var currentDate = DateTime.UtcNow.Date;
            return await _dbSet
                .Where(t => t.DueDate < currentDate && t.StatusId != 5 && t.StatusId != 6) // Не выполненные и не отклоненные
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .ToListAsync();
        }

        public async Task<IEnumerable<Domain.Entities.WorkTask>> GetTasksForDashboardAsync(Guid userId)
        {
            // Получаем задачи, которые:
            // 1. Назначены пользователю
            // 2. Созданы пользователем
            // 3. Находятся в отделе пользователя
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new List<Domain.Entities.WorkTask>();

            var isManager = user.UserRoles.Any(ur =>
                ur.Role.NormalizedName == "GENERALDIRECTOR" ||
                ur.Role.NormalizedName == "METALSHOPMANAGER" ||
                ur.Role.NormalizedName == "PAINTSHOPMANAGER");

            var query = _dbSet.AsQueryable();

            if (isManager)
            {
                // Для руководителей показываем задачи их отдела
                if (user.DepartmentId.HasValue)
                {
                    query = query.Where(t =>
                        t.AssigneeId == userId ||
                        t.CreatorId == userId ||
                        t.DepartmentId == user.DepartmentId);
                }
            }
            else
            {
                // Для обычных сотрудников показываем только их задачи
                query = query.Where(t => t.AssigneeId == userId || t.CreatorId == userId);
            }

            return await query
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .OrderByDescending(t => t.PriorityId)
                .ThenBy(t => t.DueDate)
                .ToListAsync();
        }

        public override async Task<Domain.Entities.WorkTask> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Creator)
                .Include(t => t.Assignee)
                .Include(t => t.Department)
                .Include(t => t.Product)
                .Include(t => t.Comments)
                   .ThenInclude(c => c.User)
                .Include(t => t.SubTasks)
                    .ThenInclude(s => s.Assignee)
                .Include(t => t.SubTasks)
                    .ThenInclude(s => s.Status)
                .FirstOrDefaultAsync(t => t.Id == id);
        }
    }
}
