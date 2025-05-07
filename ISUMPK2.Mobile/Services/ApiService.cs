using Microsoft.Maui.Networking;
using ISUMPK2.Mobile.Models;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

namespace ISUMPK2.Mobile.Services
{
    public interface IApiService
    {
        Task<T> GetAsync<T>(string endpoint);
        Task<T> PostAsync<T>(string endpoint, object data);
        Task<T> PutAsync<T>(string endpoint, object data);
        Task DeleteAsync(string endpoint);
        Task SetAuthToken(string token);
        Task ClearAuthToken();
        Task<T>PatchAsync<T>(string v, TaskStatusUpdateModel statusUpdate);

        bool IsAuthenticated { get; }
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ISettingsService _settingsService;
        private readonly IConnectivity _connectivity;
        private bool _isAuthenticated;

        public bool IsAuthenticated => _isAuthenticated;

        public ApiService(ISettingsService settingsService, IConnectivity connectivity)
        {
            _settingsService = settingsService;
            _connectivity = connectivity;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_settingsService.ApiUrl);

            InitializeAuthToken();
        }

        private async void InitializeAuthToken()
        {
            var token = await _settingsService.GetAuthToken();
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                _isAuthenticated = true;
            }
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            CheckConnectivity();

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            CheckConnectivity();

            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PutAsync<T>(string endpoint, object data)
        {
            CheckConnectivity();

            var jsonContent = JsonSerializer.Serialize(data);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(endpoint, content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task DeleteAsync(string endpoint)
        {
            CheckConnectivity();

            var response = await _httpClient.DeleteAsync(endpoint);
            response.EnsureSuccessStatusCode();
        }

        public async Task<T> PatchAsync<T>(string endpoint, TaskStatusUpdateModel statusUpdate)
        {
            CheckConnectivity();

            var jsonContent = JsonSerializer.Serialize(statusUpdate);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, endpoint)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task SetAuthToken(string token)
        {
            await _settingsService.SetAuthToken(token);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            _isAuthenticated = true;
        }

        public async Task ClearAuthToken()
        {
            await _settingsService.ClearAuthToken();
            _httpClient.DefaultRequestHeaders.Authorization = null;
            _isAuthenticated = false;
        }

        private void CheckConnectivity()
        {
            if (!_connectivity.NetworkAccess.HasFlag(NetworkAccess.Internet))
            {
                throw new Exception("Нет подключения к интернету");
            }
        }
    }
}
