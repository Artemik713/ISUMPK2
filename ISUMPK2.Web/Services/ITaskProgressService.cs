using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public interface ITaskProgressService
    {
        Task<int> CalculateTaskProgressAsync(Guid taskId);
        Task<int> CalculateTaskProgressAsync(TaskModel task, List<SubTaskModel> subTasks);
        Task<Dictionary<Guid, SubTaskProgress>> CalculateTasksProgressAsync(List<TaskModel> tasks);
    }

    public class SubTaskProgress
    {
        public int TotalCount { get; set; }
        public int CompletedCount { get; set; }
        public int Percentage => TotalCount > 0 ? (int)((double)CompletedCount / TotalCount * 100) : 0;
    }
}