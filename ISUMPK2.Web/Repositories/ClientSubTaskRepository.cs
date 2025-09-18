using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Web.Repositories
{
    public class ClientSubTaskRepository : ISubTaskRepository
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ClientSubTaskRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task AddAsync(SubTask entity)
        {
            var response = await _httpClient.PostAsJsonAsync("api/subtasks", entity);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/subtasks/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<SubTask>> FindAsync(System.Linq.Expressions.Expression<Func<SubTask, bool>> predicate)
        {
            // В веб-клиенте нельзя напрямую использовать предикаты LINQ
            // Обычно это реализуется через API с фильтрами
            throw new NotImplementedException("FindAsync не реализован в клиентском репозитории");
        }

        public async Task<IEnumerable<SubTask>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<SubTask>>("api/subtasks", _jsonOptions);
        }

        public async Task<SubTask> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<SubTask>($"api/subtasks/{id}", _jsonOptions);
        }

        public async Task<IEnumerable<SubTask>> GetByParentTaskIdAsync(Guid parentTaskId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<SubTask>>($"api/subtasks/task/{parentTaskId}", _jsonOptions);
        }

        public async Task SaveChangesAsync()
        {
            // В клиентском репозитории этот метод обычно не делает ничего,
            // так как изменения сохраняются сразу при вызове соответствующих методов API
        }
        public async Task<IEnumerable<SubTask>> GetByAssigneeAsync(Guid assigneeId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<SubTask>>($"api/subtasks/assignee/{assigneeId}", _jsonOptions);
        }

        public Task UpdateAsync(SubTask entity)
        {
            return _httpClient.PutAsJsonAsync($"api/subtasks/{entity.Id}", entity);
        }
    }
}