using ISUMPK2.Mobile.Models;

namespace ISUMPK2.Mobile.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string username, string password);
        Task LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<UserModel> GetCurrentUserAsync();
    }

    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly ISettingsService _settingsService;
        private UserModel _currentUser;

        public AuthService(IApiService apiService, ISettingsService settingsService)
        {
            _apiService = apiService;
            _settingsService = settingsService;
        }

        public async Task<bool> LoginAsync(string username, string password)
        {
            try
            {
                var loginModel = new LoginModel
                {
                    UserName = username,
                    Password = password
                };

                var response = await _apiService.PostAsync<UserLoginResponse>("api/auth/login", loginModel);

                if (response != null && !string.IsNullOrEmpty(response.Token))
                {
                    await _apiService.SetAuthToken(response.Token);
                    await _settingsService.SetUserId(response.Id);
                    await _settingsService.SetUserName(response.UserName);

                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task LogoutAsync()
        {
            await _apiService.ClearAuthToken();
            await _settingsService.ClearUserId();
            await _settingsService.ClearUserName();
            _currentUser = null;
        }

        public async Task<bool> IsAuthenticatedAsync()
        {
            var token = await _settingsService.GetAuthToken();
            return !string.IsNullOrEmpty(token) && _apiService.IsAuthenticated;
        }

        public async Task<UserModel> GetCurrentUserAsync()
        {
            if (_currentUser != null)
                return _currentUser;

            try
            {
                _currentUser = await _apiService.GetAsync<UserModel>("api/users/profile");
                return _currentUser;
            }
            catch
            {
                return null;
            }
        }
    }
}
