using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public interface INotificationService
    {
        Task<IEnumerable<NotificationModel>> GetAllNotificationsForUserAsync();
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadForUserAsync();
    }
}
