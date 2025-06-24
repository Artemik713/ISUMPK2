using ISUMPK2.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public class ClientTaskMaterialService : IClientTaskMaterialService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ClientTaskMaterialService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        public async Task<IEnumerable<TaskMaterialDto>> GetByTaskIdAsync(Guid taskId)
        {
            try
            {
                // Вернуть исходный маршрут
                return await _httpClient.GetFromJsonAsync<IEnumerable<TaskMaterialDto>>($"api/TaskMaterials/task/{taskId}", _jsonOptions);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка при запросе материалов задачи {taskId}: {ex.Message}");
                return new List<TaskMaterialDto>();
            }
        }

        public async Task<IEnumerable<TaskMaterialDto>> GetByMaterialIdAsync(Guid materialId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<TaskMaterialDto>>($"api/TaskMaterials/material/{materialId}");
        }

        public async Task<TaskMaterialDto> CreateAsync(TaskMaterialCreateDto createDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/TaskMaterials", createDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskMaterialDto>();
        }

        public async Task<TaskMaterialDto> UpdateAsync(Guid id, TaskMaterialUpdateDto updateDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/TaskMaterials/{id}", updateDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<TaskMaterialDto>();
        }

        public async Task DeleteAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/TaskMaterials/{id}");
            response.EnsureSuccessStatusCode();
        }
    }
}