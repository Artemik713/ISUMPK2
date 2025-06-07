using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Domain.Repositories
{
    public interface INotificationRepository: IRepository<Notification>
    {
        Task<IEnumerable<Notification>> GetReadNotificationsForUserAsync(Guid userId);
        Task<IEnumerable<Notification>> GetUnreadNotificationsForUserAsync(Guid userId);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadForUserAsync(Guid userId);
        Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId);
    }
}
