using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public abstract class ClientRepositoryBase<T> : IRepository<T> where T : BaseEntity
    {
        protected readonly HttpClient HttpClient;
        protected abstract string ApiEndpoint { get; }

        protected ClientRepositoryBase(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await HttpClient.GetFromJsonAsync<T>($"{ApiEndpoint}/{id}");
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<T>>(ApiEndpoint);
        }

        public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            // В WebAssembly невозможно передать Expression<Func<T, bool>> напрямую.
            // Вместо этого вам нужно реализовать фильтрацию на стороне API.
            throw new NotImplementedException("Фильтрация должна быть реализована на стороне API.");
        }

        public virtual async Task AddAsync(T entity)
        {
            var response = await HttpClient.PostAsJsonAsync(ApiEndpoint, entity);
            response.EnsureSuccessStatusCode();
        }

        public virtual async Task UpdateAsync(T entity)
        {
            var response = await HttpClient.PutAsJsonAsync(ApiEndpoint, entity);
            response.EnsureSuccessStatusCode();
        }

        public virtual async Task DeleteAsync(Guid id)
        {
            var response = await HttpClient.DeleteAsync($"{ApiEndpoint}/{id}");
            response.EnsureSuccessStatusCode();
        }

        public Task SaveChangesAsync()
        {
            // API автоматически сохраняет изменения
            return Task.CompletedTask;
        }
    }
}
