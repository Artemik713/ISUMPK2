// ISUMPK2.Web/Services/IAuthService.cs
using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public interface IAuthService
    {
        event Action<bool> AuthenticationChanged;
        Task<LoginResult> Login(LoginModel loginModel);
        Task Logout();
        Task<bool> IsUserAuthenticated();
        Task<UserModel> GetUserInfoAsync();
        Task<bool> ChangePasswordAsync(string currentPassword, string newPassword);
        Task<string> GetTokenAsync();
    }
}
