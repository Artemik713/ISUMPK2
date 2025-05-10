using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;

namespace ISUMPK2.Web.Repositories
{
    public class ClientRepository<T> : ClientRepositoryBase<T> where T : BaseEntity
    {
        protected override string ApiEndpoint => $"api/{typeof(T).Name.ToLower()}s";

        public ClientRepository(HttpClient httpClient) : base(httpClient)
        {
        }
    }
}
