// ISUMPK2.Web/Services/IAuthService.cs
using ISUMPK2.Web.Models;

namespace ISUMPK2.Web.Services
{
    public interface IAuthService
    {
        Task<LoginResult> Login(LoginModel loginModel);
        Task Logout();
        Task<bool> IsUserAuthenticated();
        Task<UserModel> GetUserInfoAsync();
    }
}

