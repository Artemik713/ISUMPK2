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
        private readonly AuthTokenService _authTokenService; // ИЗМЕНЕНО: используем AuthTokenService вместо IAuthService

        public ClientUserService(HttpClient httpClient, ILocalStorageService localStorageService, AuthTokenService authTokenService)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
            _authTokenService = authTokenService;
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<UserDto> GetUserByIdAsync(Guid id)
        {
            try
            {
                await _authTokenService.EnsureAuthTokenAsync(); // Теперь это работает
                var response = await _httpClient.GetAsync($"api/users/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<UserDto>();
                }

                Console.WriteLine($"Ошибка при получении пользователя: {response.StatusCode}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при получении пользователя: {ex.Message}");
                return null;
            }
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.GetAsync($"api/users/by-username/{username}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            return null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            try
            {
                Console.WriteLine("ClientUserService: Начинаем GetAllUsersAsync");

                await _authTokenService.EnsureAuthTokenAsync();

                // Проверяем что токен установлен
                var authHeader = _httpClient.DefaultRequestHeaders.Authorization;
                Console.WriteLine($"ClientUserService: Auth header = {authHeader?.Scheme} {authHeader?.Parameter?.Substring(0, Math.Min(20, authHeader.Parameter?.Length ?? 0))}...");

                Console.WriteLine("ClientUserService: Отправляем запрос на /api/users");
                var response = await _httpClient.GetAsync("api/users");

                Console.WriteLine($"ClientUserService: Получен ответ: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var users = await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
                    Console.WriteLine($"ClientUserService: Получено {users?.Count() ?? 0} пользователей");

                    // Выводим первых несколько пользователей для отладки
                    if (users != null)
                    {
                        foreach (var user in users.Take(3))
                        {
                            Console.WriteLine($"  User: {user.UserName} ({string.Join(", ", user.Roles)})");
                        }
                    }

                    return users ?? new List<UserDto>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    Console.WriteLine("ClientUserService: 403 Forbidden - недостаточно прав");
                    return new List<UserDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ClientUserService: Ошибка API: {response.StatusCode} - {errorContent}");
                    return new List<UserDto>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClientUserService: Исключение в GetAllUsersAsync: {ex.Message}");
                return new List<UserDto>();
            }
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string role)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.GetAsync($"api/users/by-role/{role}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
            }
            return new List<UserDto>();
        }

        public async Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(Guid departmentId)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.GetAsync($"api/users/by-department/{departmentId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<UserDto>>();
            }
            return new List<UserDto>();
        }

        public async Task<UserDto> GetCurrentUserAsync()
        {
            try
            {
                await _authTokenService.EnsureAuthTokenAsync();

                Console.WriteLine("ClientUserService: Отправляем запрос на /api/users/current");
                var response = await _httpClient.GetAsync("api/users/current");

                Console.WriteLine($"ClientUserService: Получен ответ {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var user = await response.Content.ReadFromJsonAsync<UserDto>();
                    Console.WriteLine($"ClientUserService: Получен пользователь {user?.UserName}");
                    return user;
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"ClientUserService: Ошибка {response.StatusCode} - {errorContent}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ClientUserService: Исключение {ex.Message}");
                return null;
            }
        }

        public async Task<UserDto> CreateUserAsync(UserCreateDto userDto)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.PostAsJsonAsync("api/users", userDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            return null;
        }

        public async Task<UserDto> UpdateUserAsync(Guid id, UserUpdateDto userDto)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", userDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserDto>();
            }
            return null;
        }

        public async Task DeleteUserAsync(Guid id)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.DeleteAsync($"api/users/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<UserLoginResponseDto> LoginAsync(UserLoginDto loginDto)
        {
            // Для логина не нужен токен
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<UserLoginResponseDto>();
            }
            return null;
        }

        public async Task<bool> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var requestData = new { currentPassword, newPassword };
            var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", requestData);
            return response.IsSuccessStatusCode;
        }

        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.GetAsync($"api/users/{userId}/roles");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
            }
            return new List<string>();
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string role)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.GetAsync($"api/users/{userId}/roles/{role}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<bool>();
            }
            return false;
        }

        public async Task AddToRoleAsync(Guid userId, string role)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.PostAsync($"api/users/{userId}/roles/{role}", null);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveFromRoleAsync(Guid userId, string role)
        {
            await _authTokenService.EnsureAuthTokenAsync();
            var response = await _httpClient.DeleteAsync($"api/users/{userId}/roles/{role}");
            response.EnsureSuccessStatusCode();
        }
    }
}