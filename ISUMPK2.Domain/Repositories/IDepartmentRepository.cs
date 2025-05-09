using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ISUMPK2.Domain.Repositories
{
    public interface IDepartmentRepository : IRepository<Department>
    {
        // Здесь можно добавить специфичные для департаментов методы
        // например:
        Task<IEnumerable<Department>> GetDepartmentsWithHeadAsync();
        Task<Department> GetDepartmentByNameAsync(string name);
    }
}
