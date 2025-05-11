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
using ISUMPK2.Web.Repositories;
using ISUMPK2.Application.Auth;
using ISUMPK2.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using MudBlazor;
using Microsoft.AspNetCore.Components.Routing;

namespace ISUMPK2.Web
{
    public partial class App : ComponentBase { }
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<Web.App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            
            // Настройка HTTP клиента
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.Configuration["API_URL"] ?? "https://localhost:7110") });

            // Добавление сервисов MudBlazor
            builder.Services.AddMudServices(config =>
            {
                config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.BottomRight;
                config.SnackbarConfiguration.PreventDuplicates = false;
                config.SnackbarConfiguration.NewestOnTop = false;
                config.SnackbarConfiguration.ShowCloseIcon = true;
                config.SnackbarConfiguration.VisibleStateDuration = 4000;
                config.SnackbarConfiguration.HideTransitionDuration = 500;
                config.SnackbarConfiguration.ShowTransitionDuration = 500;
                config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
            });


            // Настройка аутентификации
            builder.Services.AddOptions();
            builder.Services.AddAuthorizationCore();
            builder.Services.AddScoped<AuthenticationStateProvider, ISUMPK2.Web.Auth.ApiAuthenticationStateProvider>();
            builder.Services.AddScoped<ISUMPK2.Web.Auth.ApiAuthenticationStateProvider>(); // если требуется напрямую
            builder.Services.AddScoped<IAuthService, AuthService>();
            // Локальное хранилище
            builder.Services.AddScoped<ILocalStorageService, LocalStorageService>();

            // Сервис навигации
            builder.Services.AddScoped<INavigationService, NavigationService>();

            // Регистрация репозиториев - замените на клиентские реализации
            builder.Services.AddScoped<INotificationRepository, ClientNotificationRepository>();
            builder.Services.AddScoped<IMaterialRepository, ClientMaterialRepository>();
            builder.Services.AddScoped<IDepartmentRepository, ClientDepartmentRepository>();
            builder.Services.AddScoped<IUserRepository, ClientUserRepository>();
            builder.Services.AddScoped<ITaskRepository, ClientTaskRepository>();
            builder.Services.AddScoped<IProductRepository, ClientProductRepository>();
            builder.Services.AddScoped(typeof(IRepository<>), typeof(ClientRepository<>));
            builder.Services.AddScoped<IMaterialCategoryRepository, ClientMaterialCategoryRepository>();



            // Уберите дублирующиеся регистрации MudServices

            // Регистрация сервисов приложения
            builder.Services.AddScoped<ITaskService, TaskService>();
            builder.Services.AddScoped<IUserService, ClientUserService>();
            builder.Services.AddScoped<IMaterialService, MaterialService>();
            builder.Services.AddScoped<IProductService, ProductService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IChatService, ChatService>();
            builder.Services.AddScoped<IDepartmentService, DepartmentService>();
            builder.Services.AddScoped<IJwtTokenGenerator, WebAssemblyJwtTokenGenerator>();
            builder.Services.AddScoped<IPasswordHasher<User>, DummyPasswordHasher>();
            // Настройка SignalR для уведомлений
            builder.Services.AddSingleton<INotificationHubService, NotificationHubService>();
            builder.Services.AddSingleton<IChatHubService, ChatHubService>();

            await builder.Build().RunAsync();
        }
    }
}
