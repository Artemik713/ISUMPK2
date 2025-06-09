using Microsoft.Maui.Storage;
using System;
using System.Threading.Tasks;

namespace ISUMPK2.Mobile.Services
{
    public interface ISettingsService
    {
        string ApiUrl { get; }
        Task<string> GetAuthToken();
        Task SetAuthToken(string token);
        Task ClearAuthToken();
        Task<Guid?> GetUserId();
        Task SetUserId(Guid userId);
        Task ClearUserId();
        Task<string> GetUserName();
        Task SetUserName(string userName);
        Task ClearUserName();
        Task<bool> GetOfflineMode();
        Task SetOfflineMode(bool enabled);
        T GetSetting<T>(string key, T defaultValue);
        void SetSetting<T>(string key, T value);
    }

    public class SettingsService : ISettingsService
    {
        private const string AuthTokenKey = "auth_token";
        private const string UserIdKey = "user_id";
        private const string UserNameKey = "user_name";
        private const string OfflineModeKey = "offline_mode";

        private readonly ISecureStorage _secureStorage;
        private readonly IPreferences _preferences;

        public string ApiUrl
        {
            get
            {
#if     ANDROID
                // Для Android эмулятора используем специальный IP
                return "https://10.0.2.2:7110/"; 
#else
                // Для других платформ
                return "https://localhost:7110/";
#endif
            }
        }


        public SettingsService(ISecureStorage secureStorage)
        {
            _secureStorage = secureStorage;
            _preferences = Preferences.Default;
        }

        public async Task<string> GetAuthToken()
        {
            return await _secureStorage.GetAsync(AuthTokenKey);
        }

        public async Task SetAuthToken(string token)
        {
            await _secureStorage.SetAsync(AuthTokenKey, token);
        }

        public Task ClearAuthToken()
        {
            _secureStorage.Remove(AuthTokenKey);
            return Task.CompletedTask;
        }
        public async Task<Guid?> GetUserId()
        {
            var userId = _preferences.Get(UserIdKey, string.Empty);
            if (string.IsNullOrEmpty(userId))
                return null;

            if (Guid.TryParse(userId, out var result))
                return result;

            return null;
        }
        public Task SetUserId(Guid userId)
        {
            _preferences.Set(UserIdKey, userId.ToString());
            return Task.CompletedTask;
        }

        public Task ClearUserId()
        {
            _preferences.Remove(UserIdKey);
            return Task.CompletedTask;
        }

        public Task<string> GetUserName()
        {
            var userName = _preferences.Get(UserNameKey, string.Empty);
            return Task.FromResult(userName);
        }

        public Task SetUserName(string userName)
        {
            _preferences.Set(UserNameKey, userName);
            return Task.CompletedTask;
        }

        public Task ClearUserName()
        {
            _preferences.Remove(UserNameKey);
            return Task.CompletedTask;
        }

        public Task<bool> GetOfflineMode()
        {
            var offlineMode = _preferences.Get(OfflineModeKey, false);
            return Task.FromResult(offlineMode);
        }
        public Task SetOfflineMode(bool enabled)
        {
            _preferences.Set(OfflineModeKey, enabled);
            return Task.CompletedTask;
        }
        public T GetSetting<T>(string key, T defaultValue)
        {
            if (Preferences.ContainsKey(key))
            {
                // Определяем тип и получаем соответствующее значение
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)Preferences.Get(key, (bool)(object)defaultValue);
                }
                else if (typeof(T) == typeof(string))
                {
                    return (T)(object)Preferences.Get(key, (string)(object)defaultValue);
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)Preferences.Get(key, (int)(object)defaultValue);
                }
                else if (typeof(T) == typeof(double))
                {
                    return (T)(object)Preferences.Get(key, (double)(object)defaultValue);
                }
                else if (typeof(T) == typeof(float))
                {
                    return (T)(object)Preferences.Get(key, (float)(object)defaultValue);
                }
                else if (typeof(T) == typeof(long))
                {
                    return (T)(object)Preferences.Get(key, (long)(object)defaultValue);
                }
                else if (typeof(T) == typeof(DateTime))
                {
                    var ticks = Preferences.Get(key, ((DateTime)(object)defaultValue).Ticks);
                    return (T)(object)new DateTime(ticks);
                }
            }

            return defaultValue;
        }
        public void SetSetting<T>(string key, T value)
        {
            if (typeof(T) == typeof(bool))
            {
                Preferences.Set(key, (bool)(object)value);
            }
            else if (typeof(T) == typeof(string))
            {
                Preferences.Set(key, (string)(object)value);
            }
            else if (typeof(T) == typeof(int))
            {
                Preferences.Set(key, (int)(object)value);
            }
            else if (typeof(T) == typeof(double))
            {
                Preferences.Set(key, (double)(object)value);
            }
            else if (typeof(T) == typeof(float))
            {
                Preferences.Set(key, (float)(object)value);
            }
            else if (typeof(T) == typeof(long))
            {
                Preferences.Set(key, (long)(object)value);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                Preferences.Set(key, ((DateTime)(object)value).Ticks);
            }
        }
    }
}
