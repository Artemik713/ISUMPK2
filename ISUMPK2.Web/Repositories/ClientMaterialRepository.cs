using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientMaterialRepository : ClientRepositoryBase<Material>, IMaterialRepository
    {
        protected override string ApiEndpoint => "api/materials";
        private readonly HttpClient _httpClient;

        public ClientMaterialRepository(HttpClient httpClient) : base(httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<Material>> GetMaterialsWithLowStockAsync()
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<Material>>($"{ApiEndpoint}/lowstock");
        }

        public async Task<bool> HasSufficientStockAsync(Guid materialId, decimal requiredQuantity)
        {
            return await HttpClient.GetFromJsonAsync<bool>($"{ApiEndpoint}/{materialId}/check-stock/{requiredQuantity}");
        }

        public async Task<IEnumerable<MaterialTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<MaterialTransaction>>($"{ApiEndpoint}/transactions?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        }

        public async Task<IEnumerable<MaterialTransaction>> GetTransactionsByMaterialAsync(Guid materialId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<MaterialTransaction>>($"{ApiEndpoint}/{materialId}/transactions");
        }

        public async Task UpdateStockAsync(Guid materialId, decimal quantity, bool isAddition)
        {
            await HttpClient.PutAsync($"{ApiEndpoint}/{materialId}/stock?quantity={quantity}&isAddition={isAddition}", null);
        }

        public async Task<IEnumerable<Material>> GetMaterialsByProductIdAsync(Guid productId)
        {
            var response = await _httpClient.GetAsync($"api/materials/product/{productId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<Material>>();
        }

        public async Task AddTransactionAsync(MaterialTransaction transaction)
        {
            // Настраиваем правильные значения TransactionType
            if (transaction.TransactionType == "Приход")
                transaction.TransactionType = "Receipt";
            else if (transaction.TransactionType == "Расход")
                transaction.TransactionType = "Issue";

            // Используем правильную точку API и формат данных
            var transactionDto = new
            {
                MaterialId = transaction.MaterialId,
                Quantity = transaction.Quantity,
                TransactionType = transaction.TransactionType,
                TaskId = transaction.TaskId,
                Notes = transaction.Notes
            };

            var response = await HttpClient.PostAsJsonAsync($"{ApiEndpoint}/{transaction.MaterialId}/transactions", transactionDto);

            // Подробный вывод ошибки для диагностики
            if (!response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Ошибка API {(int)response.StatusCode}: {responseContent}");
                Console.WriteLine($"Отправленные данные: MaterialId={transaction.MaterialId}, Type={transaction.TransactionType}, Quantity={transaction.Quantity}");
            }


            response.EnsureSuccessStatusCode();
        }
    }
}