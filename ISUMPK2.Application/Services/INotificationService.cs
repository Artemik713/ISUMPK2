using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Application.Services
{
    public interface INotificationService
    {
        Task<NotificationDto> GetNotificationByIdAsync(Guid id);
        Task<IEnumerable<NotificationDto>> GetAllNotificationsForUserAsync(Guid userId);
        Task<IEnumerable<NotificationDto>> GetUnreadNotificationsForUserAsync(Guid userId);
        Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId);
        Task<NotificationDto> CreateNotificationAsync(NotificationCreateDto notificationDto);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadForUserAsync(Guid userId);
        Task DeleteNotificationAsync(Guid id);

        // Методы для создания системных уведомлений
        Task CreateTaskAssignedNotificationAsync(Guid taskId, Guid assigneeId);
        Task CreateTaskStatusChangedNotificationAsync(Guid taskId, int oldStatusId, int newStatusId);
        Task CreateTaskDueDateNotificationAsync(Guid taskId);
        Task CreateLowStockNotificationAsync(Guid materialId);
    }

}
