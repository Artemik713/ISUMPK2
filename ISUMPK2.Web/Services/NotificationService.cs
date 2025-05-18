using ISUMPK2.Web.Models;
using System.Net.Http.Json;

namespace ISUMPK2.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;

        public NotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<NotificationModel>> GetAllNotificationsForUserAsync()
        {
            // Запрос к /api/notifications/user (userId берётся из токена на сервере)
            var response = await _httpClient.GetAsync("api/notifications/user");
            response.EnsureSuccessStatusCode();
            var notifications = await response.Content.ReadFromJsonAsync<IEnumerable<NotificationModel>>();
            return notifications ?? Enumerable.Empty<NotificationModel>();
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var response = await _httpClient.PostAsync($"api/notifications/{notificationId}/mark-as-read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAllAsReadForUserAsync()
        {
            var response = await _httpClient.PostAsync("api/notifications/mark-all-as-read", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
