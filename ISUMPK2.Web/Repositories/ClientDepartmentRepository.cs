using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientDepartmentRepository : ClientRepositoryBase<Department>, IDepartmentRepository
    {
        protected override string ApiEndpoint => "api/departments";

        public ClientDepartmentRepository(HttpClient httpClient) : base(httpClient)
        {
        }

        // Реализация метода для получения департаментов с начальниками
        public async Task<IEnumerable<Department>> GetDepartmentsWithHeadAsync()
        {
            return await HttpClient.GetFromJsonAsync<IEnumerable<Department>>($"{ApiEndpoint}/with-heads");
        }

        // Реализация метода для получения департамента по имени
        public async Task<Department> GetDepartmentByNameAsync(string name)
        {
            return await HttpClient.GetFromJsonAsync<Department>($"{ApiEndpoint}/by-name/{Uri.EscapeDataString(name)}");
        }
    }
}
