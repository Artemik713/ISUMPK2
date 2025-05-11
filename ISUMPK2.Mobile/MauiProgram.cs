using ISUMPK2.Application.Services.Implementations;
using ISUMPK2.Application.Services;
using ISUMPK2.Mobile.Services;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
using IAuthService = ISUMPK2.Mobile.Services.IAuthService;
using ITaskService = ISUMPK2.Mobile.Services.ITaskService;
using TaskService = ISUMPK2.Mobile.Services.TaskService;
using AuthService = ISUMPK2.Mobile.Services.AuthService;
using Microsoft.Extensions.Logging;
using ISUMPK.Mobile;

namespace ISUMPK2.Mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
#if DEBUG
            HttpsClientHandlerService.Initialize();
#endif
            builder
                .UseMauiApp<App>() // Предполагается, что класс App существует в текущем пространстве имен
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Конфигурация служб
            builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);
            builder.Services.AddSingleton<ISecureStorage>(SecureStorage.Default);

            // Регистрация сервисов
            builder.Services.AddSingleton<IApiService, ApiService>();
            builder.Services.AddSingleton<IAuthService, AuthService>();
            builder.Services.AddSingleton<ITaskService, TaskService>();
            builder.Services.AddSingleton<IMaterialService, MaterialService>();
            builder.Services.AddSingleton<IProductService, ProductService>();
            builder.Services.AddSingleton<INotificationService, NotificationService>();
            builder.Services.AddSingleton<IChatService, ChatService>();
            builder.Services.AddSingleton<ISettingsService, SettingsService>();
            builder.Services.AddSingleton<IOfflineDataService, OfflineDataService>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();

            // ПРИМЕЧАНИЕ: Закомментируйте регистрацию отсутствующих ViewModel и View, 
            // пока не создадите соответствующие классы

            /* Раскомментируйте и используйте по мере создания соответствующих классов
            // Регистрация ViewModels
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<TaskListViewModel>();
            builder.Services.AddTransient<TaskDetailViewModel>();
            builder.Services.AddTransient<MaterialListViewModel>();
            builder.Services.AddTransient<ProductListViewModel>();
            builder.Services.AddTransient<NotificationListViewModel>();
            builder.Services.AddTransient<ChatViewModel>();
            builder.Services.AddTransient<ProfileViewModel>();

            // Регистрация Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<TaskListPage>();
            builder.Services.AddTransient<TaskDetailPage>();
            builder.Services.AddTransient<MaterialListPage>();
            builder.Services.AddTransient<ProductListPage>();
            builder.Services.AddTransient<NotificationListPage>();
            builder.Services.AddTransient<ChatPage>();
            builder.Services.AddTransient<ProfilePage>();
            */

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();

        }
    }
    public static class HttpsClientHandlerService
{
    public static void Initialize()
    {
#if DEBUG
        // Игнорирование ошибок SSL сертификата только в режиме отладки
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = 
                (message, cert, chain, errors) => true
        };
        
        var httpClient = new HttpClient(handler);
#endif
    }
}
}
