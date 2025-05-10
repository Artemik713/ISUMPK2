// ISUMPK2.Web/Services/ClientUserService.cs
using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Web.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public class ClientUserService : IUserService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public ClientUserService(HttpClient httpClient, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<UserDto>($"api/users/by-username/{username}");
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>("api/users");
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>($"api/users/by-role/{role}");
        }

        public async Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(Guid departmentId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<UserDto>>($"api/users/by-department/{departmentId}");
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            await SetAuthorizationHeaderAsync();
            var token = await _localStorageService.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return await _httpClient.GetFromJsonAsync<UserDto>("api/users/current");
        }

        public async Task<UserDto> CreateUserAsync(UserCreateDto userDto)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/users", userDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", userDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/users/login", loginDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserLoginResponseDto>();
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            await SetAuthorizationHeaderAsync();
            var requestData = new { currentPassword, newPassword };
            var response = await _httpClient.PostAsJsonAsync($"api/users/{userId}/change-password", requestData);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<string>>($"api/users/{userId}/roles");
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<bool>($"api/users/{userId}/roles/{role}");
        }

        public async Task AddToRoleAsync(Guid userId, string role)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsync($"api/users/{userId}/roles/{role}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveFromRoleAsync(Guid userId, string role)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/users/{userId}/roles/{role}");
            response.EnsureSuccessStatusCode();
        }
    }
}
