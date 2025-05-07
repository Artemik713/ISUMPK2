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

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
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
            var payload = jwt.Split('.')[1];
            var jsonBytes = ParseBase64WithoutPadding(payload);
            var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

            keyValuePairs.TryGetValue("nameid", out var nameid);
            if (nameid != null)
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, nameid.ToString()));
            }

            keyValuePairs.TryGetValue("unique_name", out var uniqueName);
            if (uniqueName != null)
            {
                claims.Add(new Claim(ClaimTypes.Name, uniqueName.ToString()));
            }

            keyValuePairs.TryGetValue("email", out var email);
            if (email != null)
            {
                claims.Add(new Claim(ClaimTypes.Email, email.ToString()));
            }

            keyValuePairs.TryGetValue("role", out var role);
            if (role != null)
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

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
