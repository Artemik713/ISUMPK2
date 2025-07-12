using ISUMPK2.Application.Services;
using ISUMPK2.Web.Models;
using ISUMPK2.Web.Extensions;

namespace ISUMPK2.Web.Services
{
    public class TaskProgressService : ITaskProgressService
    {
        private readonly ISubTaskService _subTaskService;

        public TaskProgressService(ISubTaskService subTaskService)
        {
            _subTaskService = subTaskService;
        }

        public async Task<int> CalculateTaskProgressAsync(Guid taskId)
        {
            try
            {
                var subTasksDto = await _subTaskService.GetByParentTaskIdAsync(taskId);
                var subTasks = subTasksDto.Select(dto => new SubTaskModel
                {
                    Id = dto.Id,
                    ParentTaskId = dto.ParentTaskId,
                    Title = dto.Title ?? string.Empty,
                    Description = dto.Description ?? string.Empty,
                    StatusId = dto.StatusId,
                    AssigneeId = dto.AssigneeId,
                    AssigneeName = dto.AssigneeName ?? string.Empty,
                    DueDate = dto.DueDate,
                    CompletedDate = dto.CompletedDate,
                    EstimatedHours = dto.EstimatedHours,
                    ActualHours = dto.ActualHours
                }).ToList();

                if (!subTasks.Any()) return 0;

                var completedCount = subTasks.Count(st => st.StatusId == 5);
                return (int)((double)completedCount / subTasks.Count * 100);
            }
            catch
            {
                return 0;
            }
        }

        public async Task<int> CalculateTaskProgressAsync(TaskModel task, List<SubTaskModel> subTasks)
        {
            // Если задача завершена, возвращаем 100%
            if (task.StatusId == 5) return 100;

            // Если задача только создана и нет подзадач
            if (task.StatusId == 1 && !subTasks.Any()) return 0;

            // Расчет на основе подзадач (приоритетный метод)
            if (subTasks.Any())
            {
                var totalSubTasks = subTasks.Count;
                var completedSubTasks = subTasks.Count(st => st.StatusId == 5);

                if (totalSubTasks == 0) return 0;

                var subTaskProgress = (int)((double)completedSubTasks / totalSubTasks * 100);

                // Если все подзадачи выполнены, но основная задача не завершена, показываем 99%
                return subTaskProgress == 100 && task.StatusId < 5 ? 99 : subTaskProgress;
            }

            // Резервный расчет на основе соотношения фактических и плановых часов
            if (task.ActualHours.HasValue && task.EstimatedHours.HasValue && task.EstimatedHours.Value > 0)
            {
                var progress = (int)((task.ActualHours.Value / task.EstimatedHours.Value) * 100);
                return Math.Min(progress, 99); // Не больше 99%, т.к. 100% - это завершенная задача
            }

            // Последний резерв - расчет на основе статуса
            return task.StatusId * 20; // Примерное соответствие статуса и прогресса
        }

        public async Task<Dictionary<Guid, SubTaskProgress>> CalculateTasksProgressAsync(List<TaskModel> tasks)
        {
            var progressDictionary = new Dictionary<Guid, SubTaskProgress>();

            foreach (var task in tasks)
            {
                try
                {
                    var subTasks = await _subTaskService.GetByParentTaskIdAsync(task.Id);
                    var subTasksList = subTasks.ToList();

                    if (subTasksList.Any())
                    {
                        var completedCount = subTasksList.Count(st => st.StatusId == 5);
                        progressDictionary[task.Id] = new SubTaskProgress
                        {
                            TotalCount = subTasksList.Count,
                            CompletedCount = completedCount
                        };
                    }
                }
                catch
                {
                    // В случае ошибки просто пропускаем эту задачу
                    continue;
                }
            }

            return progressDictionary;
        }
    }
}