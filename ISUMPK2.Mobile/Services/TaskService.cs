using Microsoft.Maui.Networking;
using ISUMPK2.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISUMPK2.Application.DTOs;
using Microsoft.Extensions.Logging;

namespace ISUMPK2.Mobile.Services
{
    public interface ITaskService
    {
        Task<List<TaskModel>> GetAllTasksAsync();
        Task<List<TaskModel>> GetTasksForDashboardAsync();
        Task<List<TaskModel>> GetMyTasksAsync();
        Task<List<TaskModel>> GetOverdueTasksAsync();
        Task<List<TaskModel>> GetActiveTasksAsync(); // Добавьте этот метод
        Task<TaskModel> GetTaskByIdAsync(Guid id);
        Task<TaskModel> UpdateTaskStatusAsync(Guid id, TaskStatusUpdateModel statusUpdate);
        Task<TaskCommentModel> AddCommentAsync(TaskCommentCreateModel comment);
        Task<TaskModel> UpdateTaskAsync(TaskModel task);
        Task DeleteTaskAsync(Guid id);
    }

    public class TaskService : ITaskService
    {
        private readonly IApiService _apiService;
        private readonly IOfflineDataService _offlineDataService;
        private readonly ISettingsService _settingsService;
        private readonly IConnectivity _connectivity;
        private readonly ILogger<TaskService> _logger;

        public TaskService(IApiService apiService, IOfflineDataService offlineDataService,
                           ISettingsService settingsService, IConnectivity connectivity, ILogger<TaskService> logger)
        {
            _apiService = apiService;
            _offlineDataService = offlineDataService;
            _settingsService = settingsService;
            _connectivity = connectivity;
            _logger = logger;

        }

        public async Task<List<TaskModel>> GetAllTasksAsync()
        {
            try
            {
                // Проверяем подключение к интернету
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    // Если нет подключения, возвращаем данные из локального хранилища
                    return await _offlineDataService.GetTasksAsync();
                }

                // Если есть подключение, получаем данные с сервера
                var tasks = await _apiService.GetAsync<List<TaskModel>>("api/tasks");

                // Сохраняем данные в локальное хранилище
                await _offlineDataService.SaveTasksAsync(tasks);

                return tasks;
            }
            catch (Exception)
            {
                // В случае ошибки пытаемся вернуть данные из локального хранилища
                return await _offlineDataService.GetTasksAsync();
            }
        }

        public async Task<List<TaskModel>> GetTasksForDashboardAsync()
        {
            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    return await _offlineDataService.GetTasksAsync();
                }

                var tasks = await _apiService.GetAsync<List<TaskModel>>("api/tasks/dashboard");
                await _offlineDataService.SaveTasksAsync(tasks);
                return tasks;
            }
            catch (Exception)
            {
                return await _offlineDataService.GetTasksAsync();
            }
        }

        public async Task<List<TaskModel>> GetActiveTasksAsync()
        {
            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    var allTasks = await _offlineDataService.GetTasksAsync();
                    return allTasks.Where(t => t.StatusId != 5 && t.StatusId != 6).ToList();
                }

                var tasks = await _apiService.GetAsync<List<TaskModel>>("api/tasks/active");
                await _offlineDataService.SaveTasksAsync(tasks);
                return tasks;
            }
            catch (Exception)
            {
                var allTasks = await _offlineDataService.GetTasksAsync();
                return allTasks.Where(t => t.StatusId != 5 && t.StatusId != 6).ToList();
            }
        }



        public async Task<List<TaskModel>> GetMyTasksAsync()
        {
            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    return await _offlineDataService.GetTasksAsync();
                }

                var tasks = await _apiService.GetAsync<List<TaskModel>>("api/tasks/my-tasks");
                await _offlineDataService.SaveTasksAsync(tasks);
                return tasks;
            }
            catch (Exception)
            {
                return await _offlineDataService.GetTasksAsync();
            }
        }

        public async Task<List<TaskModel>> GetOverdueTasksAsync()
        {
            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    var allTasks = await _offlineDataService.GetTasksAsync();
                    return allTasks.FindAll(t => t.IsOverdue);
                }

                var tasks = await _apiService.GetAsync<List<TaskModel>>("api/tasks/overdue");
                return tasks;
            }
            catch (Exception)
            {
                var allTasks = await _offlineDataService.GetTasksAsync();
                return allTasks.FindAll(t => t.IsOverdue);
            }
        }

        public async Task<TaskModel> GetTaskByIdAsync(Guid id)
        {
            try
            {
                if (_connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    return await _offlineDataService.GetTaskAsync(id);
                }

                var task = await _apiService.GetAsync<TaskModel>($"api/tasks/{id}");
                await _offlineDataService.SaveTaskAsync(task);
                return task;
            }
            catch (Exception)
            {
                return await _offlineDataService.GetTaskAsync(id);
            }
        }

        public async Task<TaskModel> UpdateTaskStatusAsync(Guid id, TaskStatusUpdateModel statusUpdate)
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                throw new Exception("Для изменения статуса задачи требуется подключение к интернету");
            }

            var updatedTask = await _apiService.PatchAsync<TaskModel>($"api/tasks/{id}/status", statusUpdate);
            await _offlineDataService.SaveTaskAsync(updatedTask);
            return updatedTask;
        }

        public async Task<TaskCommentModel> AddCommentAsync(TaskCommentCreateModel comment)
        {
            if (_connectivity.NetworkAccess != NetworkAccess.Internet)
            {
                throw new Exception("Для добавления комментария требуется подключение к интернету");
            }

            try
            {
                // Преобразование модели комментария в DTO для API
                var commentDto = new
                {
                    TaskId = comment.TaskId,
                    Comment = comment.Comment
                };

                // Вызов API для добавления комментария
                var result = await _apiService.PostAsync<TaskCommentModel>("api/tasks/comments", commentDto);

                // Обновление локальной задачи с новым комментарием
                var task = await _offlineDataService.GetTaskAsync(comment.TaskId);
                if (task != null)
                {
                    if (task.Comments == null)
                        task.Comments = new List<TaskCommentModel>();

                    task.Comments.Add(result);
                    await _offlineDataService.SaveTaskAsync(task);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении комментария: {ex.Message}");
                throw;
            }
        }
        public async Task<TaskModel> UpdateTaskAsync(TaskModel task)
        {
            try
            {
                // Преобразуем TaskModel в TaskUpdateDto для API
                var taskUpdateDto = new TaskUpdateDto
                {
                    Title = task.Title,
                    Description = task.Description,
                    StatusId = task.StatusId,
                    PriorityId = task.PriorityId,
                    AssigneeId = task.AssigneeId,
                    DueDate = task.DueDate,
                    EstimatedHours = task.EstimatedHours,
                    ProductId = task.ProductId,
                    Quantity = task.Quantity
                    // Добавьте другие необходимые поля
                };

                // Отправляем на сервер
                var response = await _apiService.PutAsync<TaskDto>($"api/tasks/{task.Id}", taskUpdateDto);

                // Обновляем локальные данные
                await _offlineDataService.SaveTaskAsync(task);

                return task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при обновлении задачи {task.Id}");
                throw;
            }
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            try
            {
                // Отправляем запрос на удаление на сервер
                await _apiService.DeleteAsync($"api/tasks/{id}");

                // Удаляем локальные данные, если нужно
                // Например: await _offlineDataService.DeleteTaskAsync(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Ошибка при удалении задачи {id}");
                throw;
            }
        }
    }
}
