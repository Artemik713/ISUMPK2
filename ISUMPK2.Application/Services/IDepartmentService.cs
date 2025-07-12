using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services
{
    public interface IDepartmentService
    {
        Task<DepartmentDto> GetDepartmentByIdAsync(Guid id);
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto departmentDto);
        Task<DepartmentDto> UpdateDepartmentAsync(Guid id, DepartmentUpdateDto departmentDto);
        Task DeleteDepartmentAsync(Guid id);
    }
}