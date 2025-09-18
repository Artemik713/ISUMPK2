using ISUMPK2.Web.Pages.User;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public interface IThemeService
    {
        Task SetThemeAsync(UserSettings.ThemeMode themeMode, string themeColor);
    }
}