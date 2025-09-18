using ISUMPK2.Application.DTOs;
using ISUMPK2.Web.Auth;
using ISUMPK2.Web.Models;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ISUMPK2.Web.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly NavigationManager _navigationManager;
        private readonly ApiAuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public event Action<bool> AuthenticationChanged;

        public AuthService(HttpClient httpClient,
                          ApiAuthenticationStateProvider authenticationStateProvider,
                          ILocalStorageService localStorage,
                          NavigationManager navigationManager)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
            _navigationManager = navigationManager;
        }

        public async Task<string> GetTokenAsync()
        {
            try
            {
                return await _localStorage.GetItemAsync<string>("authToken") ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public async Task RefreshTokenIfNeededAsync()
        {
            try
            {
                // Получаем текущий токен и время его истечения
                var token = await _localStorage.GetItemAsync<string>("authToken");
                var tokenExpiration = await _localStorage.GetItemAsync<DateTime?>("tokenExpiration");

                if (string.IsNullOrEmpty(token))
                {
                    // Если токена нет, перенаправляем на страницу входа
                    _navigationManager.NavigateTo("/account/login", true);
                    return;
                }

                // Проверяем, истекает ли токен в ближайшие 5 минут
                if (tokenExpiration.HasValue && tokenExpiration.Value.AddMinutes(-5) <= DateTime.UtcNow)
                {
                    // Отправляем запрос на обновление токена
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    var response = await _httpClient.PostAsync("api/auth/refresh", null);

                    if (response.IsSuccessStatusCode)
                    {
                        var refreshResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();

                        if (refreshResponse != null)
                        {
                            // Обновляем токен в localStorage и заголовке авторизации
                            await _localStorage.SetItemAsync("authToken", refreshResponse.Token);
                            await _localStorage.SetItemAsync("tokenExpiration", refreshResponse.TokenExpiration);

                            // Обновляем состояние авторизации
                            _authenticationStateProvider.MarkUserAsAuthenticated(refreshResponse);
                            _httpClient.DefaultRequestHeaders.Authorization =
                                new AuthenticationHeaderValue("Bearer", refreshResponse.Token);
                        }
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        // Если не удалось обновить токен, выходим из аккаунта
                        await Logout();
                        _navigationManager.NavigateTo("/account/login", true);
                    }
                }
            }
            catch (Exception)
            {
                // В случае ошибок очищаем токен и перенаправляем на страницу входа
                await Logout();
                _navigationManager.NavigateTo("/account/login", true);
            }
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
            Console.WriteLine($"Начало входа для пользователя: {loginModel.UserName}");

            // Полная очистка всех данных предыдущей сессии
            await ForceLogout();

            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка входа: {response.StatusCode} - {errorContent}");
                return new LoginResult { Successful = false, Error = "Неверное имя пользователя или пароль" };
            }

            var userLoginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();

            if (userLoginResponse == null)
            {
                return new LoginResult { Successful = false, Error = "Неверный ответ сервера" };
            }

            Console.WriteLine($"Успешный вход. Пользователь: {userLoginResponse.UserName}, Роли: {string.Join(", ", userLoginResponse.Roles)}");

            // Сохраняем новые данные
            await _localStorage.SetItemAsync("authToken", userLoginResponse.Token);
            await _localStorage.SetItemAsync("userId", userLoginResponse.Id);
            await _localStorage.SetItemAsync("userName", userLoginResponse.UserName);
            await _localStorage.SetItemAsync("userRoles", userLoginResponse.Roles);
            await _localStorage.SetItemAsync("tokenExpiration", userLoginResponse.TokenExpiration);

            // Устанавливаем заголовок авторизации
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", userLoginResponse.Token);

            // Обновление состояния аутентификации
            _authenticationStateProvider.MarkUserAsAuthenticated(userLoginResponse);
            AuthenticationChanged?.Invoke(true);

            Console.WriteLine("Токен установлен, состояние обновлено");

            return new LoginResult { Successful = true };
        }

        private async Task ForceLogout()
        {
            try
            {
                Console.WriteLine("Принудительная очистка сессии...");

                // Очищаем все возможные ключи localStorage
                var keysToRemove = new[] {
            "authToken", "userId", "userName", "userRoles", "tokenExpiration",
            "user", "token", "currentUser", "authData"
        };

                foreach (var key in keysToRemove)
                {
                    await _localStorage.RemoveItemAsync(key);
                }

                // Очищаем заголовки HTTP клиента
                _httpClient.DefaultRequestHeaders.Clear();
                _httpClient.DefaultRequestHeaders.Authorization = null;

                // Уведомляем о выходе
                _authenticationStateProvider.MarkUserAsLoggedOut();
                AuthenticationChanged?.Invoke(false);

                Console.WriteLine("Сессия очищена полностью");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при очистке сессии: {ex.Message}");
            }
        }

        public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token))
                return false;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var changePasswordDto = new
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", changePasswordDto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task Logout()
        {
            await ForceLogout();
        }

        public async Task<bool> IsUserAuthenticated()
        {
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            return authState.User.Identity.IsAuthenticated;
        }

        public async Task<UserModel> GetUserInfoAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");
            if (string.IsNullOrEmpty(token))
                return null;

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            try
            {
                var userDto = await _httpClient.GetFromJsonAsync<UserDto>("api/users/current");
                return userDto.ToModel(); // Метод преобразования DTO в модель
            }
            catch
            {
                return null;
            }
        }
    }
}