using ISUMPK2.Application.Services.Implementations;
using ISUMPK2.Application.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using ISUMPK2.Web.Services;
using ISUMPK2.Web.Auth;
using ISUMPK2.Infrastructure.Repositories;
using ISUMPK2.Domain.Repositories;

namespace ISUMPK2.Web
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            // Настройка HTTP клиента
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_URL"] ?? "https://localhost:7110") });

            // Добавление сервисов MudBlazor
            builder.Services.AddMudServices();

            // Настройка аутентификации
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<ApiAuthenticationStateProvider>(); // если требуется напрямую
            builder.Services.AddScoped<IAuthService, AuthService>();

            // Локальное хранилище
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

            // Сервис навигации
            builder.Services.AddScoped<INavigationService, NavigationService>();

            // Регистрация репозиториев
            builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
            builder.Services.AddScoped<IMaterialRepository, MaterialRepository>();
            builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITaskRepository, TaskRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Регистрация сервисов приложения
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IUserService, UserService>();
            builder.Services.AddScoped<IMaterialService, MaterialService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();

            // Настройка SignalR для уведомлений
            builder.Services.AddSingleton<INotificationHubService, NotificationHubService>();
            builder.Services.AddSingleton<IChatHubService, ChatHubService>();

            await builder.Build().RunAsync();
        }
    }
}
