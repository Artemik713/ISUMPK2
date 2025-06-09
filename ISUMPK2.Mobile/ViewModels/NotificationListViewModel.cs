using ISUMPK2.Application.Services;
using ISUMPK2.Mobile.Models;
using ISUMPK2.Mobile.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ISUMPK2.Mobile.ViewModels
{
    public class NotificationListViewModel : INotifyPropertyChanged
    {
        private readonly INotificationService _notificationService;
        private readonly INavigationService _navigationService;
        private readonly IAuthService _authService;
        private bool _isLoading;
        private ObservableCollection<NotificationModel> _notifications;

        public string Title => "Уведомления";

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<NotificationModel> Notifications
        {
            get => _notifications;
            set
            {
                if (_notifications != value)
                {
                    _notifications = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ViewNotificationCommand { get; }
        public ICommand MarkAllAsReadCommand { get; }
        public ICommand MarkAsReadCommand { get; }

        public NotificationListViewModel(INotificationService notificationService, INavigationService navigationService, IAuthService authService)
        {
            _notificationService = notificationService;
            _navigationService = navigationService;
            _authService = authService;

            RefreshCommand = new Command(async () => await LoadNotificationsAsync());
            ViewNotificationCommand = new Command<Guid>(ViewNotification);
            MarkAllAsReadCommand = new Command(async () => await MarkAllAsReadAsync());
            MarkAsReadCommand = new Command<Guid>(async (id) => await MarkAsReadAsync(id));

            Notifications = new ObservableCollection<NotificationModel>();
            
        }

        public async Task LoadNotificationsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                var user = await _authService.GetCurrentUserAsync();
                if (user?.Id == null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить информацию о пользователе", "OK");
                    return;
                }

                // Получаем DTO-объекты уведомлений
                var notificationDtos = await _notificationService.GetAllNotificationsForUserAsync(user.Id.Value);

                // Преобразуем DTO в модели
                var notificationModels = notificationDtos.Select(dto => new NotificationModel
                {
                    Id = dto.Id,
                    UserId = dto.UserId,
                    Title = dto.Title,
                    Message = dto.Message,
                    IsRead = dto.IsRead,
                    TaskId = dto.TaskId,
                    TaskTitle = dto.TaskTitle,
                    CreatedAt = dto.CreatedAt
                }).ToList();

                Notifications = new ObservableCollection<NotificationModel>(notificationModels);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить уведомления: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void ViewNotification(Guid notificationId)
        {
            try
            {
                var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null && !notification.IsRead)
                {
                    await _notificationService.MarkAsReadAsync(notificationId);
                    notification.IsRead = true;
                }

                // Если уведомление связано с задачей, перейти к ней
                if (notification?.TaskId.HasValue == true)
                {
                    _navigationService.NavigateToAsync($"task-details?id={notification.TaskId}");
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось обработать уведомление: {ex.Message}", "OK");
            }
        }

        private async Task MarkAsReadAsync(Guid notificationId)
        {
            try
            {
                await _notificationService.MarkAsReadAsync(notificationId);
                var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось отметить уведомление как прочитанное: {ex.Message}", "OK");
            }
        }

        private async Task MarkAllAsReadAsync()
        {
            try
            {
                // Используем доступный метод из интерфейса для получения пользователя
                var user = await _authService.GetCurrentUserAsync(); // или GetUserInfoAsync()
                if (user?.Id == null)
                {
                    await Shell.Current.DisplayAlert("Ошибка", "Не удалось получить информацию о пользователе", "OK");
                    return;
                }

                await _notificationService.MarkAllAsReadForUserAsync(user.Id.Value);
                foreach (var notification in Notifications)
                {
                    notification.IsRead = true;
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось отметить все уведомления как прочитанные: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}