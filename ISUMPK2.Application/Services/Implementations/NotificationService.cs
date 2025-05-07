using ISUMPK2.Application.DTOs;
using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Application.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IMaterialRepository _materialRepository;
        private readonly IUserRepository _userRepository;

        public NotificationService(
            INotificationRepository notificationRepository,
            ITaskRepository taskRepository,
            IMaterialRepository materialRepository,
            IUserRepository userRepository)
        {
            _notificationRepository = notificationRepository;
            _taskRepository = taskRepository;
            _materialRepository = materialRepository;
            _userRepository = userRepository;
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                return null;

            return MapNotificationToDto(notification);
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsForUserAsync(Guid userId)
        {
            var notifications = await _notificationRepository.FindAsync(n => n.UserId == userId);
            return notifications.Select(MapNotificationToDto).OrderByDescending(n => n.CreatedAt);
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsForUserAsync(Guid userId)
        {
            var notifications = await _notificationRepository.GetUnreadNotificationsForUserAsync(userId);
            return notifications.Select(MapNotificationToDto).OrderByDescending(n => n.CreatedAt);
        }

        public async Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId)
        {
            return await _notificationRepository.GetUnreadNotificationsCountForUserAsync(userId);
        }

        public async Task<NotificationDto> CreateNotificationAsync(NotificationCreateDto notificationDto)
        {
            var user = await _userRepository.GetByIdAsync(notificationDto.UserId);
            if (user == null)
                throw new ApplicationException($"User with ID {notificationDto.UserId} not found.");

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = notificationDto.UserId,
                Title = notificationDto.Title,
                Message = notificationDto.Message,
                IsRead = false,
                TaskId = notificationDto.TaskId,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();

            return MapNotificationToDto(notification);
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            await _notificationRepository.MarkAsReadAsync(notificationId);
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            await _notificationRepository.MarkAllAsReadForUserAsync(userId);
        }

        public async Task DeleteNotificationAsync(Guid id)
        {
            var notification = await _notificationRepository.GetByIdAsync(id);
            if (notification == null)
                throw new ApplicationException($"Notification with ID {id} not found.");

            await _notificationRepository.DeleteAsync(id);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task CreateTaskAssignedNotificationAsync(Guid taskId, Guid assigneeId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new ApplicationException($"Task with ID {taskId} not found.");

            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = assigneeId,
                Title = "Новая задача назначена",
                Message = $"Вам назначена задача: {task.Title}",
                IsRead = false,
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task CreateTaskStatusChangedNotificationAsync(Guid taskId, int oldStatusId, int newStatusId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new ApplicationException($"Task with ID {taskId} not found.");

            var oldStatusName = task.Status?.Name ?? oldStatusId.ToString();
            var newStatusName = task.Status?.Name ?? newStatusId.ToString();

            // Создаем уведомление для создателя задачи
            var creatorNotification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = task.CreatorId,
                Title = "Статус задачи изменен",
                Message = $"Статус задачи '{task.Title}' изменен с '{oldStatusName}' на '{newStatusName}'",
                IsRead = false,
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(creatorNotification);

            // Если есть назначенный исполнитель и он не является создателем, уведомляем и его
            if (task.AssigneeId.HasValue && task.AssigneeId != task.CreatorId)
            {
                var assigneeNotification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = task.AssigneeId.Value,
                    Title = "Статус задачи изменен",
                    Message = $"Статус задачи '{task.Title}' изменен с '{oldStatusName}' на '{newStatusName}'",
                    IsRead = false,
                    TaskId = taskId,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(assigneeNotification);
            }

            await _notificationRepository.SaveChangesAsync();
        }
        public async Task CreateTaskDueDateNotificationAsync(Guid taskId)
        {
            var task = await _taskRepository.GetByIdAsync(taskId);
            if (task == null)
                throw new ApplicationException($"Task with ID {taskId} not found.");

            // Проверяем, что у задачи есть назначенный исполнитель и срок выполнения
            if (!task.AssigneeId.HasValue || !task.DueDate.HasValue)
                return;

            // Создаем уведомление для исполнителя
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = task.AssigneeId.Value,
                Title = "Приближается срок выполнения задачи",
                Message = $"Срок выполнения задачи '{task.Title}' истекает {task.DueDate.Value.ToString("dd.MM.yyyy")}",
                IsRead = false,
                TaskId = taskId,
                CreatedAt = DateTime.UtcNow
            };

            await _notificationRepository.AddAsync(notification);
            await _notificationRepository.SaveChangesAsync();
        }

        public async Task CreateLowStockNotificationAsync(Guid materialId)
        {
            var material = await _materialRepository.GetByIdAsync(materialId);
            if (material == null)
                throw new ApplicationException($"Material with ID {materialId} not found.");

            // Получаем всех пользователей с ролью Storekeeper
            var storekeepers = await _userRepository.GetUsersByRoleAsync("Storekeeper");

            foreach (var storekeeper in storekeepers)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = storekeeper.Id,
                    Title = "Низкий уровень запасов материала",
                    Message = $"Количество материала '{material.Name}' ({material.Code}) ниже минимального уровня. Текущий запас: {material.CurrentStock} {material.UnitOfMeasure}, минимальный уровень: {material.MinimumStock} {material.UnitOfMeasure}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(notification);
            }

            // Дополнительно уведомляем руководство
            var managers = await _userRepository.GetUsersByRoleAsync("GeneralDirector");

            foreach (var manager in managers)
            {
                var notification = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = manager.Id,
                    Title = "Низкий уровень запасов материала",
                    Message = $"Количество материала '{material.Name}' ({material.Code}) ниже минимального уровня. Текущий запас: {material.CurrentStock} {material.UnitOfMeasure}, минимальный уровень: {material.MinimumStock} {material.UnitOfMeasure}",
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddAsync(notification);
            }

            await _notificationRepository.SaveChangesAsync();
        }

        private NotificationDto MapNotificationToDto(Notification notification)
        {
            return new NotificationDto
            {
                Id = notification.Id,
                UserId = notification.UserId,
                Title = notification.Title,
                Message = notification.Message,
                IsRead = notification.IsRead,
                TaskId = notification.TaskId,
                TaskTitle = notification.Task?.Title,
                CreatedAt = notification.CreatedAt
            };
        }
    }
}
