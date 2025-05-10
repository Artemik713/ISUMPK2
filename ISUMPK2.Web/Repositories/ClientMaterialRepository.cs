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

        public ClientMaterialRepository(HttpClient httpClient) : base(httpClient)
        {
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
    }
}
