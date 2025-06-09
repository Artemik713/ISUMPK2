using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace ISUMPK2.Web.Services
{
    public class ClientNotificationService : IClientNotificationService
    {
        private readonly HttpClient _httpClient;

        public ClientNotificationService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<NotificationDto>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<NotificationDto>>("api/notifications");
        }

        public async Task<IEnumerable<NotificationDto>> GetFilteredAsync(NotificationFilterDto filter)
        {
            var queryParams = new StringBuilder("api/notifications/filter?");

            if (filter.StartDate.HasValue)
                queryParams.Append($"startDate={HttpUtility.UrlEncode(filter.StartDate.Value.ToString("o"))}&");

            if (filter.EndDate.HasValue)
                queryParams.Append($"endDate={HttpUtility.UrlEncode(filter.EndDate.Value.ToString("o"))}&");

            if (filter.IsRead.HasValue)
                queryParams.Append($"isRead={filter.IsRead.Value}&");

            if (!string.IsNullOrEmpty(filter.Type))
                queryParams.Append($"type={HttpUtility.UrlEncode(filter.Type)}");

            var response = await _httpClient.GetFromJsonAsync<IEnumerable<NotificationDto>>(queryParams.ToString().TrimEnd('&'));
            return response;
        }

        public async Task<NotificationDto> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<NotificationDto>($"api/notifications/{id}");
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var response = await _httpClient.PutAsync($"api/notifications/{id}/read", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task MarkAllAsReadAsync()
        {
            var response = await _httpClient.PutAsync("api/notifications/read-all", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task<int> GetUnreadCountAsync()
        {
            return await _httpClient.GetFromJsonAsync<int>("api/notifications/unread-count");
        }
    }
}