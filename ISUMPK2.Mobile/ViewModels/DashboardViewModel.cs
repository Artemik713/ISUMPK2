using ISUMPK2.Application.Services;
using ISUMPK2.Mobile.Models;
using ISUMPK2.Mobile.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;
using MobileNavigationService = ISUMPK2.Mobile.Services.INavigationService;
using AppTaskService = ISUMPK2.Application.Services.ITaskService;
using ISUMPK2.Mobile.Extensions;


namespace ISUMPK2.Mobile.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly AppTaskService _taskService;
        private readonly INotificationService _notificationService;
        private readonly MobileNavigationService _navigationService;
        private readonly ISettingsService _settingsService;

        private ObservableCollection<TaskModel> _tasks;
        private ObservableCollection<NotificationModel> _notifications;
        private int _myTasksCount;
        private int _tasksOnReviewCount;
        private int _overdueTasksCount;

        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks;
            set => SetProperty(ref _tasks, value);
        }

        public ObservableCollection<NotificationModel> Notifications
        {
            get => _notifications;
            set => SetProperty(ref _notifications, value);
        }

        public int MyTasksCount
        {
            get => _myTasksCount;
            set => SetProperty(ref _myTasksCount, value);
        }

        public int TasksOnReviewCount
        {
            get => _tasksOnReviewCount;
            set => SetProperty(ref _tasksOnReviewCount, value);
        }

        public int OverdueTasksCount
        {
            get => _overdueTasksCount;
            set => SetProperty(ref _overdueTasksCount, value);
        }

        public ICommand RefreshCommand { get; }
        public ICommand ViewTaskCommand { get; }
        public ICommand ViewNotificationCommand { get; }
        public ICommand ViewAllTasksCommand { get; }
        public ICommand ViewAllNotificationsCommand { get; }
        public ICommand ViewMyTasksCommand { get; }
        public ICommand ViewOverdueTasksCommand { get; }

        public DashboardViewModel(AppTaskService taskService, INotificationService notificationService,
                             MobileNavigationService navigationService, ISettingsService settingsService)
        {
            _taskService = taskService;
            _notificationService = notificationService;
            _navigationService = navigationService;
            _settingsService = settingsService;

            Tasks = new ObservableCollection<TaskModel>();
            Notifications = new ObservableCollection<NotificationModel>();

            Title = "Панель управления";

            // Создание команд
            RefreshCommand = CreateCommand(async () => await LoadDataAsync());
            ViewTaskCommand = CreateCommand<Guid>(async taskId => await ViewTaskAsync(taskId));
            ViewNotificationCommand = CreateCommand<Guid>(async notificationId => await ViewNotificationAsync(notificationId));
            ViewAllNotificationsCommand = CreateCommand(() => _navigationService.NavigateToAsync("notifications"));
            ViewAllTasksCommand = CreateCommand(() => _navigationService.NavigateToAsync("tasks"));
            ViewMyTasksCommand = CreateCommand(() => _navigationService.NavigateToAsync("tasks?filter=my"));
        }
        public async Task LoadDataAsync()
        {
            await ExecuteAsync(async () =>
            {
                // Получаем текущего пользователя
                var userId = await _settingsService.GetUserId();
                if (!userId.HasValue)
                {
                    // Обработка ситуации, когда пользователь не авторизован
                    return;
                }

                // Загружаем задачи
                var tasksDto = await _taskService.GetTasksForDashboardAsync(userId.Value);
                var tasks = tasksDto.Select(dto => dto.ToMobileModel()).ToList();
                Tasks.Clear();

                foreach (var task in tasks.OrderByDescending(t => t.PriorityId)
                                        .ThenBy(t => t.DueDate)
                                        .Take(5))
                {
                    Tasks.Add(task);
                }

                // Считаем статистику
                MyTasksCount = tasks.Count(t => t.StatusId != 5 && t.StatusId != 6);
                TasksOnReviewCount = tasks.Count(t => t.StatusId == 4);
                OverdueTasksCount = tasks.Count(t => t.IsOverdue);

                // Загружаем уведомления для текущего пользователя
                var notificationsDto = await _notificationService.GetUnreadNotificationsForUserAsync(userId.Value);
                var notifications = notificationsDto
                                    .OrderByDescending(n => n.CreatedAt)
                                    .Take(5)
                                    .Select(dto => dto.ToMobileModel())
                                    .ToList();
                Notifications.Clear();
                foreach (var n in notifications)
                {
                    Notifications.Add(n);
                }
            });
        }


        private async Task ViewTaskAsync(Guid taskId)
        {
            await _navigationService.NavigateToAsync($"tasks/{taskId}");
        }

        private async Task ViewNotificationAsync(Guid notificationId)
        {
            var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);

            if (notification != null && !notification.IsRead)
            {
                await _notificationService.MarkAsReadAsync(notificationId);
                notification.IsRead = true;
            }

            if (notification?.TaskId != null)
            {
                await _navigationService.NavigateToAsync($"tasks/{notification.TaskId}");
            }
            else
            {
                // Показываем уведомление в диалоге
                await _navigationService.DisplayAlertAsync(
                    notification?.Title ?? "Уведомление",
                    notification?.Message ?? "",
                    "OK");
            }
        }

        public override async Task OnAppearingAsync()
        {
            await LoadDataAsync();
        }
    }
}
