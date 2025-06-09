using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public interface IClientTaskMaterialService
    {
        Task<IEnumerable<TaskMaterialDto>> GetByTaskIdAsync(Guid taskId);
        Task<IEnumerable<TaskMaterialDto>> GetByMaterialIdAsync(Guid materialId);
        Task<TaskMaterialDto> CreateAsync(TaskMaterialCreateDto createDto);
        Task<TaskMaterialDto> UpdateAsync(Guid id, TaskMaterialUpdateDto updateDto);
        Task DeleteAsync(Guid id);
    }
}