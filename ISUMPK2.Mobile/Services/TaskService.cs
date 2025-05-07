using Microsoft.Maui.Networking;
using ISUMPK2.Mobile.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ISUMPK2.Application.DTOs;

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

    }

    public class TaskService : ITaskService
    {
        private readonly IApiService _apiService;
        private readonly IOfflineDataService _offlineDataService;
        private readonly ISettingsService _settingsService;
        private readonly IConnectivity _connectivity;

        public TaskService(IApiService apiService, IOfflineDataService offlineDataService,
                           ISettingsService settingsService, IConnectivity connectivity)
        {
            _apiService = apiService;
            _offlineDataService = offlineDataService;
            _settingsService = settingsService;
            _connectivity = connectivity;
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

            return await _apiService.PostAsync<TaskCommentModel>("api/tasks/comments", comment);
        }
    }
}
