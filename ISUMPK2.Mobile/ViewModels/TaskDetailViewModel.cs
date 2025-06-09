using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ISUMPK2.Mobile.Models;
using ISUMPK2.Mobile.Services;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.ViewModels
{
    [QueryProperty(nameof(TaskId), "id")]
    public class TaskDetailViewModel : INotifyPropertyChanged
    {
        private readonly ITaskService _taskService;
        private readonly INavigationService _navigationService;
        private Guid _taskId;
        private TaskModel _task;
        private bool _isLoading;
        private bool _isEditing;

        public Guid TaskId
        {
            get => _taskId;
            set
            {
                if (_taskId != value)
                {
                    _taskId = value;
                    OnPropertyChanged();
                    LoadTaskAsync().ConfigureAwait(false);
                }
            }
        }

        public TaskModel Task
        {
            get => _task;
            set
            {
                if (_task != value)
                {
                    _task = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand UpdateStatusCommand { get; }
        public ICommand EditTaskCommand { get; }
        public ICommand SaveTaskCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        public TaskDetailViewModel(ITaskService taskService, INavigationService navigationService)
        {
            _taskService = taskService;
            _navigationService = navigationService;

            UpdateStatusCommand = new Command<int>(UpdateStatus);
            EditTaskCommand = new Command(() => IsEditing = true);
            SaveTaskCommand = new Command(SaveTask);
            CancelEditCommand = new Command(() => IsEditing = false);
            DeleteTaskCommand = new Command(DeleteTask);
        }

        private async Task LoadTaskAsync()
        {
            if (TaskId == Guid.Empty) return;

            try
            {
                IsLoading = true;
                Task = await _taskService.GetTaskByIdAsync(TaskId);
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить задачу: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void UpdateStatus(int statusId)
        {
            try
            {
                // Создаем объект TaskStatusUpdateModel вместо передачи int напрямую
                var statusUpdate = new TaskStatusUpdateModel
                {
                    StatusId = statusId
                };

                await _taskService.UpdateTaskStatusAsync(TaskId, statusUpdate);
                await LoadTaskAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось обновить статус: {ex.Message}", "OK");
            }
        }

        private async void SaveTask()
        {
            try
            {
                await _taskService.UpdateTaskAsync(Task);
                IsEditing = false;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось сохранить задачу: {ex.Message}", "OK");
            }
        }

        private async void DeleteTask()
        {
            var confirm = await Shell.Current.DisplayAlert("Подтверждение", "Вы действительно хотите удалить задачу?", "Да", "Нет");

            if (confirm)
            {
                try
                {
                    await _taskService.DeleteTaskAsync(TaskId);
                    await _navigationService.NavigateBackAsync();
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Ошибка", $"Не удалось удалить задачу: {ex.Message}", "OK");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}