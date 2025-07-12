using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using ISUMPK2.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<TaskService> _logger;

        public TaskService(
            ITaskRepository taskRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IMaterialRepository materialRepository,
            ApplicationDbContext context,
            ILogger<TaskService> logger = null)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _materialRepository = materialRepository;
            _context = context;
            _logger = logger;
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

        public async Task ReserveMaterialsAsync(Guid taskId, List<TaskMaterialCreateDto> materials)
        {
            if (materials == null || !materials.Any())
            {
                throw new ApplicationException("Список материалов не может быть пустым");
            }

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                throw new ApplicationException($"Задача с ID {taskId} не найдена");
            }

            // Используем транзакцию для согласованного обновления запасов
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine($"Резервирование материалов для задачи {taskId}, количество материалов: {materials.Count}");

                foreach (var materialDto in materials)
                {
                    // Получаем данные о материале
                    var material = await _materialRepository.GetByIdAsync(materialDto.MaterialId);
                    if (material == null)
                    {
                        throw new ApplicationException($"Материал с ID {materialDto.MaterialId} не найден");
                    }

                    if (material.CurrentStock < materialDto.Quantity)
                    {
                        throw new ApplicationException($"Недостаточно материала {material.Name} на складе. Доступно: {material.CurrentStock} {material.UnitOfMeasure}");
                    }

                    // Уменьшаем запас материала
                    material.CurrentStock -= materialDto.Quantity;
                    await _materialRepository.UpdateAsync(material);

                    // Создаем транзакцию расхода материала
                    var materialTransaction = new MaterialTransaction
                    {
                        Id = Guid.NewGuid(),
                        MaterialId = materialDto.MaterialId,
                        Quantity = materialDto.Quantity,
                        TransactionType = "Issue", // Расход
                        TaskId = taskId,
                        Notes = $"Зарезервировано для задачи #{task.Title}",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    // Добавляем транзакцию в базу
                    await _context.MaterialTransactions.AddAsync(materialTransaction);

                    Console.WriteLine($"Зарезервирован материал {material.Name}, количество: {materialDto.Quantity}");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                Console.WriteLine($"Резервирование материалов для задачи {taskId} успешно завершено");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Ошибка при резервировании материалов: {ex.Message}");
                throw new ApplicationException($"Ошибка при резервировании материалов: {ex.Message}", ex);
            }
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

                Console.WriteLine($"Найдено {existingMaterials.Count} существующих материалов для задачи {taskId}");

                if (existingMaterials.Any())
                    _context.TaskMaterials.RemoveRange(existingMaterials);

                // Добавляем новые материалы
                Console.WriteLine($"Добавление {materials.Count} новых материалов для задачи {taskId}");
                foreach (var materialDto in materials)
                {
                    // Проверяем, что есть материал с указанным ID
                    var material = await _materialRepository.GetByIdAsync(materialDto.MaterialId);
                    if (material == null)
                    {
                        Console.WriteLine($"Материал с ID {materialDto.MaterialId} не найден");
                        continue;
                    }

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
                    Console.WriteLine($"Добавлен материал {materialDto.MaterialId}, количество: {materialDto.Quantity}");
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                Console.WriteLine($"Транзакция успешно завершена для задачи {taskId}");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Ошибка при обновлении материалов: {ex.Message}");
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

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _logger?.LogInformation("=== НАЧАЛО УДАЛЕНИЯ ЗАДАЧИ {TaskId} ===", id);

                // 1. Удаляем все связанные подзадачи
                _logger?.LogInformation("Checking and deleting subtasks...");
                var subTasks = await _context.SubTasks
                    .Where(st => st.ParentTaskId == id)
                    .ToListAsync();

                if (subTasks.Any())
                {
                    _logger?.LogInformation("Deleting {Count} subtasks for task {TaskId}", subTasks.Count, id);
                    _context.SubTasks.RemoveRange(subTasks);
                    await _context.SaveChangesAsync(); // Сохраняем после каждого этапа
                    _logger?.LogInformation("Subtasks deleted successfully");
                }

                // 2. Удаляем связи задачи с материалами (TaskMaterials)
                _logger?.LogInformation("Checking and deleting task materials...");
                var taskMaterials = await _context.TaskMaterials
                    .Where(tm => tm.TaskId == id)
                    .ToListAsync();

                if (taskMaterials.Any())
                {
                    _logger?.LogInformation("Deleting {Count} task materials for task {TaskId}", taskMaterials.Count, id);
                    _context.TaskMaterials.RemoveRange(taskMaterials);
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("Task materials deleted successfully");
                }

                // 3. Удаляем транзакции материалов
                _logger?.LogInformation("Checking and deleting material transactions...");
                var materialTransactions = await _context.MaterialTransactions
                    .Where(mt => mt.TaskId == id)
                    .ToListAsync();

                if (materialTransactions.Any())
                {
                    _logger?.LogInformation("Deleting {Count} material transactions for task {TaskId}", materialTransactions.Count, id);
                    _context.MaterialTransactions.RemoveRange(materialTransactions);
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("Material transactions deleted successfully");
                }

                // 4. Удаляем транзакции продуктов
                _logger?.LogInformation("Checking and deleting product transactions...");
                var productTransactions = await _context.ProductTransactions
                    .Where(pt => pt.TaskId == id)
                    .ToListAsync();

                if (productTransactions.Any())
                {
                    _logger?.LogInformation("Deleting {Count} product transactions for task {TaskId}", productTransactions.Count, id);
                    _context.ProductTransactions.RemoveRange(productTransactions);
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("Product transactions deleted successfully");
                }

                // 5. Удаляем комментарии задачи
                _logger?.LogInformation("Checking and deleting task comments...");
                var comments = await _context.TaskComments
                    .Where(c => c.TaskId == id)
                    .ToListAsync();

                if (comments.Any())
                {
                    _logger?.LogInformation("Deleting {Count} comments for task {TaskId}", comments.Count, id);
                    _context.TaskComments.RemoveRange(comments);
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("Comments deleted successfully");
                }

                // 6. Удаляем все связанные уведомления
                _logger?.LogInformation("Checking and deleting notifications...");
                var notifications = await _context.Notifications
                    .Where(n => n.TaskId == id)
                    .ToListAsync();

                if (notifications.Any())
                {
                    _logger?.LogInformation("Deleting {Count} notifications for task {TaskId}", notifications.Count, id);
                    _context.Notifications.RemoveRange(notifications);
                    await _context.SaveChangesAsync();
                    _logger?.LogInformation("Notifications deleted successfully");
                }

                // 7. Проверяем, есть ли еще связанные записи, которые мы могли пропустить
                _logger?.LogInformation("Checking for any remaining foreign key references...");

                // Проверим, есть ли еще записи, ссылающиеся на эту задачу
                var remainingSubTasks = await _context.SubTasks.CountAsync(st => st.ParentTaskId == id);
                var remainingTaskMaterials = await _context.TaskMaterials.CountAsync(tm => tm.TaskId == id);
                var remainingMaterialTransactions = await _context.MaterialTransactions.CountAsync(mt => mt.TaskId == id);
                var remainingProductTransactions = await _context.ProductTransactions.CountAsync(pt => pt.TaskId == id);
                var remainingComments = await _context.TaskComments.CountAsync(c => c.TaskId == id);
                var remainingNotifications = await _context.Notifications.CountAsync(n => n.TaskId == id);

                _logger?.LogInformation("Remaining references: SubTasks={SubTasks}, TaskMaterials={TaskMaterials}, " +
                                       "MaterialTransactions={MaterialTransactions}, ProductTransactions={ProductTransactions}, " +
                                       "Comments={Comments}, Notifications={Notifications}",
                                       remainingSubTasks, remainingTaskMaterials, remainingMaterialTransactions,
                                       remainingProductTransactions, remainingComments, remainingNotifications);

                if (remainingSubTasks > 0 || remainingTaskMaterials > 0 || remainingMaterialTransactions > 0 ||
                    remainingProductTransactions > 0 || remainingComments > 0 || remainingNotifications > 0)
                {
                    throw new ApplicationException($"Cannot delete task: still has {remainingSubTasks} subtasks, " +
                                                 $"{remainingTaskMaterials} task materials, {remainingMaterialTransactions} material transactions, " +
                                                 $"{remainingProductTransactions} product transactions, {remainingComments} comments, " +
                                                 $"{remainingNotifications} notifications");
                }

                // 8. Удаляем саму задачу
                _logger?.LogInformation("Deleting the task itself...");
                await _taskRepository.DeleteAsync(id);
                await _context.SaveChangesAsync();
                _logger?.LogInformation("Task {TaskId} deleted successfully", id);

                await transaction.CommitAsync();
                _logger?.LogInformation("=== УДАЛЕНИЕ ЗАДАЧИ {TaskId} ЗАВЕРШЕНО УСПЕШНО ===", id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger?.LogError(ex, "=== ОШИБКА ПРИ УДАЛЕНИИ ЗАДАЧИ {TaskId} ===", id);
                _logger?.LogError("Exception details: {ExceptionType}: {Message}", ex.GetType().Name, ex.Message);
                _logger?.LogError("Stack trace: {StackTrace}", ex.StackTrace);

                // Если это проблема с внешними ключами, дадим более понятное сообщение
                if (ex.Message.Contains("foreign key") || ex.Message.Contains("REFERENCE") ||
                    ex.Message.Contains("DELETE statement conflicted"))
                {
                    throw new ApplicationException("Невозможно удалить задачу: она все еще имеет связанные данные в базе. " +
                                                 "Попробуйте использовать принудительное удаление.", ex);
                }

                throw new ApplicationException($"Ошибка при удалении задачи: {ex.Message}", ex);
            }
        }
        public async Task ForceDeleteTaskAsync(Guid id)
        {
            _logger?.LogInformation("=== НАЧАЛО ПРИНУДИТЕЛЬНОГО УДАЛЕНИЯ ЗАДАЧИ {TaskId} ===", id);

            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
                throw new ApplicationException($"Task with ID {id} not found.");

            // Отключаем проверки внешних ключей временно (только для SQL Server)
            try
            {
                // Используем отдельные транзакции для каждого типа данных
                await DeleteTaskDependenciesInBatches(id);

                // Теперь удаляем саму задачу
                using var finalTransaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    await _taskRepository.DeleteAsync(id);
                    await _context.SaveChangesAsync();
                    await finalTransaction.CommitAsync();

                    _logger?.LogInformation("=== ПРИНУДИТЕЛЬНОЕ УДАЛЕНИЕ ЗАДАЧИ {TaskId} ЗАВЕРШЕНО ===", id);
                }
                catch (Exception ex)
                {
                    await finalTransaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Ошибка при принудительном удалении задачи {TaskId}", id);
                throw new ApplicationException($"Ошибка при принудительном удалении задачи: {ex.Message}", ex);
            }
        }

        private async Task DeleteTaskDependenciesInBatches(Guid taskId)
        {
            var batchSize = 100; // Удаляем по 100 записей за раз

            // 1. Удаляем подзадачи
            await DeleteInBatches<SubTask>("SubTasks", st => st.ParentTaskId == taskId, batchSize);

            // 2. Удаляем материалы задачи
            await DeleteInBatches<TaskMaterial>("TaskMaterials", tm => tm.TaskId == taskId, batchSize);

            // 3. Удаляем транзакции материалов
            await DeleteInBatches<MaterialTransaction>("MaterialTransactions", mt => mt.TaskId == taskId, batchSize);

            // 4. Удаляем транзакции продуктов
            await DeleteInBatches<ProductTransaction>("ProductTransactions", pt => pt.TaskId == taskId, batchSize);

            // 5. Удаляем комментарии
            await DeleteInBatches<TaskComment>("TaskComments", tc => tc.TaskId == taskId, batchSize);

            // 6. Удаляем уведомления
            await DeleteInBatches<Notification>("Notifications", n => n.TaskId == taskId, batchSize);
        }
        private async Task DeleteInBatches<T>(string entityName, System.Linq.Expressions.Expression<Func<T, bool>> predicate, int batchSize) where T : class
        {
            _logger?.LogInformation("Deleting {EntityName} in batches...", entityName);

            var dbSet = _context.Set<T>();
            int deletedCount = 0;

            while (true)
            {
                var batch = await dbSet.Where(predicate).Take(batchSize).ToListAsync();
                if (!batch.Any()) break;

                dbSet.RemoveRange(batch);
                await _context.SaveChangesAsync();

                deletedCount += batch.Count;
                _logger?.LogInformation("Deleted {BatchCount} {EntityName} records (total: {TotalCount})",
                                       batch.Count, entityName, deletedCount);

                // Небольшая пауза между пакетами для снижения нагрузки
                await Task.Delay(10);
            }

            _logger?.LogInformation("Finished deleting {EntityName}: {TotalCount} records removed", entityName, deletedCount);
        }

        public async Task<TaskDependencyInfoDto> GetTaskDependenciesAsync(Guid taskId)
        {
            _logger?.LogInformation("=== НАЧАЛО ПРОВЕРКИ ЗАВИСИМОСТЕЙ ===");
            _logger?.LogInformation("Checking dependencies for task {TaskId}", taskId);

            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
            {
                _logger?.LogWarning("Task {TaskId} not found", taskId);
                throw new ApplicationException($"Task with ID {taskId} not found.");
            }

            _logger?.LogInformation("Task found: {Title}", task.Title);

            var dependencies = new List<TaskDependencyDto>();
            bool canDelete = true;

            try
            {
                // 1. Проверяем подзадачи
                _logger?.LogInformation("Checking subtasks...");
                try
                {
                    var subTasks = await _context.SubTasks
                        .Where(st => st.ParentTaskId == taskId)
                        .ToListAsync();

                    _logger?.LogInformation("Found {Count} subtasks", subTasks.Count);

                    if (subTasks.Any())
                    {
                        var activeSubTasks = subTasks.Where(st => st.StatusId != 5 && st.StatusId != 6).ToList();

                        dependencies.Add(new TaskDependencyDto
                        {
                            Type = "SubTasks",
                            Count = subTasks.Count,
                            Description = $"Подзадачи ({subTasks.Count} всего, {activeSubTasks.Count} активных)",
                            Items = subTasks.Select(st => new TaskDependencyItemDto
                            {
                                Id = st.Id,
                                Name = st.Title ?? "Без названия",
                                Status = GetSubTaskStatusName(st.StatusId),
                                Details = $"Срок: {st.DueDate?.ToString("dd.MM.yyyy") ?? "не указан"}"
                            }).ToList()
                        });

                        if (activeSubTasks.Any())
                            canDelete = false;
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error checking subtasks");
                }

                // 2. Проверяем материалы задачи
                _logger?.LogInformation("Checking task materials...");
                try
                {
                    var taskMaterials = await _context.TaskMaterials
                        .Where(tm => tm.TaskId == taskId)
                        .ToListAsync();

                    _logger?.LogInformation("Found {Count} task materials", taskMaterials.Count);

                    if (taskMaterials.Any())
                    {
                        // Загружаем материалы отдельно, чтобы избежать проблем с Include
                        var materialIds = taskMaterials.Select(tm => tm.MaterialId).ToList();
                        var materials = await _context.Materials
                            .Where(m => materialIds.Contains(m.Id))
                            .ToListAsync();

                        var materialDict = materials.ToDictionary(m => m.Id, m => m);

                        dependencies.Add(new TaskDependencyDto
                        {
                            Type = "Materials",
                            Count = taskMaterials.Count,
                            Description = $"Связанные материалы ({taskMaterials.Count})",
                            Items = taskMaterials.Select(tm => new TaskDependencyItemDto
                            {
                                Id = tm.MaterialId,
                                Name = materialDict.ContainsKey(tm.MaterialId)
                                    ? materialDict[tm.MaterialId].Name
                                    : "Неизвестный материал",
                                Details = $"Количество: {tm.Quantity}",
                                Status = "Зарезервировано"
                            }).ToList()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error checking task materials");
                }

                // 3. Проверяем транзакции материалов
                _logger?.LogInformation("Checking material transactions...");
                try
                {
                    var materialTransactions = await _context.MaterialTransactions
                        .Where(mt => mt.TaskId == taskId)
                        .ToListAsync();

                    _logger?.LogInformation("Found {Count} material transactions", materialTransactions.Count);

                    if (materialTransactions.Any())
                    {
                        var materialIds = materialTransactions.Select(mt => mt.MaterialId).ToList();
                        var materials = await _context.Materials
                            .Where(m => materialIds.Contains(m.Id))
                            .ToListAsync();

                        var materialDict = materials.ToDictionary(m => m.Id, m => m);

                        dependencies.Add(new TaskDependencyDto
                        {
                            Type = "MaterialTransactions",
                            Count = materialTransactions.Count,
                            Description = $"Транзакции материалов ({materialTransactions.Count})",
                            Items = materialTransactions.Select(mt => new TaskDependencyItemDto
                            {
                                Id = mt.Id,
                                Name = materialDict.ContainsKey(mt.MaterialId)
                                    ? materialDict[mt.MaterialId].Name
                                    : "Неизвестный материал",
                                Details = $"{mt.TransactionType}: {mt.Quantity}",
                                Status = mt.CreatedAt.ToString("dd.MM.yyyy")
                            }).ToList()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error checking material transactions");
                }

                // 4. Проверяем комментарии (без Include для безопасности)
                _logger?.LogInformation("Checking comments...");
                try
                {
                    var comments = await _context.TaskComments
                        .Where(tc => tc.TaskId == taskId)
                        .ToListAsync();

                    _logger?.LogInformation("Found {Count} comments", comments.Count);

                    if (comments.Any())
                    {
                        dependencies.Add(new TaskDependencyDto
                        {
                            Type = "Comments",
                            Count = comments.Count,
                            Description = $"Комментарии ({comments.Count})",
                            Items = comments.Select(c => new TaskDependencyItemDto
                            {
                                Id = c.Id,
                                Name = "Комментарий",
                                Details = c.Comment?.Length > 50 ? c.Comment.Substring(0, 50) + "..." : c.Comment ?? "",
                                Status = c.CreatedAt.ToString("dd.MM.yyyy HH:mm")
                            }).ToList()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error checking comments");
                }

                // 5. Проверяем уведомления
                _logger?.LogInformation("Checking notifications...");
                try
                {
                    var notifications = await _context.Notifications
                        .Where(n => n.TaskId == taskId)
                        .ToListAsync();

                    _logger?.LogInformation("Found {Count} notifications", notifications.Count);

                    if (notifications.Any())
                    {
                        dependencies.Add(new TaskDependencyDto
                        {
                            Type = "Notifications",
                            Count = notifications.Count,
                            Description = $"Уведомления ({notifications.Count})",
                            Items = notifications.Select(n => new TaskDependencyItemDto
                            {
                                Id = n.Id,
                                Name = n.Title ?? "Уведомление",
                                Details = n.Message ?? "",
                                Status = n.IsRead ? "Прочитано" : "Не прочитано"
                            }).ToList()
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "Error checking notifications");
                }

                // Формируем итоговое описание
                string summary = canDelete
                    ? "Задачу можно удалить. Все связанные данные будут удалены автоматически."
                    : "Задачу нельзя удалить из-за активных зависимостей. Завершите все подзадачи или используйте принудительное удаление.";

                _logger?.LogInformation("Dependencies check completed. CanDelete: {CanDelete}, Dependencies: {Count}", canDelete, dependencies.Count);
                _logger?.LogInformation("=== ЗАВЕРШЕНИЕ ПРОВЕРКИ ЗАВИСИМОСТЕЙ ===");

                return new TaskDependencyInfoDto
                {
                    TaskId = taskId,
                    TaskTitle = task.Title ?? "Без названия",
                    CanDelete = canDelete,
                    Dependencies = dependencies,
                    Summary = summary
                };
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error checking task dependencies for task {TaskId}", taskId);
                throw new ApplicationException($"Error checking task dependencies: {ex.Message}", ex);
            }
        }

        private string GetSubTaskStatusName(int statusId)
        {
            return statusId switch
            {
                1 => "Создана",
                2 => "В работе",
                3 => "Требует уточнения",
                4 => "На проверке",
                5 => "Выполнена",
                6 => "Отклонена",
                _ => "Неизвестный статус"
            };
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
                    Console.WriteLine($"Задача с ID {taskId} не найдена");
                    return Enumerable.Empty<TaskMaterialDto>();
                }

                // Проверяем, загружены ли материалы
                if (task.TaskMaterials == null)
                {
                    Console.WriteLine($"Коллекция TaskMaterials равна null для задачи {taskId}");
                    return Enumerable.Empty<TaskMaterialDto>();
                }

                // Преобразуем в DTO
                var result = task.TaskMaterials
                    .Select(tm => new TaskMaterialDto
                    {
                        Id = tm.Id,
                        TaskId = taskId,
                        MaterialId = tm.MaterialId,
                        MaterialName = tm.Material?.Name ?? "Материал не найден",
                        MaterialCode = tm.Material?.Code ?? "",
                        Quantity = tm.Quantity,
                        UnitOfMeasure = tm.Material?.UnitOfMeasure ?? ""
                    })
                    .ToList();

                Console.WriteLine($"TaskService: Найдено {result.Count} материалов");
                foreach (var mat in result)
                {
                    Console.WriteLine($"  - Материал: {mat.MaterialName}, Количество: {mat.Quantity}");
                }

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
