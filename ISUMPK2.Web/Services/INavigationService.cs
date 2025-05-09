using Microsoft.AspNetCore.Components;

namespace ISUMPK2.Web.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task NavigateBackAsync();
        Task DisplayAlertAsync(string title, string message, string cancel);
        Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel);
        void NavigateTo(string url, bool forceLoad = false);
    }
    
    public class NavigationService : INavigationService
    {
        private readonly NavigationManager _navigationManager;

        public NavigationService(NavigationManager navigationManager)
        {
            _navigationManager = navigationManager;
        }

        public void NavigateTo(string uri, bool forceLoad = false)
        {
            _navigationManager.NavigateTo(uri, forceLoad);
        }

        public Task NavigateToAsync(string route)
        {
            _navigationManager.NavigateTo(route);
            return Task.CompletedTask;
        }

        public Task NavigateBackAsync()
        {
            // В Blazor WebAssembly нет прямого метода для навигации назад
            // Можно использовать JavaScript Interop для этого
            _navigationManager.NavigateTo("javascript:history.back()");
            return Task.CompletedTask;
        }

        public Task DisplayAlertAsync(string title, string message, string cancel)
        {
            // Реализация через MudBlazor DialogService
            // Или через JavaScript Interop
            return Task.CompletedTask;
        }

        public Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel)
        {
            // Реализация через MudBlazor DialogService
            // Или через JavaScript Interop
            return Task.FromResult(false);
        }
    }
}
