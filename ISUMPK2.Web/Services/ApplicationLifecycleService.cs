using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public class WebApplicationLifecycleService : IApplicationLifecycleService
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly NavigationManager _navigationManager;
        private bool _isShuttingDown;

        public WebApplicationLifecycleService(IJSRuntime jsRuntime, NavigationManager navigationManager)
        {
            _jsRuntime = jsRuntime;
            _navigationManager = navigationManager;
            _isShuttingDown = false;
        }

        public bool IsShuttingDown => _isShuttingDown;

        public async Task ExitApplication()
        {
            _isShuttingDown = true;
            await _jsRuntime.InvokeVoidAsync("appFunctions.exitApp");
        }

        public Task RestartApplication()
        {
            _navigationManager.NavigateTo("/", true);
            return Task.CompletedTask;
        }
    }
}
