using ISUMPK2.Application.Services.Implementations;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ISUMPK2.Web.Services;
using ISUMPK2.Web.Auth;

namespace ISUMPK2.Web
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Настройка HTTP клиента
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_URL"] ?? "https://localhost:7001") });

            // Добавление сервисов MudBlazor
            builder.Services.AddMudServices();
            // Настройка аутентификации
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Регистрация сервисов
            IServiceCollection serviceCollection = builder.Services.AddScoped<ITaskService, ITaskService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IMaterialService, IMaterialService>();
            builder.Services.AddScoped<IProductService, IProductService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IChatService, IChatService>();
            builder.Services.AddScoped<IDepartmentService, IDepartmentService>();

            // Настройка SignalR для уведомлений
            builder.Services.AddSingleton<INotificationHubService, INotificationHubService>();
            builder.Services.AddSingleton<IChatHubService, ChatHubService>();

            builder.Build().RunAsync();
        }
    }

    public interface INavigationService
    {
        Task DisplayAlertAsync(string v1, string v2, string v3);
        void NavigateTo(string url, bool forceLoad = false);
        Task NavigateToAsync(string v);
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

        Task INavigationService.DisplayAlertAsync(string v1, string v2, string v3)
        {
            throw new NotImplementedException();
        }

        void INavigationService.NavigateTo(string url, bool forceLoad)
        {
            throw new NotImplementedException();
        }

        Task INavigationService.NavigateToAsync(string v)
        {
            throw new NotImplementedException();
        }
    }
}