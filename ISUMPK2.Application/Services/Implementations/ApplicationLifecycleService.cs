using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ISUMPK2.Application.Services.Implementations
{
    public class ApplicationLifecycleService : IApplicationLifecycleService
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private bool _isShuttingDown;

        public ApplicationLifecycleService(IHostApplicationLifetime hostApplicationLifetime)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
        }

        public bool IsShuttingDown => _isShuttingDown;

        public Task ExitApplication()
        {
            _isShuttingDown = true;
            _hostApplicationLifetime.StopApplication();
            return Task.CompletedTask;
        }

        public Task RestartApplication()
        {
            // Запускаем асинхронное завершение с перезапуском
            _isShuttingDown = true;

            // Создаем отдельный поток для перезапуска после небольшой задержки
            Thread restartThread = new Thread(() =>
            {
                Thread.Sleep(1000); // Даем время для завершения текущих операций
                _hostApplicationLifetime.StopApplication();

                // Здесь можно добавить код для перезапуска приложения,
                // если не используется внешний оркестратор или системный сервис
            });

            restartThread.Start();
            return Task.CompletedTask;
        }
    }
}
