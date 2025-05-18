using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using WorkTask = ISUMPK2.Domain.Entities.WorkTask; // Переименовал псевдоним для ясности

namespace ISUMPK2.Web.Repositories
{
    public class ClientTaskRepository : ClientRepositoryBase<WorkTask>, ITaskRepository
    {
        protected override string ApiEndpoint => "api/tasks";

        public ClientTaskRepository(HttpClient httpClient) : base(httpClient)
        {
        }
        public override async Task<WorkTask> GetByIdAsync(Guid id)
        {
            try
            {
                var task = await HttpClient.GetFromJsonAsync<WorkTask>($"{ApiEndpoint}/{id}");
                return task;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке задачи {id}: {ex.Message}");
                throw;
            }
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByAssignedToAsync(Guid userId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/user/{userId}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByCreatedByAsync(Guid userId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/creator/{userId}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByDepartmentAsync(Guid departmentId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/department/{departmentId}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByStatusAsync(string status)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/status/{Uri.EscapeDataString(status)}");
        }

        public async System.Threading.Tasks.Task UpdateStatusAsync(Guid taskId, string newStatus)
        {
            var response = await HttpClient.PutAsync(
                $"{ApiEndpoint}/{taskId}/status/{Uri.EscapeDataString(newStatus)}",
                null);
            response.EnsureSuccessStatusCode();
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByStatusAsync(int statusId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/status/{statusId}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByAssigneeAsync(Guid assigneeId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/assignee/{assigneeId}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByCreatorAsync(Guid creatorId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/creator/{creatorId}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByPriorityAsync(int priority)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/priority/{priority}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByDueDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/duedate?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksByProductAsync(Guid productId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/product/{productId}");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetOverdueTasks()
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/overdue");
        }

        public async System.Threading.Tasks.Task<IEnumerable<WorkTask>> GetTasksForDashboardAsync(Guid userId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<WorkTask>>($"{ApiEndpoint}/dashboard/{userId}");
        }
        public override async Task UpdateAsync(WorkTask entity)
        {
            // Отправляем PUT-запрос на правильный URL с ID в URL-пути
            var response = await HttpClient.PutAsJsonAsync($"{ApiEndpoint}/{entity.Id}", entity);
            response.EnsureSuccessStatusCode();
        }

    }
}
