using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ISUMPK2.Mobile.Models;
using ISUMPK2.Mobile.Services;
using Microsoft.Maui.Controls;

namespace ISUMPK2.Mobile.ViewModels
{
    public class TaskListViewModel : INotifyPropertyChanged
    {
        private readonly ITaskService _taskService;
        private readonly INavigationService _navigationService;
        private bool _isLoading;
        private string _searchQuery;
        private ObservableCollection<TaskModel> _tasks;

        public string Title => "Задачи";

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

        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                if (_searchQuery != value)
                {
                    _searchQuery = value;
                    OnPropertyChanged();
                    FilterTasks();
                }
            }
        }

        public ObservableCollection<TaskModel> Tasks
        {
            get => _tasks;
            set
            {
                if (_tasks != value)
                {
                    _tasks = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand RefreshCommand { get; }
        public ICommand ViewTaskCommand { get; }
        public ICommand CreateTaskCommand { get; }

        public TaskListViewModel(ITaskService taskService, INavigationService navigationService)
        {
            _taskService = taskService;
            _navigationService = navigationService;

            RefreshCommand = new Command(async () => await LoadTasksAsync());
            ViewTaskCommand = new Command<Guid>(ViewTask);
            CreateTaskCommand = new Command(CreateTask);

            Tasks = new ObservableCollection<TaskModel>();
        }

        public async Task LoadTasksAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                // Используем GetAllTasksAsync вместо GetTasksAsync
                var tasks = await _taskService.GetAllTasksAsync();
                Tasks = new ObservableCollection<TaskModel>(tasks);
            }
            catch (Exception ex)
            {
                // Обработка ошибки
                await Shell.Current.DisplayAlert("Ошибка", $"Не удалось загрузить задачи: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void FilterTasks()
        {
            // Реализация фильтрации задач по поисковому запросу
        }

        private void ViewTask(Guid taskId)
        {
            _navigationService.NavigateToAsync($"task-details?id={taskId}");
        }

        private void CreateTask()
        {
            _navigationService.NavigateToAsync("create-task");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}