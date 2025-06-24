using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Application.Services
{
    public interface ITaskService
    {

        Task<TaskDto> GetTaskByIdAsync(Guid id);
        Task<IEnumerable<TaskDto>> GetActiveTasksAsync();
        Task<IEnumerable<TaskDto>> GetAllTasksAsync();
        Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(int statusId);
        Task<IEnumerable<TaskDto>> GetTasksByAssigneeAsync(Guid assigneeId);
        Task<IEnumerable<TaskDto>> GetTasksByCreatorAsync(Guid creatorId);
        Task<IEnumerable<TaskDto>> GetTasksByDepartmentAsync(Guid departmentId);
        Task<IEnumerable<TaskDto>> GetTasksByPriorityAsync(int priorityId);
        Task<IEnumerable<TaskDto>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<TaskDto>> GetTasksByProductAsync(Guid productId);
        Task<IEnumerable<TaskDto>> GetOverdueTasksAsync();
        Task<IEnumerable<TaskDto>> GetTasksForDashboardAsync(Guid userId);
        Task<TaskDto> CreateTaskAsync(Guid creatorId, TaskCreateDto taskDto);
        Task<TaskDto> UpdateTaskAsync(Guid id, TaskUpdateDto taskDto);
        Task<TaskDto> UpdateTaskStatusAsync(Guid id, Guid userId, TaskStatusUpdateDto statusDto);
        Task DeleteTaskAsync(Guid id);
        Task<TaskCommentDto> AddCommentAsync(Guid userId, TaskCommentCreateDto commentDto);
        Task<IEnumerable<TaskCommentDto>> GetCommentsByTaskIdAsync(Guid taskId);
        // Добавьте эти методы в интерфейс
        Task<IEnumerable<TaskMaterialDto>> GetTaskMaterialsAsync(Guid taskId);
        Task UpdateTaskMaterialsAsync(Guid taskId, List<TaskMaterialCreateDto> materials);
    }
}
