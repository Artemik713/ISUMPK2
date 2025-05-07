using ISUMPK2.Domain.Entities;
using ISUMPK2.Domain.Repositories;
using ISUMPK2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace ISUMPK2.Infrastructure.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.NormalizedUserName == username.ToUpper());
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.NormalizedEmail == email.ToUpper());
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string roleName)
        {
            var normalizedRoleName = roleName.ToUpper();
            return await _context.Users
                .Where(u => u.UserRoles.Any(ur => ur.Role.NormalizedName == normalizedRoleName))
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetUsersByDepartmentAsync(Guid departmentId)
        {
            return await _dbSet.Where(u => u.DepartmentId == departmentId).ToListAsync();
        }

        public async Task<bool> IsInRoleAsync(Guid userId, string roleName)
        {
            var normalizedRoleName = roleName.ToUpper();
            return await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.Role.NormalizedName == normalizedRoleName);
        }

        public async Task AddToRoleAsync(Guid userId, string roleName)
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"User with ID {userId} not found");

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpper());
            if (role == null)
                throw new ArgumentException($"Role {roleName} not found");

            var userRole = new UserRole { UserId = userId, RoleId = role.Id };
            await _context.UserRoles.AddAsync(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveFromRoleAsync(Guid userId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpper());
            if (role == null)
                throw new ArgumentException($"Role {roleName} not found");

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return new List<string>();

            return user.UserRoles.Select(ur => ur.Role.Name);
        }

        public override async Task<User> GetByIdAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
