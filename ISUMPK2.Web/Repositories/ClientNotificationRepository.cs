using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientNotificationRepository : ClientRepositoryBase<Notification>, INotificationRepository
    {
        protected override string ApiEndpoint => "api/notifications";

        public ClientNotificationRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<IEnumerable<Notification>> GetNotificationsForUserAsync(Guid userId)
        {
            // Изменено: больше не передаём userId в URL
            return await HttpClient.GetFromJsonAsync<IEnumerable<Notification>>($"{ApiEndpoint}/user");
        }

        public async Task<IEnumerable<Notification>> GetReadNotificationsForUserAsync(Guid userId)
        {
            // Вызываем соответствующий API endpoint
            return await HttpClient.GetFromJsonAsync<IEnumerable<Notification>>($"{ApiEndpoint}/read");
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsForUserAsync(Guid userId)
        {
            // Изменено: больше не передаём userId в URL
            return await HttpClient.GetFromJsonAsync<IEnumerable<Notification>>($"{ApiEndpoint}/unread");
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            // Правильный URL, соответствующий API контроллеру
            await HttpClient.PostAsync($"{ApiEndpoint}/{notificationId}/mark-as-read", null);
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            // Изменено: больше не передаём userId в URL
            await HttpClient.PostAsync($"{ApiEndpoint}/mark-all-as-read", null);
        }

        public async Task<int> GetUnreadCountForUserAsync(Guid userId)
        {
            // Изменено: больше не передаём userId в URL
            return await HttpClient.GetFromJsonAsync<int>($"{ApiEndpoint}/count-unread");
        }

        public async Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId)
        {
            // Изменено: больше не передаём userId в URL
            return await HttpClient.GetFromJsonAsync<int>($"{ApiEndpoint}/count-unread");
        }

        public async Task CreateLowStockNotificationAsync(Guid materialId)
        {
            // Этот метод требует уточнения - нужно проверить, есть ли такой эндпоинт в API
            await HttpClient.PostAsync($"{ApiEndpoint}/lowstock/{materialId}", null);
        }
    }
}