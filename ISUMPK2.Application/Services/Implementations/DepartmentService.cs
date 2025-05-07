using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services.Implementations
{
    public class DepartmentService : IDepartmentService
    {
        private readonly IRepository<Department> _departmentRepository;
        private readonly IUserRepository _userRepository;

        public DepartmentService(IRepository<Department> departmentRepository, IUserRepository userRepository)
        {
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            return department != null ? MapToDto(department) : null;
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return departments.Select(MapToDto);
        }

        public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentCreateDto departmentDto)
        {
            var department = new Department
            {
                Name = departmentDto.Name,
                Description = departmentDto.Description,
                HeadId = departmentDto.HeadId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _departmentRepository.AddAsync(department);
            await _departmentRepository.SaveChangesAsync();

            return MapToDto(department);
        }

        public async Task<DepartmentDto> UpdateDepartmentAsync(Guid id, DepartmentUpdateDto departmentDto)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return null;

            department.Name = departmentDto.Name;
            department.Description = departmentDto.Description;
            department.HeadId = departmentDto.HeadId;
            department.UpdatedAt = DateTime.UtcNow;

            await _departmentRepository.UpdateAsync(department);
            await _departmentRepository.SaveChangesAsync();

            return MapToDto(department);
        }

        public async Task DeleteDepartmentAsync(Guid id)
        {
            await _departmentRepository.DeleteAsync(id);
            await _departmentRepository.SaveChangesAsync();
        }

        private DepartmentDto MapToDto(Department department)
        {
            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                HeadId = department.HeadId,
                HeadName = department.Head?.FirstName + " " + department.Head?.LastName,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };
        }
    }
}
