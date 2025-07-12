using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public class ClientTaskService : ITaskService
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorageService;

        public ClientTaskService(HttpClient httpClient, ILocalStorageService localStorageService)
        {
            _httpClient = httpClient;
            _localStorageService = localStorageService;
        }

        private async Task SetAuthorizationHeaderAsync()
        {
            var token = await _localStorageService.GetItemAsync<string>("authToken");
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
        }

        public async Task<TaskDto> GetTaskByIdAsync(Guid id)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<TaskDto>($"api/tasks/{id}");
        }

        public async Task<IEnumerable<TaskDto>> GetActiveTasksAsync()
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>("api/tasks/active");
        }

        public async Task<IEnumerable<TaskDto>> GetAllTasksAsync()
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>("api/tasks");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByStatusAsync(int statusId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/by-status/{statusId}");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByAssigneeAsync(Guid assigneeId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/by-assignee/{assigneeId}");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByCreatorAsync(Guid creatorId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/by-creator/{creatorId}");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByDepartmentAsync(Guid departmentId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/by-department/{departmentId}");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByPriorityAsync(int priorityId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/by-priority/{priorityId}");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/by-due-date?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksByProductAsync(Guid productId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/by-product/{productId}");
        }

        public async Task<IEnumerable<TaskDto>> GetOverdueTasksAsync()
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>("api/tasks/overdue");
        }

        public async Task<IEnumerable<TaskDto>> GetTasksForDashboardAsync(Guid userId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskDto>>($"api/tasks/dashboard");
        }

        public async Task<TaskDto> CreateTaskAsync(Guid creatorId, TaskCreateDto taskDto)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/tasks", taskDto);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<TaskDto>();
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка при создании задачи: {response.StatusCode}, {errorContent}");
                throw new Exception($"Ошибка при создании задачи: {response.StatusCode}");
            }
        }

        public async Task<TaskDto> UpdateTaskAsync(Guid id, TaskUpdateDto taskDto)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PutAsJsonAsync($"api/tasks/{id}", taskDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskDto>();
        }

        public async Task<TaskDto> UpdateTaskStatusAsync(Guid id, Guid userId, TaskStatusUpdateDto statusDto)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PatchAsync($"api/tasks/{id}/status", JsonContent.Create(statusDto));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskDto>();
        }

        public async Task DeleteTaskAsync(Guid id)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.DeleteAsync($"api/tasks/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<TaskCommentDto> AddCommentAsync(Guid userId, TaskCommentCreateDto commentDto)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync("api/tasks/comments", commentDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskCommentDto>();
        }

        public async Task<IEnumerable<TaskCommentDto>> GetCommentsByTaskIdAsync(Guid taskId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskCommentDto>>($"api/tasks/{taskId}/comments");
        }
        public async Task<IEnumerable<TaskMaterialDto>> GetTaskMaterialsAsync(Guid taskId)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<TaskMaterialDto>>($"api/TaskMaterials/task/{taskId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting task materials: {ex.Message}");
                return new List<TaskMaterialDto>();
            }
        }

        public async Task ReserveMaterialsAsync(Guid taskId, List<TaskMaterialCreateDto> materials)
        {
            await SetAuthorizationHeaderAsync();
            var response = await _httpClient.PostAsJsonAsync($"api/tasks/{taskId}/reserve-materials", materials);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка при резервировании материалов: {response.StatusCode}, {errorContent}");
                response.EnsureSuccessStatusCode();
            }
        }
        public async Task UpdateTaskMaterialsAsync(Guid taskId, List<TaskMaterialCreateDto> materials)
        {
            try
            {
                await SetAuthorizationHeaderAsync();

                // Проверьте URL - он должен быть api/tasks/{id}/materials, а не api/tasks/{id}/m
                var response = await _httpClient.PutAsJsonAsync($"api/tasks/{taskId}/materials", materials);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Error updating task materials: {response.StatusCode}, Content: {errorContent}");
                    response.EnsureSuccessStatusCode();
                }

                // Резервировать материалы уже должно на стороне API при вызове UpdateTaskMaterialsAsync
                // Закомментируйте эту строку, если резервирование уже происходит на API
                // await ReserveMaterialsAsync(taskId, materials);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating task materials: {ex.Message}");
                throw;
            }
        }
        public async Task<TaskDependencyInfoDto> GetTaskDependenciesAsync(Guid taskId)
        {
            await SetAuthorizationHeaderAsync();
            return await _httpClient.GetFromJsonAsync<TaskDependencyInfoDto>($"api/tasks/{taskId}/dependencies");
        }
        public async Task ForceDeleteTaskAsync(Guid taskId)
        {
            await SetAuthorizationHeaderAsync();

            try
            {
                Console.WriteLine($"Принудительное удаление задачи {taskId}");

                var response = await _httpClient.DeleteAsync($"api/tasks/{taskId}/force");

                Console.WriteLine($"Ответ сервера для принудительного удаления: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка принудительного удаления задачи {taskId}: {response.StatusCode}, {errorContent}");

                    var httpEx = new HttpRequestException($"Ошибка при принудительном удалении задачи: {response.StatusCode}")
                    {
                        Data = { { "StatusCode", response.StatusCode } }
                    };

                    if (httpEx.GetType().GetProperty("StatusCode") != null)
                    {
                        httpEx.GetType().GetProperty("StatusCode")?.SetValue(httpEx, response.StatusCode);
                    }

                    throw httpEx;
                }

                Console.WriteLine($"Задача {taskId} принудительно удалена");
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Неожиданная ошибка при принудительном удалении задачи {taskId}: {ex.Message}");
                throw new ApplicationException($"Неожиданная ошибка при принудительном удалении задачи: {ex.Message}", ex);
            }
        }
    }
}
