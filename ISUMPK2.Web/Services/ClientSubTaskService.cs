using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public class ClientSubTaskService : ISubTaskService
    {
        private readonly HttpClient _httpClient;

        public ClientSubTaskService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<SubTaskDto> CreateSubTaskAsync(SubTaskCreateDto subTaskDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/subtasks", subTaskDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SubTaskDto>();
        }

        public async Task DeleteSubTaskAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/subtasks/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<SubTaskDto>> GetByParentTaskIdAsync(Guid parentTaskId)
        {
            try
            {
                // Добавьте обработку ошибок
                var response = await _httpClient.GetAsync($"api/subtasks/task/{parentTaskId}");
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadFromJsonAsync<IEnumerable<SubTaskDto>>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке подзадач: {ex.Message}");
                return new List<SubTaskDto>();
            }
        }

        public async Task<SubTaskDto> GetSubTaskByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<SubTaskDto>($"api/subtasks/{id}");
        }

        public async Task<SubTaskDto> UpdateSubTaskAsync(Guid id, SubTaskUpdateDto subTaskDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/subtasks/{id}", subTaskDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<SubTaskDto>();
        }
        public async Task<IEnumerable<SubTaskDto>> GetAllSubTasksAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<SubTaskDto>>("api/subtasks");
        }

        public async Task<IEnumerable<SubTaskDto>> GetByAssigneeAsync(Guid assigneeId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<SubTaskDto>>($"api/subtasks/assignee/{assigneeId}");
        }
    }
}