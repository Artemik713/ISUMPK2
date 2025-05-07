using ISUMPK2.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Domain.Repositories
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User> GetByUsernameAsync(string username);
        Task<User> GetByEmailAsync(string email);
        Task<IEnumerable<string>> GetRolesAsync(Guid userId);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName);
        Task<IEnumerable<User>> GetUsersByDepartmentAsync(Guid departmentId);
        Task<bool> IsInRoleAsync(Guid userId, string roleName);
        Task AddToRoleAsync(Guid userId, string roleName);
        Task RemoveFromRoleAsync(Guid userId, string roleName);
    }
}
