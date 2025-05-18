using ISUMPK2.Application.Services;
using ISUMPK2.Application.DTOs;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ITaskService = ISUMPK2.Application.Services.ITaskService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
using Microsoft.EntityFrameworkCore;
using ISUMPK2.Infrastructure.Data;


namespace ISUMPK2.Application.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;

        public TaskService(
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            ApplicationDbContext context)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _context = context;
        }

        public async Task<TaskDto> GetTaskByIdAsync(Guid id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                return null;

            return MapTaskToDto(task);
        }
        public async Task<IEnumerable<TaskDto>> GetActiveTasksAsync()
        {
            // Получаем все задачи
            var tasks = await _taskRepository.GetAllAsync();

            // Фильтруем активные задачи (не завершенные и не отмененные)
            // Предполагается, что StatusId 5 - "Завершено", 6 - "Отменено"
            var activeTasks = tasks.Where(t => t.StatusId != 5 && t.StatusId != 6);

            // Преобразуем в DTO и возвращаем
            return activeTasks.Select(MapTaskToDto);
        }
        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            var tasks = await _taskRepository.GetAllAsync();
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(int statusId)
        {
            var tasks = await _taskRepository.GetTasksByStatusAsync(statusId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByAssigneeAsync(Guid assigneeId)
        {
            var tasks = await _taskRepository.GetTasksByAssigneeAsync(assigneeId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByCreatorAsync(Guid creatorId)
        {
            var tasks = await _taskRepository.GetTasksByCreatorAsync(creatorId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByDepartmentAsync(Guid departmentId)
        {
            var tasks = await _taskRepository.GetTasksByDepartmentAsync(departmentId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByPriorityAsync(int priorityId)
        {
            var tasks = await _taskRepository.GetTasksByPriorityAsync(priorityId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var tasks = await _taskRepository.GetTasksByDueDateRangeAsync(startDate, endDate);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByProductAsync(Guid productId)
        {
            var tasks = await _taskRepository.GetTasksByProductAsync(productId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync()
        {
            var tasks = await _taskRepository.GetOverdueTasks();
            return tasks.Select(MapTaskToDto);
        }

        public async Task<IEnumerable<TaskDto>> GetTasksForDashboardAsync(Guid userId)
        {
            var tasks = await _taskRepository.GetTasksForDashboardAsync(userId);
            return tasks.Select(MapTaskToDto);
        }

        public async Task<TaskDto> CreateTaskAsync(Guid creatorId, TaskCreateDto taskDto)
        {
            var creator = await _userRepository.GetByIdAsync(creatorId);
            if (creator == null)
                throw new ApplicationException($"User with ID {creatorId} not found.");

            var task = new WorkTask
            {
                Id = Guid.NewGuid(),
                Title = taskDto.Title,
                Description = taskDto.Description,
                StatusId = taskDto.StatusId,
                PriorityId = taskDto.PriorityId,
                CreatorId = creatorId,
                AssigneeId = taskDto.AssigneeId,
                DepartmentId = taskDto.DepartmentId,
                StartDate = taskDto.StartDate,
                DueDate = taskDto.DueDate,
                EstimatedHours = taskDto.EstimatedHours,
                ProductId = taskDto.ProductId,
                Quantity = taskDto.Quantity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _taskRepository.AddAsync(task);
            await _taskRepository.SaveChangesAsync();

            // Если назначен исполнитель, отправляем уведомление
            if (task.AssigneeId.HasValue)
            {
                await _notificationService.CreateTaskAssignedNotificationAsync(task.Id, task.AssigneeId.Value);
            }

            var createdTask = await _taskRepository.GetByIdAsync(task.Id);
            return MapTaskToDto(createdTask);
        }

        public async Task<TaskDto> UpdateTaskAsync(Guid id, TaskUpdateDto taskDto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new ApplicationException($"Task with ID {id} not found.");

            // Сохраняем предыдущий статус и исполнителя для проверки изменений
            var previousStatusId = task.StatusId;
            var previousAssigneeId = task.AssigneeId;

            // Обновляем задачу
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.StatusId = taskDto.StatusId;
            task.PriorityId = taskDto.PriorityId;
            task.AssigneeId = taskDto.AssigneeId;
            task.DepartmentId = taskDto.DepartmentId;
            task.StartDate = taskDto.StartDate;
            task.DueDate = taskDto.DueDate;
            task.CompletedDate = taskDto.CompletedDate;
            task.EstimatedHours = taskDto.EstimatedHours;
            task.ActualHours = taskDto.ActualHours;
            task.ProductId = taskDto.ProductId;
            task.Quantity = taskDto.Quantity;
            task.IsForceMarked = taskDto.IsForceMarked;
            task.UpdatedAt = DateTime.UtcNow;

            // Если задача выполнена и не указана дата завершения, устанавливаем её
            if (task.StatusId == 5 && !task.CompletedDate.HasValue)
            {
                task.CompletedDate = DateTime.UtcNow;
            }

            await _taskRepository.UpdateAsync(task);
            await _taskRepository.SaveChangesAsync();

            // Если изменился статус, отправляем уведомление
            if (previousStatusId != task.StatusId)
            {
                await _notificationService.CreateTaskStatusChangedNotificationAsync(task.Id, previousStatusId, task.StatusId);
            }

            // Если изменился исполнитель и новый исполнитель назначен, отправляем уведомление
            if (previousAssigneeId != task.AssigneeId && task.AssigneeId.HasValue)
            {
                await _notificationService.CreateTaskAssignedNotificationAsync(task.Id, task.AssigneeId.Value);
            }

            var updatedTask = await _taskRepository.GetByIdAsync(id);
            return MapTaskToDto(updatedTask);
        }

        public async Task<TaskDto> UpdateTaskStatusAsync(Guid id, Guid userId, TaskStatusUpdateDto statusDto)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new ApplicationException($"Task with ID {id} not found.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ApplicationException($"User with ID {userId} not found.");

            // Проверяем, имеет ли пользователь право изменять статус задачи
            bool canUpdateStatus = task.AssigneeId == userId || task.CreatorId == userId;

            // Проверяем роли пользователя
            var userRoles = await _userRepository.GetRolesAsync(userId);
            bool isManager = userRoles.Any(r => r == "GeneralDirector" || r == "MetalShopManager" || r == "PaintShopManager");

            if (!canUpdateStatus && !isManager)
                throw new ApplicationException("You don't have permission to update this task's status.");

            // Сохраняем предыдущий статус для уведомления
            var previousStatusId = task.StatusId;

            // Обновляем статус
            task.StatusId = statusDto.StatusId;
            task.UpdatedAt = DateTime.UtcNow;

            // Если задача выполнена и не указана дата завершения, устанавливаем её
            if (task.StatusId == 5 && !task.CompletedDate.HasValue)
            {
                task.CompletedDate = DateTime.UtcNow;
            }

            await _taskRepository.UpdateAsync(task);

            // Добавляем комментарий, если он есть
            if (!string.IsNullOrEmpty(statusDto.Comment))
            {
                var comment = new TaskComment
                {
                    Id = Guid.NewGuid(),
                    TaskId = id,
                    UserId = userId,
                    Comment = statusDto.Comment,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                task.Comments.Add(comment);
            }

            await _taskRepository.SaveChangesAsync();

            // Отправляем уведомление об изменении статуса
            await _notificationService.CreateTaskStatusChangedNotificationAsync(task.Id, previousStatusId, task.StatusId);

            var updatedTask = await _taskRepository.GetByIdAsync(id);
            return MapTaskToDto(updatedTask);
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new ApplicationException($"Task with ID {id} not found.");

            await _taskRepository.DeleteAsync(id);
            await _taskRepository.SaveChangesAsync();
        }

        public async Task<TaskCommentDto> AddCommentAsync(Guid userId, TaskCommentCreateDto commentDto)
        {
            var task = await _taskRepository.GetByIdAsync(commentDto.TaskId);
            if (task == null)
                throw new ApplicationException($"Task with ID {commentDto.TaskId} not found.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new ApplicationException($"User with ID {userId} not found.");

            var comment = new TaskComment
            {
                Id = Guid.NewGuid(),
                TaskId = commentDto.TaskId,
                UserId = userId,
                Comment = commentDto.Comment,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();


            return new TaskCommentDto
            {
                Id = comment.Id,
                TaskId = comment.TaskId,
                UserId = comment.UserId,
                UserName = $"{user.FirstName} {user.LastName}",
                Comment = comment.Comment,
                CreatedAt = comment.CreatedAt
            };
        }

        public async Task<IEnumerable<TaskCommentDto>> GetCommentsByTaskIdAsync(Guid taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new ApplicationException($"Task with ID {taskId} not found.");

            return task.Comments.Select(c => new TaskCommentDto
            {
                Id = c.Id,
                TaskId = c.TaskId,
                UserId = c.UserId,
                UserName = $"{c.User.FirstName} {c.User.LastName}",
                Comment = c.Comment,
                CreatedAt = c.CreatedAt
            }).OrderByDescending(c => c.CreatedAt);
        }

        private TaskDto MapTaskToDto(WorkTask task)
        {
            string assigneeName = null;
            if (task.Assignee != null)
            {
                assigneeName = $"{task.Assignee.FirstName} {task.Assignee.LastName}";
                Console.WriteLine($"Найден исполнитель: {assigneeName} для задачи {task.Id}");
            }
            else if (task.AssigneeId.HasValue)
            {
                Console.WriteLine($"Для задачи {task.Id} указан AssigneeId={task.AssigneeId}, но объект Assignee не загружен");
                assigneeName = "Не назначен";
            }
            else
            {
                Console.WriteLine($"Для задачи {task.Id} не указан AssigneeId");
                assigneeName = "Не назначен";
            }
            return new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                StatusId = task.StatusId,
                StatusName = task.Status?.Name,
                PriorityId = task.PriorityId,
                PriorityName = task.Priority?.Name,
                CreatorId = task.CreatorId,
                CreatorName = task.Creator != null ? $"{task.Creator.FirstName} {task.Creator.LastName}" : null,
                AssigneeId = task.AssigneeId,
                AssigneeName = assigneeName,
                DepartmentId = task.DepartmentId,
                DepartmentName = task.Department?.Name,
                StartDate = task.StartDate,
                DueDate = task.DueDate,
                CompletedDate = task.CompletedDate,
                EstimatedHours = task.EstimatedHours,
                ActualHours = task.ActualHours,
                ProductId = task.ProductId,
                ProductName = task.Product?.Name,
                Quantity = task.Quantity,
                IsForceMarked = task.IsForceMarked,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                Comments = task.Comments?.Select(c => new TaskCommentDto
                {
                    Id = c.Id,
                    TaskId = c.TaskId,
                    UserId = c.UserId,
                    UserName = c.User != null ? $"{c.User.FirstName} {c.User.LastName}" : null,
                    Comment = c.Comment,
                    CreatedAt = c.CreatedAt
                }).OrderByDescending(c => c.CreatedAt).ToList() ?? new List<TaskCommentDto>()
            };
        }
    }
}
