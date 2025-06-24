using System.Threading.Tasks;
using ISUMPK2.Web.Pages.User;
using Microsoft.JSInterop;

namespace ISUMPK2.Web.Services
{
    public class ThemeService : IThemeService
    {
        private readonly IJSRuntime _jsRuntime;

        public ThemeService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task SetThemeAsync(UserSettings.ThemeMode themeMode, string themeColor)
        {
            // Вызов JS для установки темы
            await _jsRuntime.InvokeVoidAsync("applyTheme", themeMode.ToString().ToLower(), themeColor);
        }
    }
}