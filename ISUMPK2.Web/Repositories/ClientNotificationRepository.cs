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
            return await HttpClient.GetFromJsonAsync<IEnumerable<Notification>>($"{ApiEndpoint}/user/{userId}");
        }

        public async Task<IEnumerable<Notification>> GetUnreadNotificationsForUserAsync(Guid userId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<Notification>>($"{ApiEndpoint}/user/{userId}/unread");
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            await HttpClient.PutAsync($"{ApiEndpoint}/{notificationId}/read", null);
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            await HttpClient.PutAsync($"{ApiEndpoint}/user/{userId}/mark-all-read", null);
        }

        public async Task<int> GetUnreadCountForUserAsync(Guid userId)
        {
            return await HttpClient.GetFromJsonAsync<int>($"{ApiEndpoint}/user/{userId}/unread/count");
        }

        public async Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId)
        {
            return await HttpClient.GetFromJsonAsync<int>($"{ApiEndpoint}/user/{userId}/unread/count");
        }

        public async Task CreateLowStockNotificationAsync(Guid materialId)
        {
            await HttpClient.PostAsync($"{ApiEndpoint}/lowstock/{materialId}", null);
        }
    }
}
