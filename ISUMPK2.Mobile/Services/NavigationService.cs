using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task NavigateBackAsync();
        Task DisplayAlertAsync(string title, string message, string cancel);
        Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel);
    }

    public class NavigationService : INavigationService
    {
        public async Task NavigateToAsync(string route)
        {
            await Shell.Current.GoToAsync(route);
        }

        public async Task NavigateBackAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        public async Task DisplayAlertAsync(string title, string message, string cancel)
        {
            await Shell.Current.DisplayAlert(title, message, cancel);
        }

        public async Task<bool> DisplayConfirmationAsync(string title, string message, string accept, string cancel)
        {
            return await Shell.Current.DisplayAlert(title, message, accept, cancel);
        }
    }
}
