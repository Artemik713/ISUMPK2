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
        private readonly IDepartmentRepository _departmentRepository;
        private readonly IUserRepository _userRepository;

        public DepartmentService(IDepartmentRepository departmentRepository, IUserRepository userRepository)
        {
            _departmentRepository = departmentRepository;
            _userRepository = userRepository;
        }

        public async Task<DepartmentDto> GetDepartmentByIdAsync(Guid id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
                return null;

            return await MapToDtoAsync(department);
        }

        public async Task<IEnumerable<DepartmentDto>> GetAllDepartmentsAsync()
        {
            var departments = await _departmentRepository.GetAllAsync();
            var departmentDtos = new List<DepartmentDto>();

            foreach (var department in departments)
            {
                departmentDtos.Add(await MapToDtoAsync(department));
            }

            return departmentDtos;
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

            // Загружаем созданный департамент с руководителем
            var createdDepartment = await _departmentRepository.GetByIdAsync(department.Id);
            return await MapToDtoAsync(createdDepartment);
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

            // Загружаем обновленный департамент с руководителем
            var updatedDepartment = await _departmentRepository.GetByIdAsync(department.Id);
            return await MapToDtoAsync(updatedDepartment);
        }

        public async Task DeleteDepartmentAsync(Guid id)
        {
            await _departmentRepository.DeleteAsync(id);
            await _departmentRepository.SaveChangesAsync();
        }

        private async Task<DepartmentDto> MapToDtoAsync(Department department)
        {
            string headName = null;

            // Если есть HeadId, загружаем информацию о руководителе
            if (department.HeadId.HasValue)
            {
                try
                {
                    // Сначала проверяем навигационное свойство
                    if (department.Head != null)
                    {
                        // Формируем полное ФИО: Фамилия Имя Отчество
                        var lastName = department.Head.LastName ?? "";
                        var firstName = department.Head.FirstName ?? "";
                        var middleName = department.Head.MiddleName ?? "";

                        headName = $"{lastName} {firstName} {middleName}".Trim();

                        // Если все поля пустые, показываем userName
                        if (string.IsNullOrWhiteSpace(headName))
                        {
                            headName = department.Head.UserName ?? "Руководитель";
                        }
                    }
                    else
                    {
                        // Если навигационное свойство не загружено, делаем отдельный запрос
                        var head = await _userRepository.GetByIdAsync(department.HeadId.Value);
                        if (head != null)
                        {
                            var lastName = head.LastName ?? "";
                            var firstName = head.FirstName ?? "";
                            var middleName = head.MiddleName ?? "";

                            headName = $"{lastName} {firstName} {middleName}".Trim();

                            if (string.IsNullOrWhiteSpace(headName))
                            {
                                headName = head.UserName ?? "Руководитель";
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Если не удалось загрузить руководителя, показываем базовое сообщение
                    headName = "Ошибка загрузки данных";
                }
            }
            else
            {
                headName = "Не назначен";
            }

            return new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                Description = department.Description,
                HeadId = department.HeadId,
                HeadName = headName,
                CreatedAt = department.CreatedAt,
                UpdatedAt = department.UpdatedAt
            };
        }
    }
}