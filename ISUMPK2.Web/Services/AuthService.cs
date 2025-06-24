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
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", loginModel);

            if (!response.IsSuccessStatusCode)
            {
                return new LoginResult { Successful = false, Error = "Неверное имя пользователя или пароль" };
            }

            var userLoginResponse = await response.Content.ReadFromJsonAsync<UserLoginResponse>();

            if (userLoginResponse == null)
            {
                return new LoginResult { Successful = false, Error = "Неверный ответ сервера" };
            }

            await _localStorage.SetItemAsync("authToken", userLoginResponse.Token);
            await _localStorage.SetItemAsync("userId", userLoginResponse.Id);
            await _localStorage.SetItemAsync("userName", userLoginResponse.UserName);
            await _localStorage.SetItemAsync("userRoles", userLoginResponse.Roles);
            await _localStorage.SetItemAsync("tokenExpiration", userLoginResponse.TokenExpiration);

            // Обновление состояния аутентификации
            _authenticationStateProvider.MarkUserAsAuthenticated(userLoginResponse);
            AuthenticationChanged?.Invoke(true);

            // Установка заголовка авторизации для HTTP запросов
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userLoginResponse.Token);

            return new LoginResult { Successful = true };
        }


        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("userId");
            await _localStorage.RemoveItemAsync("userName");
            await _localStorage.RemoveItemAsync("userRoles");
            await _localStorage.RemoveItemAsync("tokenExpiration");

            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            AuthenticationChanged?.Invoke(false);

            _httpClient.DefaultRequestHeaders.Authorization = null;
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
