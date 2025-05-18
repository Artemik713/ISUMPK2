using ISUMPK2.Web.Models;
using System.Net.Http.Json;

namespace ISUMPK2.Web.Services
{
    public class ClientMaterialService : IMaterialService
    {
        private readonly HttpClient _httpClient;

        public ClientMaterialService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<IEnumerable<MaterialModel>> GetMaterialsAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialModel>>("api/materials");
            return result ?? Enumerable.Empty<MaterialModel>();
        }

        public async Task<MaterialModel?> GetMaterialByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<MaterialModel>($"api/materials/{id}");
        }

        public async Task<MaterialModel> CreateMaterialAsync(MaterialModel model)
        {
            var response = await _httpClient.PostAsJsonAsync("api/materials", model);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<MaterialModel>();
        }

        public async Task UpdateMaterialAsync(MaterialModel model)
        {
            var response = await _httpClient.PutAsJsonAsync($"api/materials/{model.Id}", model);
            response.EnsureSuccessStatusCode();
        }

        public async Task DeleteMaterialAsync(Guid id)
        {
            var response = await _httpClient.DeleteAsync($"api/materials/{id}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<IEnumerable<MaterialModel>> GetMaterialsWithLowStockAsync()
        {
            var result = await _httpClient.GetFromJsonAsync<IEnumerable<MaterialModel>>("api/materials/low-stock");
            return result ?? Enumerable.Empty<MaterialModel>();
        }
    }
}