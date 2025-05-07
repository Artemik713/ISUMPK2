using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services
{
    public class DepartmentDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? HeadId { get; set; }
        public string HeadName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class DepartmentCreateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? HeadId { get; set; }
    }

    public class DepartmentUpdateDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid? HeadId { get; set; }
    }

    public interface IDepartmentService
    {
        Task<DepartmentDto> GetDepartmentByIdAsync(Guid id);
        Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync();
        Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto departmentDto);
        Task<DepartmentDto> UpdateDepartmentAsync(Guid id, DepartmentUpdateDto departmentDto);
        Task DeleteDepartmentAsync(Guid id);
    }
}
