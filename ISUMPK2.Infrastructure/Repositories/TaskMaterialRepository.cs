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
    public class TaskMaterialRepository : Repository<TaskMaterial>, ITaskMaterialRepository
    {
        public TaskMaterialRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<TaskMaterial>> GetByTaskIdAsync(Guid taskId)
        {
            return await _dbSet
                .Include(tm => tm.Material)
                .Where(tm => tm.TaskId == taskId)
                .ToListAsync();
        }

        public async Task<IEnumerable<TaskMaterial>> GetByMaterialIdAsync(Guid materialId)
        {
            return await _dbSet
                .Include(tm => tm.WorkTask)
                .Where(tm => tm.MaterialId == materialId)
                .ToListAsync();
        }
    }
}