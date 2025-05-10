using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientUserRepository : ClientRepositoryBase<User>, IUserRepository
    {
        protected override string ApiEndpoint => "api/users";

        public ClientUserRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        public async Task AddToRoleAsync(Guid userId, string roleName)
        {
            await HttpClient.PostAsync($"{ApiEndpoint}/{userId}/roles/{roleName}", null);
        }

        public async Task<bool> CheckRoleExistsAsync(string roleName)
        {
            return await HttpClient.GetFromJsonAsync<bool>($"api/roles/{roleName}/exists");
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await HttpClient.GetFromJsonAsync<User>($"{ApiEndpoint}/byemail/{Uri.EscapeDataString(email)}");
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await HttpClient.GetFromJsonAsync<User>($"{ApiEndpoint}/byusername/{Uri.EscapeDataString(username)}");
        }

        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<string>>($"{ApiEndpoint}/{userId}/roles");
        }

        public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(Guid departmentId)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<User>>($"{ApiEndpoint}/bydepartment/{departmentId}");
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<User>>($"{ApiEndpoint}/byrole/{Uri.EscapeDataString(roleName)}");
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
        {
            return await HttpClient.GetFromJsonAsync<bool>($"{ApiEndpoint}/{userId}/roles/{Uri.EscapeDataString(roleName)}/check");
        }

        public async Task RemoveFromRoleAsync(Guid userId, string roleName)
        {
            await HttpClient.DeleteAsync($"{ApiEndpoint}/{userId}/roles/{Uri.EscapeDataString(roleName)}");
        }
    }
}
