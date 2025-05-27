using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Repositories
{
    public interface ISubTaskRepository : IRepository<SubTask>
    {
        Task<IEnumerable<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId);
        Task<IEnumerable<SubTask>> GetByAssigneeAsync(Guid assigneeId);
    }
}