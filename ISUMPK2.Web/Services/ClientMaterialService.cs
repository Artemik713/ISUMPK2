using ISUMPK2.Application.DTOs;
using ISUMPK2.Application.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Services
{
    public class ClientMaterialService : IMaterialService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ClientMaterialService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<MaterialTransactionDto> AddMaterialTransactionAsync(MaterialTransactionCreateDto transactionDto)
        {
            // Убедимся, что TransactionType правильный
            if (transactionDto.TransactionType == "Приход")
                transactionDto.TransactionType = "Receipt";
            else if (transactionDto.TransactionType == "Расход")
                transactionDto.TransactionType = "Issue";

            // Используем прямое API-соединение с эндпоинтом, который работает
            var response = await _httpClient.PostAsJsonAsync($"api/materials/{transactionDto.MaterialId}/transactions", transactionDto);

            // Для отладки
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка API: {response.StatusCode}, содержимое: {content}");
                throw new Exception($"API вернул ошибку: {response.StatusCode}, {content}");
            }

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MaterialTransactionDto>(_jsonOptions);
        }

        public async Task<MaterialTransactionDto> AddTransactionAsync(Guid userId, MaterialTransactionCreateDto transactionDto)
        {
            var response = await _httpClient.PostAsJsonAsync($"api/materials/transactions/user/{userId}", transactionDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MaterialTransactionDto>(_jsonOptions);
        }

        public async Task<MaterialDto> CreateMaterialAsync(MaterialCreateDto model)
        {
            try
            {
                // Логирование отправляемых данных
                string jsonData = System.Text.Json.JsonSerializer.Serialize(model);
                Console.WriteLine($"Отправка данных: {jsonData}");

                var response = await _httpClient.PostAsJsonAsync("api/materials", model);

                if (!response.IsSuccessStatusCode)
                {
                    // Получение подробностей об ошибке
                    string errorContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка API: {response.StatusCode}, {errorContent}");
                    throw new Exception($"API вернул ошибку: {response.StatusCode}, {errorContent}");
                }

                return await response.Content.ReadFromJsonAsync<MaterialDto>(_jsonOptions);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                throw;
            }
        }

        public async Task<MaterialCategoryDto> CreateCategoryAsync(MaterialCategoryDto categoryDto)
        {
            var response = await _httpClient.PostAsJsonAsync("api/materials/categories", categoryDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MaterialCategoryDto>(_jsonOptions);
        }

        public async Task DeleteCategoryAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/materials/categories/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteMaterialAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/materials/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<MaterialCategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialCategoryDto>>("api/materials/categories", _jsonOptions);
            return categories ?? Enumerable.Empty<MaterialCategoryDto>();
        }

        public async Task<IEnumerable<MaterialDto>> GetAllMaterialsAsync()
        {
            var materials = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialDto>>("api/materials", _jsonOptions);
            return materials ?? Enumerable.Empty<MaterialDto>();
        }

        public async Task<MaterialCategoryDto> GetCategoryByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<MaterialCategoryDto>($"api/materials/categories/{id}", _jsonOptions);
        }

        public async Task<MaterialDto> GetMaterialByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<MaterialDto>($"api/materials/{id}", _jsonOptions);
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsByCategoryAsync(Guid categoryId, bool includeSubcategories = false)
        {
            var materials = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialDto>>(
                $"api/materials/categories/{categoryId}/materials?includeSubcategories={includeSubcategories}", _jsonOptions);
            return materials ?? Enumerable.Empty<MaterialDto>();
        }

        public async Task<IEnumerable<MaterialDto>> GetMaterialsWithLowStockAsync()
        {
            var materials = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialDto>>("api/materials/low-stock", _jsonOptions);
            return materials ?? Enumerable.Empty<MaterialDto>();
        }

        public async Task<IEnumerable<MaterialCategoryDto>> GetSubcategoriesAsync(Guid parentCategoryId)
        {
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialCategoryDto>>(
                $"api/materials/categories/{parentCategoryId}/subcategories", _jsonOptions);
            return categories ?? Enumerable.Empty<MaterialCategoryDto>();
        }

        public async Task<IEnumerable<MaterialCategoryDto>> GetTopLevelCategoriesAsync()
        {
            var categories = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialCategoryDto>>(
                "api/materials/categories/top-level", _jsonOptions);
            return categories ?? Enumerable.Empty<MaterialCategoryDto>();
        }

        public async Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var transactions = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialTransactionDto>>(
                $"api/materials/transactions?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}", _jsonOptions);
            return transactions ?? Enumerable.Empty<MaterialTransactionDto>();
        }

        public async Task<IEnumerable<MaterialTransactionDto>> GetTransactionsByMaterialAsync(Guid materialId)
        {
            var transactions = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialTransactionDto>>(
                $"api/materials/{materialId}/transactions", _jsonOptions);
            return transactions ?? Enumerable.Empty<MaterialTransactionDto>();
        }

        public async Task<bool> HasSufficientStockAsync(Guid materialId, decimal requiredQuantity)
        {
            var response = await _httpClient.GetFromJsonAsync<bool>(
                $"api/materials/{materialId}/check-stock?requiredQuantity={requiredQuantity}", _jsonOptions);
            return response;
        }

        public async Task<IEnumerable<MaterialDto>> SearchMaterialsAsync(string searchTerm)
        {
            var materials = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialDto>>(
                $"api/materials/search?searchTerm={Uri.EscapeDataString(searchTerm)}", _jsonOptions);
            return materials ?? Enumerable.Empty<MaterialDto>();
        }

        public async Task<MaterialCategoryDto> UpdateCategoryAsync(Guid id, MaterialCategoryDto categoryDto)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/materials/categories/{id}", categoryDto);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MaterialCategoryDto>(_jsonOptions);
        }

        public async Task<MaterialDto> UpdateMaterialAsync(Guid id, MaterialUpdateDto model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/materials/{id}", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MaterialDto>(_jsonOptions);
        }
        public async Task UpdateStockAsync(Guid materialId, decimal quantity, bool isAddition)
        {
            try
            {
                // Формируем запрос на обновление остатков материала
                var response = await _httpClient.PutAsync(
                    $"api/materials/{materialId}/stock?quantity={quantity}&isAddition={isAddition}",
                    null);

                // Проверка на ошибки
                if (!response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Ошибка при обновлении запасов: {response.StatusCode}, {content}");
                    throw new Exception($"Ошибка при обновлении запасов материала: {response.StatusCode}, {content}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Исключение при обновлении запасов материала: {ex.Message}");
                throw;
            }
        }
    }
}