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
    public class SubTaskRepository : Repository<SubTask>, ISubTaskRepository
    {
        public SubTaskRepository(ApplicationDbContext context) : base(context)
        {
        }

        public override async Task<SubTask> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Status)
                .Include(s => s.Assignee)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId)
        {
            return await _dbSet
                .Where(s => s.ParentTaskId == parentTaskId)
                .Include(s => s.Status)
                .Include(s => s.Assignee)
                .ToListAsync();
        }

        public async Task<IEnumerable<SubTask>> GetByAssigneeAsync(Guid assigneeId)
        {
            return await _dbSet
                .Where(s => s.AssigneeId == assigneeId)
                .Include(s => s.Status)
                .Include(s => s.ParentTask)
                .ToListAsync();
        }
    }
}