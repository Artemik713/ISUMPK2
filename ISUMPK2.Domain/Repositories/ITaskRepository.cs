using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Domain.Repositories
{
    public interface ITaskRepository : IRepository<WorkTask>
    {
        Task<IEnumerable<WorkTask>> GetTasksByStatusAsync(int statusId);
        Task<IEnumerable<WorkTask>> GetTasksByAssigneeAsync(Guid assigneeId);
        Task<IEnumerable<WorkTask>> GetTasksByCreatorAsync(Guid creatorId);
        Task<IEnumerable<WorkTask>> GetTasksByDepartmentAsync(Guid departmentId);
        Task<IEnumerable<WorkTask>> GetTasksByPriorityAsync(int priorityId);
        Task<IEnumerable<WorkTask>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<WorkTask>> GetTasksByProductAsync(Guid productId);
        Task<IEnumerable<WorkTask>> GetOverdueTasks();
        Task<IEnumerable<WorkTask>> GetTasksForDashboardAsync(Guid userId);
    }
}
