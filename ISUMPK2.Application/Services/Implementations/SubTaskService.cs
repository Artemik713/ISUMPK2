using ISUMPK2.Application.DTOs;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services.Implementations
{
    public class SubTaskService : ISubTaskService
    {
        private readonly ISubTaskRepository _subTaskRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;

        public SubTaskService(
        ISubTaskRepository subTaskRepository,
        IUserRepository userRepository,
        ITaskRepository taskRepository) // Добавляем в конструктор
        {
            _subTaskRepository = subTaskRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
        }

        public async Task<SubTaskDto> CreateSubTaskAsync(SubTaskCreateDto subTaskDto)
        {
            try
            {
                // Проверка на существование родительской задачи через репозиторий
                var parentTask = await _taskRepository.GetByIdAsync(subTaskDto.ParentTaskId);
                if (parentTask == null)
                    throw new InvalidOperationException($"Родительская задача с ID {subTaskDto.ParentTaskId} не найдена");

                var subTask = new SubTask
                {
                    Id = Guid.NewGuid(),
                    ParentTaskId = subTaskDto.ParentTaskId,
                    Title = subTaskDto.Title,
                    Description = subTaskDto.Description,
                    StatusId = subTaskDto.StatusId,
                    AssigneeId = subTaskDto.AssigneeId,
                    DueDate = subTaskDto.DueDate,
                    EstimatedHours = subTaskDto.EstimatedHours,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _subTaskRepository.AddAsync(subTask);
                await _subTaskRepository.SaveChangesAsync();

                return await GetSubTaskByIdAsync(subTask.Id);
            }
            catch (Exception ex)
            {
                // Логирование ошибки
                Console.WriteLine($"Ошибка при создании подзадачи: {ex.Message}");
                Console.WriteLine($"Стек вызовов: {ex.StackTrace}");

                // Проблема может быть в нарушении ограничений внешнего ключа
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner exception: {ex.InnerException.Message}");

                throw; // Перебрасываем исключение для обработки в контроллере
            }
        }

        public async Task DeleteSubTaskAsync(Guid id)
        {
            await _subTaskRepository.DeleteAsync(id);
            await _subTaskRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<SubTaskDto>> GetByParentTaskIdAsync(Guid parentTaskId)
        {
            var tasks = await _subTaskRepository.FindAsync(s => s.ParentTaskId == parentTaskId);
            var result = new List<SubTaskDto>();

            foreach (var task in tasks)
            {
                var dto = new SubTaskDto
                {
                    Id = task.Id,
                    ParentTaskId = task.ParentTaskId,
                    Title = task.Title,
                    Description = task.Description,
                    StatusId = task.StatusId,
                    StatusName = GetStatusName(task.StatusId),
                    AssigneeId = task.AssigneeId,
                    DueDate = task.DueDate,
                    CompletedDate = task.CompletedDate,
                    EstimatedHours = task.EstimatedHours,
                    ActualHours = task.ActualHours
                };

                if (task.AssigneeId.HasValue)
                {
                    var assignee = await _userRepository.GetByIdAsync(task.AssigneeId.Value);
                    if (assignee != null)
                    {
                        dto.AssigneeName = $"{assignee.FirstName} {assignee.LastName}";
                    }
                }

                result.Add(dto); // Используйте переменную result вместо dtos
            }

            return result;
        }

        public async Task<IEnumerable<SubTaskDto>> GetAllSubTasksAsync()
        {
            var subTasks = await _subTaskRepository.GetAllAsync();
            var dtos = new List<SubTaskDto>();

            foreach (var subTask in subTasks)
            {
                var dto = new SubTaskDto
                {
                    Id = subTask.Id,
                    ParentTaskId = subTask.ParentTaskId,
                    Title = subTask.Title,
                    Description = subTask.Description,
                    StatusId = subTask.StatusId,
                    StatusName = GetStatusName(subTask.StatusId),
                    AssigneeId = subTask.AssigneeId,
                    DueDate = subTask.DueDate,
                    CompletedDate = subTask.CompletedDate,
                    EstimatedHours = subTask.EstimatedHours,
                    ActualHours = subTask.ActualHours
                };

                if (subTask.AssigneeId.HasValue)
                {
                    var assignee = await _userRepository.GetByIdAsync(subTask.AssigneeId.Value);
                    if (assignee != null)
                    {
                        dto.AssigneeName = $"{assignee.FirstName} {assignee.LastName}";
                    }
                }

                dtos.Add(dto);
            }

            return dtos;
        }

        public async Task<IEnumerable<SubTaskDto>> GetByAssigneeAsync(Guid assigneeId)
        {
            var subTasks = await _subTaskRepository.GetByAssigneeAsync(assigneeId);
            var result = new List<SubTaskDto>(); // Переименовать переменную dtos в result

            foreach (var subTask in subTasks)
            {
                var dto = new SubTaskDto
                {
                    Id = subTask.Id,
                    ParentTaskId = subTask.ParentTaskId,
                    Title = subTask.Title,
                    Description = subTask.Description,
                    StatusId = subTask.StatusId,
                    StatusName = GetStatusName(subTask.StatusId),
                    AssigneeId = subTask.AssigneeId,
                    DueDate = subTask.DueDate,
                    CompletedDate = subTask.CompletedDate,
                    EstimatedHours = subTask.EstimatedHours,
                    ActualHours = subTask.ActualHours
                };

                if (subTask.AssigneeId.HasValue)
                {
                    var assignee = await _userRepository.GetByIdAsync(subTask.AssigneeId.Value);
                    if (assignee != null)
                    {
                        dto.AssigneeName = $"{assignee.FirstName} {assignee.LastName}";
                    }
                }

                result.Add(dto); // Используйте переменную result вместо dtos
            }

            return result;
        }
        public async Task<SubTaskDto> GetSubTaskByIdAsync(Guid id)
        {
            var subTask = await _subTaskRepository.GetByIdAsync(id);
            if (subTask == null)
                return null;

            var dto = new SubTaskDto
            {
                Id = subTask.Id,
                ParentTaskId = subTask.ParentTaskId,
                Title = subTask.Title,
                Description = subTask.Description,
                StatusId = subTask.StatusId,
                StatusName = GetStatusName(subTask.StatusId),
                AssigneeId = subTask.AssigneeId,
                DueDate = subTask.DueDate,
                CompletedDate = subTask.CompletedDate,
                EstimatedHours = subTask.EstimatedHours,
                ActualHours = subTask.ActualHours
            };

            if (subTask.AssigneeId.HasValue)
            {
                var assignee = await _userRepository.GetByIdAsync(subTask.AssigneeId.Value);
                if (assignee != null)
                {
                    dto.AssigneeName = $"{assignee.FirstName} {assignee.LastName}";
                }
            }

            return dto;
        }

        public async Task<SubTaskDto> UpdateSubTaskAsync(Guid id, SubTaskUpdateDto subTaskDto)
        {
            var subTask = await _subTaskRepository.GetByIdAsync(id);
            if (subTask == null)
                throw new Exception($"SubTask with ID {id} not found");

            subTask.Title = subTaskDto.Title;
            subTask.Description = subTaskDto.Description;
            subTask.StatusId = subTaskDto.StatusId;
            subTask.AssigneeId = subTaskDto.AssigneeId;
            subTask.DueDate = subTaskDto.DueDate;
            subTask.EstimatedHours = subTaskDto.EstimatedHours;
            subTask.UpdatedAt = DateTime.UtcNow;

            await _subTaskRepository.UpdateAsync(subTask);
            await _subTaskRepository.SaveChangesAsync();

            return await GetSubTaskByIdAsync(id);
        }

        private string GetStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Создана",
                2 => "В работе",
                3 => "Требует уточнения",
                4 => "На проверке",
                5 => "Выполнена",
                6 => "Отклонена",
                _ => $"Статус {statusId}"
            };
        }
    }
}