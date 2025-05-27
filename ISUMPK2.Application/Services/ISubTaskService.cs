using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services
{
    public interface ISubTaskService
    {
        Task<SubTaskDto> GetSubTaskByIdAsync(Guid id);
        Task<IEnumerable<SubTaskDto>> GetAllSubTasksAsync(); // Добавленный метод
        Task<IEnumerable<SubTaskDto>> GetByParentTaskIdAsync(Guid parentTaskId);
        Task<IEnumerable<SubTaskDto>> GetByAssigneeAsync(Guid assigneeId); // Добавленный метод
        Task<SubTaskDto> CreateSubTaskAsync(SubTaskCreateDto subTaskDto);
        Task<SubTaskDto> UpdateSubTaskAsync(Guid id, SubTaskUpdateDto subTaskDto);
        Task DeleteSubTaskAsync(Guid id);
    }
}