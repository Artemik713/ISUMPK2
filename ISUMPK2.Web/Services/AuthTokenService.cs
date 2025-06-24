using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace ISUMPK2.Web.Services
{
    // ISUMPK2.Web\Services\AuthTokenService.cs
    public class AuthTokenService : IDisposable
    {
        private readonly NavigationManager _navigationManager;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationStateProvider _authStateProvider;
        private Timer _tokenTimer;
        private readonly TimeSpan _tokenRefreshInterval = TimeSpan.FromMinutes(55); // Обновлять за 5 минут до истечения

        public AuthTokenService(
            NavigationManager navigationManager,
            ILocalStorageService localStorage,
            AuthenticationStateProvider authStateProvider)
        {
            _navigationManager = navigationManager;
            _localStorage = localStorage;
            _authStateProvider = authStateProvider;

            // Запустить таймер сразу после создания сервиса
            _tokenTimer = new Timer(CheckTokenValidity, null, TimeSpan.Zero, _tokenRefreshInterval);
        }

        private async void CheckTokenValidity(object state)
        {
            try
            {
                var authState = await _authStateProvider.GetAuthenticationStateAsync();

                // Если пользователь не аутентифицирован, нет смысла проверять
                if (!authState.User.Identity.IsAuthenticated)
                    return;

                // Получаем токен из localStorage
                var token = await _localStorage.GetItemAsync<string>("authToken");
                if (string.IsNullOrEmpty(token))
                    return;

                // Проверяем, не истек ли токен или скоро истечет
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var expiryDate = jwtToken.ValidTo;
                var timeUntilExpiry = expiryDate - DateTime.UtcNow;

                // Если до истечения меньше 5 минут, перенаправляем на страницу входа
                if (timeUntilExpiry.TotalMinutes < 5)
                {
                    await _localStorage.RemoveItemAsync("authToken");
                    _navigationManager.NavigateTo("/login?expired=true", forceLoad: true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке токена: {ex.Message}");
            }
        }

        public void Dispose()
        {
            _tokenTimer?.Dispose();
        }
    }
}