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

        // ISUMPK2.Infrastructure/Repositories/UserRepository.cs
        public async Task<bool> CheckRoleExistsAsync(string roleName)
        {
            return await _context.Roles.AnyAsync(r => r.NormalizedName == roleName.ToUpper());
        }

        public Task<ApplicationDbContext> GetContextAsync()
        {
            return Task.FromResult(_context);
        }

        public async Task<User> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return null;
            }

            var normalizedUsername = username.ToUpper();

            // Используем проекцию для избежания проблем с NULL значениями
            return await _dbSet
                .AsNoTracking()  // Оптимизация, если не планируете изменять объект
                .Where(u => u.NormalizedUserName == normalizedUsername)
                .Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    NormalizedUserName = u.NormalizedUserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    NormalizedEmail = u.NormalizedEmail ?? string.Empty,
                    EmailConfirmed = u.EmailConfirmed,
                    PasswordHash = u.PasswordHash ?? string.Empty,
                    SecurityStamp = u.SecurityStamp ?? string.Empty,
                    ConcurrencyStamp = u.ConcurrencyStamp ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    LockoutEnd = u.LockoutEnd,
                    LockoutEnabled = u.LockoutEnabled,
                    AccessFailedCount = u.AccessFailedCount,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    DepartmentId = u.DepartmentId,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();
        }

        public async Task<User> GetByEmailAsync(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            var normalizedEmail = email.ToUpper();

            // Используем проекцию для избежания проблем с NULL значениями
            return await _dbSet
                .AsNoTracking()  // Оптимизация, если не планируете изменять объект
                .Where(u => u.NormalizedEmail == normalizedEmail)
                .Select(u => new User
                {
                    Id = u.Id,
                    UserName = u.UserName ?? string.Empty,
                    NormalizedUserName = u.NormalizedUserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    NormalizedEmail = u.NormalizedEmail ?? string.Empty,
                    EmailConfirmed = u.EmailConfirmed,
                    PasswordHash = u.PasswordHash ?? string.Empty,
                    SecurityStamp = u.SecurityStamp ?? string.Empty,
                    ConcurrencyStamp = u.ConcurrencyStamp ?? string.Empty,
                    PhoneNumber = u.PhoneNumber,
                    PhoneNumberConfirmed = u.PhoneNumberConfirmed,
                    TwoFactorEnabled = u.TwoFactorEnabled,
                    LockoutEnd = u.LockoutEnd,
                    LockoutEnabled = u.LockoutEnabled,
                    AccessFailedCount = u.AccessFailedCount,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    MiddleName = u.MiddleName,
                    DepartmentId = u.DepartmentId,
                    CreatedAt = u.CreatedAt,
                    UpdatedAt = u.UpdatedAt
                })
                .FirstOrDefaultAsync();
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
            if (string.IsNullOrEmpty(roleName))
            {
                throw new ArgumentException("Имя роли не может быть пустым");
            }

            var user = await GetByIdAsync(userId);
            if (user == null)
                throw new ArgumentException($"Пользователь с ID {userId} не найден");

            // Проверяем, существует ли роль перед попыткой её добавления
            var normalizedRoleName = roleName.ToUpper();

            // Используем более безопасный запрос с дополнительными проверками
            var role = await _context.Roles
                .Where(r => r.NormalizedName == normalizedRoleName)
                .Select(r => new { r.Id, r.NormalizedName })
                .FirstOrDefaultAsync();

            if (role == null)
                throw new ArgumentException($"Роль {roleName} не найдена");

            // Проверяем, есть ли у пользователя уже эта роль
            var exists = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);

            if (exists)
                return; // Роль уже назначена - ничего не делаем

            // Создаем новую связь пользователь-роль
            var userRole = new UserRole { UserId = userId, RoleId = role.Id };
            await _context.UserRoles.AddAsync(userRole);
        }

        public async Task RemoveFromRoleAsync(Guid userId, string roleName)
        {
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == roleName.ToUpper());
            if (role == null)
                throw new ArgumentException($"Роль {roleName} не найдена");

            var userRole = await _context.UserRoles.FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == role.Id);
            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                // Удаляем вызов SaveChangesAsync здесь, это будет делать вызывающий код
            }
        }

        public async Task<IEnumerable<string>> GetRolesAsync(Guid userId)
        {
            // Напрямую получаем имена ролей без загрузки полного пользователя
            var roleNames = await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_context.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Name ?? string.Empty)
                .ToListAsync();

            return roleNames;
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
