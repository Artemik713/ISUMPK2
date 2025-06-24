using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services
{
    public interface ITaskMaterialService
    {
        Task<TaskMaterialDto> GetByIdAsync(Guid id);
        Task<IEnumerable<TaskMaterialDto>> GetByTaskIdAsync(Guid taskId);
        Task<IEnumerable<TaskMaterialDto>> GetByMaterialIdAsync(Guid materialId);
        Task<TaskMaterialDto> CreateTaskMaterialAsync(TaskMaterialCreateDto createDto);
        Task<TaskMaterialDto> UpdateTaskMaterialAsync(Guid id, TaskMaterialUpdateDto updateDto);
        Task DeleteTaskMaterialAsync(Guid id);
    }
}