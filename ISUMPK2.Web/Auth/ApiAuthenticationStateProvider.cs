using Microsoft.AspNetCore.Components.Authorization;
using ISUMPK2.Web.Models;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using ISUMPK2.Web.Services;
using System.Linq;
using AutoMapper.Internal;

namespace ISUMPK2.Web.Auth
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }

        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", savedToken);

            try
            {
                return new AuthenticationState(new ClaimsPrincipal(
                    new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
            }
            catch (Exception ex)
            {
                // Если возникла ошибка при разборе токена, очищаем хранилище
                Console.WriteLine($"Ошибка при разборе JWT токена: {ex.Message}");
                await _localStorage.RemoveItemAsync("authToken");
                await _localStorage.RemoveItemAsync("userId");
                await _localStorage.RemoveItemAsync("userName");
                await _localStorage.RemoveItemAsync("userRoles");
                await _localStorage.RemoveItemAsync("tokenExpiration");

                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }


        public void MarkUserAsAuthenticated(UserLoginResponse user)
        {
            var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
            }.Concat(user.Roles.Select(role => new Claim(ClaimTypes.Role, role))), "jwt"));

            var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
        {
            var claims = new List<Claim>();
            try
            {
                var parts = jwt.Split('.');
                if (parts.Length != 3)
                {
                    Console.WriteLine("JWT токен имеет неверный формат");
                    return claims;
                }

                var payload = parts[1];
                var jsonBytes = ParseBase64WithoutPadding(payload);
                var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

                if (keyValuePairs == null)
                {
                    Console.WriteLine("Не удалось десериализовать payload JWT");
                    return claims;
                }

                if (keyValuePairs.TryGetValue("nameid", out var nameid) && nameid != null)
                {
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, nameid.ToString()));
                }

                if (keyValuePairs.TryGetValue("unique_name", out var uniqueName) && uniqueName != null)
                {
                    claims.Add(new Claim(ClaimTypes.Name, uniqueName.ToString()));
                }

                if (keyValuePairs.TryGetValue("email", out var email) && email != null)
                {
                    claims.Add(new Claim(ClaimTypes.Email, email.ToString()));
                }

                if (keyValuePairs.TryGetValue("role", out var role) && role != null)
                {
                    if (role is JsonElement element && element.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var r in element.EnumerateArray())
                        {
                            claims.Add(new Claim(ClaimTypes.Role, r.GetString()));
                        }
                    }
                    else
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role.ToString()));
                    }
                }

                return claims;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при разборе claims из JWT: {ex.Message}");
                return claims;
            }
        }


        private byte[] ParseBase64WithoutPadding(string base64)
        {
            try
            {
                // Убедимся, что у нас корректный Base64-URL формат
                base64 = base64.Replace('-', '+').Replace('_', '/');

                // Добавляем необходимое выравнивание, если нужно
                switch (base64.Length % 4)
                {
                    case 0: break;                 // Выравнивание правильное
                    case 2: base64 += "=="; break; // Добавляем два символа выравнивания
                    case 3: base64 += "="; break;  // Добавляем один символ выравнивания
                    default:
                        throw new FormatException("Некорректная длина строки Base64");
                }

                return Convert.FromBase64String(base64);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при декодировании Base64: {ex.Message}");
                // Возвращаем пустой объект JSON в случае ошибки
                return System.Text.Encoding.UTF8.GetBytes("{}");
            }
        }

    }
}
