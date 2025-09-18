using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public class NotificationService : INotificationService
    {
        private readonly HttpClient _httpClient;

        public NotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(Guid id)
        {
            var response = await _httpClient.GetAsync($"api/notifications/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NotificationDto>();
            }
            throw new HttpRequestException($"Ошибка при получении уведомления: {response.StatusCode}");
        }

        public async Task<IEnumerable<NotificationDto>> GetAllNotificationsForUserAsync(Guid userId)
        {
            // Обратите внимание, что userId здесь не используется, т.к. сервер берет ID из токена
            var response = await _httpClient.GetAsync($"api/notifications/user");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDto>>() ?? Array.Empty<NotificationDto>();
            }
            throw new HttpRequestException($"Ошибка при получении уведомлений: {response.StatusCode}");
        }

        public async Task<IEnumerable<NotificationDto>> GetUnreadNotificationsForUserAsync(Guid userId)
        {
            var response = await _httpClient.GetAsync("api/notifications/unread");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<NotificationDto>>() ?? Array.Empty<NotificationDto>();
            }
            throw new HttpRequestException($"Ошибка при получении непрочитанных уведомлений: {response.StatusCode}");
        }

        public async Task<int> GetUnreadNotificationsCountForUserAsync(Guid userId)
        {
            // Обратите внимание, что userId здесь не используется, т.к. сервер берет ID из токена
            var response = await _httpClient.GetAsync($"api/notifications/count-unread");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<int>();
            }
            throw new HttpRequestException($"Ошибка при получении количества непрочитанных уведомлений: {response.StatusCode}");
        }

        public async Task<NotificationDto> CreateNotificationAsync(NotificationCreateDto notificationDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/notifications", notificationDto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<NotificationDto>();
            }
            throw new HttpRequestException($"Ошибка при создании уведомления: {response.StatusCode}");
        }

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var response = await _httpClient.PostAsync($"api/notifications/{notificationId}/mark-as-read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAllAsReadForUserAsync(Guid userId)
        {
            // Обратите внимание, что userId здесь не используется, т.к. сервер берет ID из токена
            var response = await _httpClient.PostAsync("api/notifications/mark-all-as-read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteNotificationAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/notifications/{id}");
            response.EnsureSuccessStatusCode();
        }

        // Системные уведомления реализованы только на сервере
        public Task CreateTaskAssignedNotificationAsync(Guid taskId, Guid assigneeId)
        {
            throw new NotImplementedException("Метод должен быть реализован на сервере");
        }

        public Task CreateTaskStatusChangedNotificationAsync(Guid taskId, int oldStatusId, int newStatusId)
        {
            throw new NotImplementedException("Метод должен быть реализован на сервере");
        }

        public Task CreateTaskDueDateNotificationAsync(Guid taskId)
        {
            throw new NotImplementedException("Метод должен быть реализован на сервере");
        }

        public Task CreateLowStockNotificationAsync(Guid materialId)
        {
            throw new NotImplementedException("Метод должен быть реализован на сервере");
        }
    }
}