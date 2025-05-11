using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientMaterialCategoryRepository : IMaterialCategoryRepository
    {
        private readonly HttpClient _httpClient;
        private const string ApiEndpoint = "api/materials/categories";

        public ClientMaterialCategoryRepository(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task AddAsync(MaterialCategory entity)
        {
            await _httpClient.PostAsJsonAsync(ApiEndpoint, entity);
        }

        public async Task DeleteAsync(Guid id)
        {
            await _httpClient.DeleteAsync($"{ApiEndpoint}/{id}");
        }

        public async Task<IEnumerable<MaterialCategory>> FindAsync(System.Linq.Expressions.Expression<Func<MaterialCategory, bool>> predicate)
        {
            // В клиентском приложении мы не можем использовать LINQ выражения напрямую
            // Вместо этого получаем все категории и фильтруем их на клиенте
            var allCategories = await GetAllAsync();
            return allCategories.Where(predicate.Compile());
        }

        public async Task<IEnumerable<MaterialCategory>> GetAllAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MaterialCategory>>(ApiEndpoint);
        }

        public async Task<MaterialCategory> GetByIdAsync(Guid id)
        {
            return await _httpClient.GetFromJsonAsync<MaterialCategory>($"{ApiEndpoint}/{id}");
        }

        public async Task<IEnumerable<Material>> GetMaterialsByCategoryAsync(Guid categoryId, bool includeSubcategories = false)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<Material>>($"{ApiEndpoint}/{categoryId}/materials?includeSubcategories={includeSubcategories}");
        }

        public async Task<IEnumerable<MaterialCategory>> GetSubcategoriesAsync(Guid parentCategoryId)
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MaterialCategory>>($"{ApiEndpoint}/{parentCategoryId}/subcategories");
        }

        public async Task<IEnumerable<MaterialCategory>> GetTopLevelCategoriesAsync()
        {
            return await _httpClient.GetFromJsonAsync<IEnumerable<MaterialCategory>>($"{ApiEndpoint}/top-level");
        }

        public async Task SaveChangesAsync()
        {
            // В клиентской реализации этот метод не нужен, так как изменения сохраняются
            // сразу при выполнении соответствующих HTTP-запросов
        }

        public async Task UpdateAsync(MaterialCategory entity)
        {
            await _httpClient.PutAsJsonAsync($"{ApiEndpoint}/{entity.Id}", entity);
        }
    }
}
