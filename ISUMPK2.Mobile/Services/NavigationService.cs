using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.Services
{
    public interface INavigationService
    {
        Task NavigateToAsync(string route);
        Task NavigateToAsync(string route, bool forceLoad);
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

        // Добавляем реализацию метода с параметром forceLoad
        public async Task NavigateToAsync(string route, bool forceLoad)
        {
            if (forceLoad)
            {
                // При необходимости форсированной загрузки, можно сначала
                // перейти на пустую страницу и затем на целевую
                // или использовать другие подходы в зависимости от потребностей

                // Вариант 1: Для абсолютных URL с "///"
                if (route.StartsWith("///"))
                {
                    await Shell.Current.GoToAsync("//"); // Сначала на корень
                    await Shell.Current.GoToAsync(route); // Затем по маршруту
                    return;
                }

                // Вариант 2: Для относительных URL
                await Shell.Current.GoToAsync(route);
            }
            else
            {
                // Стандартная навигация
                await Shell.Current.GoToAsync(route);
            }
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