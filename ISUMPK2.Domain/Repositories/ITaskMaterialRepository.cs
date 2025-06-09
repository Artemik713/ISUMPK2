using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Repositories
{
    public interface ITaskMaterialRepository : IRepository<TaskMaterial>
    {
        Task<IEnumerable<TaskMaterial>> GetByTaskIdAsync(Guid taskId);
        Task<IEnumerable<TaskMaterial>> GetByMaterialIdAsync(Guid materialId);
    }
}
