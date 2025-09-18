using ISUMPK2.Application.DTOs;
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
        public async Task<WorkTask> GetTaskWithMaterialsAsync(Guid taskId)
        {
            try
            {
                // Сначала получаем базовую информацию о задаче
                var task = await GetByIdAsync(taskId);

                if (task == null)
                {
                    return null;
                }

                // Затем запрашиваем материалы для задачи
                var response = await HttpClient.GetFromJsonAsync<IEnumerable<TaskMaterialDto>>($"{ApiEndpoint}/{taskId}/materials");

                // Если response пустой, вернем задачу без материалов
                if (response == null)
                {
                    task.TaskMaterials = new List<TaskMaterial>();
                    return task;
                }

                // Создаем коллекцию материалов для задачи
                task.TaskMaterials = new List<TaskMaterial>();

                // Заполняем коллекцию материалов
                foreach (var materialDto in response)
                {
                    var taskMaterial = new TaskMaterial
                    {
                        Id = materialDto.Id,
                        TaskId = taskId,
                        MaterialId = materialDto.MaterialId,
                        Quantity = materialDto.Quantity,
                        // Заполняем навигационное свойство Material, если оно используется
                        Material = new Material
                        {
                            Id = materialDto.MaterialId,
                            Name = materialDto.MaterialName,
                            Code = materialDto.MaterialCode,
                            UnitOfMeasure = materialDto.UnitOfMeasure
                        }
                    };

                    task.TaskMaterials.Add(taskMaterial);
                }

                return task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении задачи с материалами: {ex.Message}");
                throw;
            }
        }
        public async Task AddMaterialToTaskAsync(Guid taskId, TaskMaterial taskMaterial)
        {
            var response = await HttpClient.PostAsJsonAsync(
                $"{ApiEndpoint}/{taskId}/materials",
                taskMaterial);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateTaskMaterialAsync(Guid taskId, TaskMaterial taskMaterial)
        {
            var response = await HttpClient.PutAsJsonAsync(
                $"{ApiEndpoint}/{taskId}/materials/{taskMaterial.Id}",
                taskMaterial);
            response.EnsureSuccessStatusCode();
        }

        public async Task RemoveTaskMaterialAsync(Guid taskId, Guid taskMaterialId)
        {
            var response = await HttpClient.DeleteAsync(
                $"{ApiEndpoint}/{taskId}/materials/{taskMaterialId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<TaskMaterial>> GetTaskMaterialsAsync(Guid taskId)
        {
            try
            {
                return await HttpClient.GetFromJsonAsync<IEnumerable<TaskMaterial>>(
                    $"{ApiEndpoint}/{taskId}/materials");
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<TaskMaterial>();
            }
        }
    }
}
