using ISUMPK2.Web.Models; // Added the correct namespace for NotificationModel

namespace ISUMPK2.Web.Services
{
    public interface INotificationHubService
    {
        event Action<NotificationModel> OnReceiveNotification;
        Task ConnectAsync();
        Task DisconnectAsync();
        Task JoinUserGroupAsync(string userId);
        Task LeaveUserGroupAsync(string userId);
    }
}
