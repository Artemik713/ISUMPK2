using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientProductRepository : ClientRepositoryBase<Product>, IProductRepository
    {
        protected override string ApiEndpoint => "api/products";

        public ClientProductRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task<IEnumerable<Product>> GetProductsByStatusAsync(string status)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<Product>>($"{ApiEndpoint}/status/{Uri.EscapeDataString(status)}");
        }

        public async Task<IEnumerable<ProductTransaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<ProductTransaction>>($"{ApiEndpoint}/transactions?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}");
        }

        public async Task<IEnumerable<ProductTransaction>> GetTransactionsByProductAsync(Guid productId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<ProductTransaction>>($"{ApiEndpoint}/{productId}/transactions");
        }

        public async Task UpdateStatusAsync(Guid productId, string newStatus)
        {
            await HttpClient.PutAsync($"{ApiEndpoint}/{productId}/status/{Uri.EscapeDataString(newStatus)}", null);
        }

        public async Task UpdateStockAsync(Guid productId, decimal quantity, bool isAddition)
        {
            await HttpClient.PutAsync($"{ApiEndpoint}/{productId}/stock?quantity={quantity}&isAddition={isAddition}", null);
        }

        public async Task<IEnumerable<Product>> GetProductsByTypeAsync(Guid typeId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<Product>>($"{ApiEndpoint}/type/{typeId}");
        }

        public async Task<IEnumerable<ProductMaterial>> GetProductMaterialsAsync(Guid productId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<ProductMaterial>>($"{ApiEndpoint}/{productId}/materials");
        }

        public async Task<bool> HasSufficientMaterialsForProductionAsync(Guid productId, decimal quantity)
        {
            return await HttpClient.GetFromJsonAsync<bool>($"{ApiEndpoint}/{productId}/check-materials/{quantity}");
        }
    }
}
