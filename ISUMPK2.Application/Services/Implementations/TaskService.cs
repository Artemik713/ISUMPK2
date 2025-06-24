using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using ISUMPK2.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ITaskService = ISUMPK2.Application.Services.ITaskService;
using Task = System.Threading.Tasks.Task;


namespace ISUMPK2.Application.Services.Implementations
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly ApplicationDbContext _context;
        private readonly IMaterialRepository _materialRepository;

        public TaskService(
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IMaterialRepository materialRepository,
            ApplicationDbContext context)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _materialRepository = materialRepository;
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

        public async Task UpdateTaskMaterialsAsync(Guid taskId, List<TaskMaterialCreateDto> materials)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new ApplicationException($"Task with ID {taskId} not found.");

            // Используем транзакцию для согласованного изменения данных
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Удаляем все существующие материалы для задачи
                var existingMaterials = await _context.TaskMaterials
                    .Where(tm => tm.TaskId == taskId)
                    .ToListAsync();
                if (existingMaterials.Any())
                    _context.TaskMaterials.RemoveRange(existingMaterials);

                // Добавляем новые материалы
                foreach (var materialDto in materials)
                {
                    var taskMaterial = new TaskMaterial
                    {
                        Id = Guid.NewGuid(),
                        TaskId = taskId,
                        MaterialId = materialDto.MaterialId,
                        Quantity = materialDto.Quantity,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    await _context.TaskMaterials.AddAsync(taskMaterial);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ApplicationException($"Error updating task materials: {ex.Message}", ex);
            }
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

            // --- Удаляем проверку зависимостей ---

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1. Удаляем все связанные подзадачи
                var subTasks = await _context.SubTasks
                    .Where(st => st.ParentTaskId == id)
                    .ToListAsync();
                if (subTasks.Any())
                    _context.SubTasks.RemoveRange(subTasks);

                // 2. Удаляем все связанные уведомления
                var notifications = await _context.Notifications
                    .Where(n => n.TaskId == id)
                    .ToListAsync();
                if (notifications.Any())
                    _context.Notifications.RemoveRange(notifications);

                // 3. Удаляем комментарии задачи
                var comments = await _context.TaskComments
                    .Where(c => c.TaskId == id)
                    .ToListAsync();
                if (comments.Any())
                    _context.TaskComments.RemoveRange(comments);

                // 4. Удаляем транзакции материалов
                var materialTransactions = await _context.MaterialTransactions
                    .Where(mt => mt.TaskId == id)
                    .ToListAsync();
                if (materialTransactions.Any())
                    _context.MaterialTransactions.RemoveRange(materialTransactions);

                // 5. Удаляем транзакции продуктов
                var productTransactions = await _context.ProductTransactions
                    .Where(pt => pt.TaskId == id)
                    .ToListAsync();
                if (productTransactions.Any())
                    _context.ProductTransactions.RemoveRange(productTransactions);

                // 6. Удаляем саму задачу
                await _taskRepository.DeleteAsync(id);

                // 7. Сохраняем все изменения
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new ApplicationException($"Error deleting task: {ex.Message}", ex);
            }
        }

        // Добавляем новый метод для проверки зависимостей задачи
        // Исправленный метод для проверки зависимостей задачи
        // Исправленный метод для проверки зависимостей задачи
        private async Task<List<string>> CheckTaskDependencies(Guid taskId)
        {
            var dependencies = new List<string>();

            // Проверяем наличие подзадач (без анализа их статуса)
            var subTasksCount = await _context.SubTasks
                .Where(t => t.ParentTaskId == taskId)
                .CountAsync();

            if (subTasksCount > 0)
            {
                dependencies.Add($"Задача имеет {subTasksCount} связанных подзадач");
            }

            // Проверяем наличие связанных транзакций продуктов
            var productTransactionsCount = await _context.ProductTransactions
                .Where(pt => pt.TaskId == taskId)
                .CountAsync();

            if (productTransactionsCount > 0)
            {
                dependencies.Add($"Задача имеет {productTransactionsCount} связанных транзакций продуктов");
            }

            // Проверяем наличие связанных транзакций материалов
            var materialTransactionsCount = await _context.MaterialTransactions
                .Where(mt => mt.TaskId == taskId)
                .CountAsync();

            if (materialTransactionsCount > 0)
            {
                dependencies.Add($"Задача имеет {materialTransactionsCount} связанных транзакций материалов");
            }

            return dependencies;
        }

        public async Task<IEnumerable<TaskMaterialDto>> GetTaskMaterialsAsync(Guid taskId)
        {
            try
            {
                Console.WriteLine($"TaskService: Запрос материалов для задачи {taskId}");

                // Получаем задачу вместе с материалами
                var task = await _taskRepository.GetTaskWithMaterialsAsync(taskId);

                if (task == null)
                {
                    Console.WriteLine("Задача не найдена");
                    return Enumerable.Empty<TaskMaterialDto>();
                }

                var result = task.TaskMaterials?
                    .Select(tm => new TaskMaterialDto
                    {
                        Id = tm.Id,
                        TaskId = taskId,
                        MaterialId = tm.MaterialId,
                        MaterialName = tm.Material?.Name ?? "Материал не найден",
                        MaterialCode = tm.Material?.Code ?? "",
                        Quantity = tm.Quantity,
                        UnitOfMeasure = tm.Material?.UnitOfMeasure ?? "шт."
                    })
                    .ToList() ?? new List<TaskMaterialDto>();

                Console.WriteLine($"TaskService: Найдено {result.Count} материалов");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TaskService: Ошибка при получении материалов задачи: {ex.Message}");
                throw;
            }
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
